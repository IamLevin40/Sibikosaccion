using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MysteryCardManager : MonoBehaviour
{
    [Header("Card Selection")]
    public GameObject cardSelectionUI;
    public Button[] cardButtons;
    public Image[] cardImages;
    public List<MysteryCardData> allCards;

    [Header("Card Details")]
    public GameObject cardDetailsUI;
    public Image selectedCardImage;
    public Text selectedCardName;
    public Text selectedCardDescription;
    public Button confirmCardButton;
    public Button rerollButton;

    [Header("Quiz System")]
    public GameObject questionsUI;
    public Text questionText;
    public Button[] optionButtons;
    public GameObject questionMessageUI;
    public GameObject correctMessage;
    public GameObject incorrectMessage;
    public List<QuestionData> allQuestions;

    private Player currentPlayer;
    private MysteryCardData[] drawnCards = new MysteryCardData[3];
    private MysteryCardData currentlySelectedCard;
    private bool hasRerolled = false;
    private bool cardAlreadySelected = false;
    private bool isAnimating = false;

    public void ShowMysteryCards(Player player)
    {
        currentPlayer = player;
        hasRerolled = false;
        cardAlreadySelected = false;
        DrawUniqueCards(initialDraw: true);

        cardSelectionUI.SetActive(true);
        rerollButton.gameObject.SetActive(false);

        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(() =>
        {
            ShowQuestion();
        });

        confirmCardButton.onClick.RemoveAllListeners();
        confirmCardButton.onClick.AddListener(() =>
        {
            if (currentlySelectedCard != null)
            {
                currentlySelectedCard.ActivateEffect(currentPlayer, this);
                cardSelectionUI.SetActive(false);
                cardDetailsUI.SetActive(false);
                rerollButton.gameObject.SetActive(false);
                currentPlayer.dice.rollButton.interactable = true;
            }
        });
    }

    private void DrawUniqueCards(bool initialDraw)
    {
        List<MysteryCardData> corruptCards = allCards.Where(c => c.cardType == CardType.Corrupt).ToList();
        List<MysteryCardData> nonCorruptCards = allCards.Where(c => c.cardType == CardType.NonCorrupt).ToList();

        HashSet<MysteryCardData> selected = new HashSet<MysteryCardData>();

        int tries = 0;
        while (selected.Count < 3 && tries < 100)
        {
            tries++;

            MysteryCardData card = null;
            if (initialDraw)
            {
                card = (Random.value <= 0.8f)
                    ? corruptCards[Random.Range(0, corruptCards.Count)]
                    : nonCorruptCards[Random.Range(0, nonCorruptCards.Count)];
            }
            else
            {
                if (selected.Count == 0)
                    card = nonCorruptCards[Random.Range(0, nonCorruptCards.Count)];
                else
                    card = allCards[Random.Range(0, allCards.Count)];
            }

            selected.Add(card);
        }

        drawnCards = selected.ToArray();

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            cardImages[i].sprite = drawnCards[i].backImage;
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => {
                if (!isAnimating && !cardAlreadySelected)
                {
                    StartCoroutine(FlipCard(index));
                }
            });
        }
    }

    private IEnumerator FlipCard(int index)
    {
        isAnimating = true;
        cardDetailsUI.SetActive(false);

        GameObject cardObject = cardImages[index].transform.parent.gameObject;
        Image cardImage = cardImages[index];

        float duration = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            float yRotation = Mathf.Lerp(0f, 90f, time / duration);
            cardObject.transform.rotation = Quaternion.Euler(0, yRotation, cardObject.transform.eulerAngles.z);
            time += Time.deltaTime;
            yield return null;
        }

        cardImage.sprite = drawnCards[index].frontImage;

        time = 0f;
        while (time < duration)
        {
            float yRotation = Mathf.Lerp(90f, 0f, time / duration);
            cardObject.transform.rotation = Quaternion.Euler(0, yRotation, cardObject.transform.eulerAngles.z);
            time += Time.deltaTime;
            yield return null;
        }

        cardAlreadySelected = true;
        PreviewCard(index);

        isAnimating = false;
    }

    private void PreviewCard(int index)
    {
        rerollButton.gameObject.SetActive(true);
        cardDetailsUI.SetActive(true);
        currentlySelectedCard = drawnCards[index];

        selectedCardImage.sprite = currentlySelectedCard.frontImage;
        selectedCardName.text = currentlySelectedCard.cardName;
        selectedCardDescription.text = currentlySelectedCard.description;
    }

    private void ShowQuestion()
    {
        if (allQuestions.Count == 0) return;

        QuestionData question = allQuestions[Random.Range(0, allQuestions.Count)];

        cardSelectionUI.SetActive(false);
        cardDetailsUI.SetActive(false);
        cardAlreadySelected = false;

        questionsUI.SetActive(true);
        questionText.text = question.question;

        List<string> allOptions = new List<string>(question.incorrectOptions);
        allOptions.Add(question.correctOption);

        for (int i = 0; i < allOptions.Count; i++)
        {
            string temp = allOptions[i];
            int randomIndex = Random.Range(i, allOptions.Count);
            allOptions[i] = allOptions[randomIndex];
            allOptions[randomIndex] = temp;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].GetComponentInChildren<Text>().text = allOptions[i];
            optionButtons[i].onClick.RemoveAllListeners();

            if (allOptions[i] == question.correctOption)
            {
                optionButtons[i].onClick.AddListener(() =>
                {
                    questionsUI.SetActive(false);
                    cardSelectionUI.SetActive(true);
                    rerollButton.gameObject.SetActive(false);
                    hasRerolled = true;
                    DrawUniqueCards(initialDraw: false);
                    StartCoroutine(DisplayQuestionMessage(true));
                });
            }
            else
            {
                optionButtons[i].onClick.AddListener(() =>
                {
                    questionsUI.SetActive(false);
                    cardSelectionUI.SetActive(false);
                    cardDetailsUI.SetActive(false);
                    rerollButton.gameObject.SetActive(false);
                    currentPlayer.dice.rollButton.interactable = true;
                    StartCoroutine(DisplayQuestionMessage(false));
                });
            }
        }
    }

    private IEnumerator DisplayQuestionMessage(bool isCorrect)
    {
        questionMessageUI.SetActive(true);
        correctMessage.SetActive(isCorrect);
        incorrectMessage.SetActive(!isCorrect);

        yield return new WaitForSeconds(2f);
        questionMessageUI.SetActive(false);
        correctMessage.SetActive(false);
        incorrectMessage.SetActive(false);
    }
}
