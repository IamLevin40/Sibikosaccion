using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
    public Image diceImage;
    public Sprite[] diceFaces;
    public Player player;
    public Button rollButton;
    public GameObject rollText;
    public GameObject editPropertyUI;

    public RectTransform diceTransform;
    public RectTransform startPosition;
    public RectTransform rollArea;

    public float launchSpeedMin = 200f;
    public float launchSpeedMax = 300f;
    public float damping = 0.98f;
    public float slowDamping = 0.7f;
    public float stopThreshold = 0.1f;

    private Vector2 velocity;
    private Vector2 dicePos;
    private bool isRolling = false;
    private Quaternion startRotation;
    private Vector3 startScale;
    private Vector3 rollScale;

    private void Start()
    {
        rollButton.onClick.AddListener(RollDice);
        rollButton.interactable = true;
        rollText.SetActive(true);

        // Initialize values
        startRotation = startPosition.rotation;
        startScale = diceTransform.localScale;
        rollScale = startScale * 0.5f;

        // Set dice at start
        diceTransform.position = startPosition.position;
        diceTransform.rotation = startRotation;
        diceTransform.localScale = startScale;
    }

    public void RollDice()
    {
        if (player.isMoving || isRolling) return;
        rollText.SetActive(false);
        editPropertyUI.SetActive(false);
        StartCoroutine(RollDicePhysics());
    }

    private IEnumerator RollDicePhysics()
    {
        isRolling = true;
        rollButton.interactable = false;

        // Animate move to roll area with scale & rotation
        yield return AnimateMoveToRollArea();

        // Initialize roll physics
        dicePos = diceTransform.position;
        velocity = Random.insideUnitCircle.normalized * Random.Range(launchSpeedMin, launchSpeedMax);

        float timeSinceLastFace = 0f;
        int currentFace = 0;

        // Roll with velocity-based face changing
        while (velocity.magnitude > stopThreshold)
        {
            MoveAndBounce();
            timeSinceLastFace += Time.deltaTime;

            float dynamicInterval = Mathf.Clamp(1f / velocity.magnitude, 0.02f, 0.25f);
            if (timeSinceLastFace >= dynamicInterval)
            {
                currentFace = Random.Range(0, diceFaces.Length);
                diceImage.sprite = diceFaces[currentFace];
                timeSinceLastFace = 0f;
            }

            velocity *= damping;
            yield return null;
        }

        // Slower final phase
        while (velocity.magnitude > 5f)
        {
            MoveAndBounce();
            timeSinceLastFace += Time.deltaTime;

            float dynamicInterval = Mathf.Clamp(1f / velocity.magnitude, 0.5f, 0.75f);
            if (timeSinceLastFace >= dynamicInterval)
            {
                currentFace = Random.Range(0, diceFaces.Length);
                diceImage.sprite = diceFaces[currentFace];
                timeSinceLastFace = 0f;
            }

            velocity *= slowDamping;
            yield return null;
        }

        // Final result
        int finalFace = Random.Range(0, diceFaces.Length);
        diceImage.sprite = diceFaces[finalFace];

        yield return new WaitForSeconds(1f);

        // Return to start
        yield return MoveBackToStart(finalFace + 1);

        isRolling = false;
    }

    private void MoveAndBounce()
    {
        dicePos += velocity * Time.deltaTime;

        Vector3 localPos = rollArea.InverseTransformPoint(dicePos);
        Vector3 halfSize = rollArea.rect.size / 2f;

        float diceWidth = diceTransform.rect.width * diceTransform.localScale.x / 2f;
        float diceHeight = diceTransform.rect.height * diceTransform.localScale.y / 2f;

        bool bouncedX = false;
        bool bouncedY = false;

        if (localPos.x + diceWidth > halfSize.x || localPos.x - diceWidth < -halfSize.x)
        {
            velocity.x *= -1;
            bouncedX = true;
            localPos.x = Mathf.Clamp(localPos.x, -halfSize.x + diceWidth, halfSize.x - diceWidth);
        }

        if (localPos.y + diceHeight > halfSize.y || localPos.y - diceHeight < -halfSize.y)
        {
            velocity.y *= -1;
            bouncedY = true;
            localPos.y = Mathf.Clamp(localPos.y, -halfSize.y + diceHeight, halfSize.y - diceHeight);
        }

        dicePos = rollArea.TransformPoint(localPos);
        diceTransform.position = dicePos;

        // Apply rotation based on bounce
        if (bouncedX || bouncedY)
        {
            float spin = (bouncedX ? velocity.x : 0f) + (bouncedY ? velocity.y : 0f);
            diceTransform.Rotate(0, 0, spin * 0.05f);
        }
    }

    private IEnumerator AnimateMoveToRollArea()
    {
        Vector3 start = startPosition.position;
        Quaternion startRot = startPosition.rotation;
        Vector3 target = GetCenterOfRect(rollArea);
        Quaternion targetRot = Quaternion.identity;

        float duration = 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            Vector3 pos = Vector3.Lerp(start, target, t);
            diceTransform.position = pos;

            // Scale: peak at middle of transition
            float scaleT = t < 0.5f ? (t / 0.5f) : (1f - (t - 0.5f) / 0.5f);
            Vector3 scale = Vector3.Lerp(startScale, rollScale, scaleT);
            diceTransform.localScale = scale;

            // Rotation: gradually to 0
            diceTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            yield return null;
        }

        // Snap to target
        diceTransform.position = target;
        diceTransform.localScale = rollScale;
        diceTransform.rotation = targetRot;
    }

    private IEnumerator MoveBackToStart(int result)
    {
        Vector3 start = diceTransform.position;
        Quaternion currentRot = diceTransform.rotation;
        Vector3 end = startPosition.position;
        Quaternion endRot = startRotation;

        float duration = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3); // Cubic ease-out

            diceTransform.position = Vector3.Lerp(start, end, easeOut);
            diceTransform.localScale = Vector3.Lerp(rollScale, startScale, easeOut);
            diceTransform.rotation = Quaternion.Slerp(currentRot, endRot, easeOut);
            yield return null;
        }

        // Final snap
        diceTransform.position = end;
        diceTransform.localScale = startScale;
        diceTransform.rotation = endRot;

        player.Move(result);
    }

    private Vector3 GetCenterOfRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return (corners[0] + corners[2]) / 2f;
    }
}
