using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour
{
    public Image diceImage;
    public Sprite[] diceFaces;
    public Player player;
    public Button rollButton;
    public Transform diceTransform;

    private void Start()
    {
        rollButton.onClick.AddListener(RollDice);
        rollButton.interactable = true;
    }

    public void RollDice()
    {
        if (player.isMoving) return;
        StartCoroutine(RollDiceAnimation());
    }

    private IEnumerator RollDiceAnimation()
    {
        rollButton.interactable = false;
        float scaleDuration = 0.2f;
        float rollDuration = 1.0f;
        
        // scale up effect
        Vector3 originalScale = diceTransform.localScale;
        Vector3 enlargedScale = originalScale * 1.2f;
        float timer = 0f;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            diceTransform.localScale = Vector3.Lerp(originalScale, enlargedScale, timer / scaleDuration);
            yield return null;
        }
        
        // rolling effect
        timer = 0f;
        int randomFace = 0;
        while (timer < rollDuration)
        {
            timer += Time.deltaTime;
            randomFace = Random.Range(0, diceFaces.Length);
            diceImage.sprite = diceFaces[randomFace];
            yield return new WaitForSeconds(0.05f);
        }
        
        // final dice result
        int finalResult = Random.Range(0, diceFaces.Length);
        diceImage.sprite = diceFaces[finalResult];
        
        // scale down effect
        timer = 0f;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            diceTransform.localScale = Vector3.Lerp(enlargedScale, originalScale, timer / scaleDuration);
            yield return null;
        }
        
        // move the player
        player.Move(finalResult + 1);
    }
}