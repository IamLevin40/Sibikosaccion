using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
    [Header("Dice Properties")]
    public Image diceImage;
    public Sprite[] diceFaces;
    public Player player;
    public Button rollButton;
    public GameObject rollText;
    public GameObject editPropertyUI;

    [Header("Dice Animation")]
    public RectTransform diceTransform;
    public RectTransform startPosition;
    public RectTransform rollArea;

    [Header("Physics Properties")]
    public float minLaunchSpeed = 200f;
    public float maxLaunchSpeed = 300f;
    public float damping = 0.98f;
    public float slowDamping = 0.7f;
    public float stopThreshold = 0.1f;

    private Vector2 velocity;
    private Vector2 dicePosition;
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
        dicePosition = diceTransform.position;
        velocity = Random.insideUnitCircle.normalized * Random.Range(minLaunchSpeed, maxLaunchSpeed);

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
        int finalFace = currentFace;
        diceImage.sprite = diceFaces[finalFace];

        yield return new WaitForSeconds(1f);

        // Return to start
        yield return MoveBackToStart(finalFace + 1);

        isRolling = false;
    }

    private void MoveAndBounce()
    {
        dicePosition += velocity * Time.deltaTime;

        Vector3 localPosition = rollArea.InverseTransformPoint(dicePosition);
        Vector3 halfSize = rollArea.rect.size / 2f;

        float diceWidth = diceTransform.rect.width * diceTransform.localScale.x / 2f;
        float diceHeight = diceTransform.rect.height * diceTransform.localScale.y / 2f;

        bool bouncedX = false;
        bool bouncedY = false;

        if (localPosition.x + diceWidth > halfSize.x || localPosition.x - diceWidth < -halfSize.x)
        {
            velocity.x *= -1;
            bouncedX = true;
            localPosition.x = Mathf.Clamp(localPosition.x, -halfSize.x + diceWidth, halfSize.x - diceWidth);
        }

        if (localPosition.y + diceHeight > halfSize.y || localPosition.y - diceHeight < -halfSize.y)
        {
            velocity.y *= -1;
            bouncedY = true;
            localPosition.y = Mathf.Clamp(localPosition.y, -halfSize.y + diceHeight, halfSize.y - diceHeight);
        }

        dicePosition = rollArea.TransformPoint(localPosition);
        diceTransform.position = dicePosition;

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
        Quaternion startRotation = startPosition.rotation;
        Vector3 target = GetCenterOfRect(rollArea);
        Quaternion targetRotation = Quaternion.identity;

        float duration = 0.2f;
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
            diceTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Snap to target
        diceTransform.position = target;
        diceTransform.localScale = rollScale;
        diceTransform.rotation = targetRotation;
    }

    private IEnumerator MoveBackToStart(int result)
    {
        Vector3 start = diceTransform.position;
        Quaternion currentRotation = diceTransform.rotation;
        Vector3 end = startPosition.position;
        Quaternion endRotation = startRotation;

        float duration = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3);

            diceTransform.position = Vector3.Lerp(start, end, easeOut);
            diceTransform.localScale = Vector3.Lerp(rollScale, startScale, easeOut);
            diceTransform.rotation = Quaternion.Slerp(currentRotation, endRotation, easeOut);
            yield return null;
        }

        // Final snap
        diceTransform.position = end;
        diceTransform.localScale = startScale;
        diceTransform.rotation = endRotation;

        player.Move(result);
    }

    private Vector3 GetCenterOfRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return (corners[0] + corners[2]) / 2f;
    }
}
