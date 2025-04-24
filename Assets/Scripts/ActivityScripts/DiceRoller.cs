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

    [Header("Admin Mode")]
    public bool isAdmin = false;

    private Vector2 velocity;
    private Vector2 dicePosition;
    private bool isRolling = false;
    private Quaternion startRotation;
    private Vector3 startScale;
    private Vector3 rollScale;

    private int[] adminRollSequence  = { 4, 6, 3, 5, 2, 1, 3, 5, 4, 3, 4, 2, 4 };
    private int currentAdminRollIndex  = 0;

    private void Start()
    {
        rollButton.onClick.AddListener(RollDice);
        rollButton.interactable = true;
        rollText.SetActive(true);

        startRotation = startPosition.rotation;
        startScale = diceTransform.localScale;
        rollScale = startScale * 0.5f;

        diceTransform.position = startPosition.position;
        diceTransform.rotation = startRotation;
        diceTransform.localScale = startScale;
    }

    public void RollDice()
    {
        if (player.isMoving || isRolling) return;
        rollText.SetActive(false);
        editPropertyUI.SetActive(false);
        player.editPropertyUI.showStatsPropertyManager.ClearUI();
        StartCoroutine(RollDicePhysics());
    }

    private IEnumerator RollDicePhysics()
    {
        isRolling = true;
        rollButton.interactable = false;

        yield return AnimateMoveToRollArea();

        dicePosition = diceTransform.position;
        velocity = Random.insideUnitCircle.normalized * Random.Range(minLaunchSpeed, maxLaunchSpeed);

        float timeSinceLastFace = 0f;
        int currentFace = 0;

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

        if (isAdmin)
        {
            currentFace = adminRollSequence[currentAdminRollIndex] - 1;
            currentAdminRollIndex = (currentAdminRollIndex + 1) % adminRollSequence.Length;
            Debug.Log("Admin Roll: " + currentFace);
        }

        int finalFace = currentFace;
        diceImage.sprite = diceFaces[finalFace];

        yield return new WaitForSeconds(1f);
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

        if (bouncedX || bouncedY)
        {
            float spinValue = (bouncedX ? velocity.x : 0f) + (bouncedY ? velocity.y : 0f);
            diceTransform.Rotate(0, 0, spinValue * 0.05f);
        }
    }

    private IEnumerator AnimateMoveToRollArea()
    {
        Vector3 startPos = startPosition.position;
        Quaternion startRotation = startPosition.rotation;
        Vector3 targetPos = GetCenterOfRect(rollArea);
        Quaternion targetRotation = Quaternion.identity;

        float duration = 0.2f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            Vector3 position = Vector3.Lerp(startPos, targetPos, t);
            diceTransform.position = position;

            float scaleT = t < 0.5f ? (t / 0.5f) : (1f - (t - 0.5f) / 0.5f);
            Vector3 scale = Vector3.Lerp(startScale, rollScale, scaleT);
            diceTransform.localScale = scale;

            diceTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        diceTransform.position = targetPos;
        diceTransform.localScale = rollScale;
        diceTransform.rotation = targetRotation;
    }

    private IEnumerator MoveBackToStart(int result)
    {
        Vector3 startPos = diceTransform.position;
        Quaternion currentRotation = diceTransform.rotation;
        Vector3 endPos = startPosition.position;
        Quaternion endRotation = startRotation;

        float duration = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3);

            diceTransform.position = Vector3.Lerp(startPos, endPos, easeOut);
            diceTransform.localScale = Vector3.Lerp(rollScale, startScale, easeOut);
            diceTransform.rotation = Quaternion.Slerp(currentRotation, endRotation, easeOut);
            yield return null;
        }

        diceTransform.position = endPos;
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
