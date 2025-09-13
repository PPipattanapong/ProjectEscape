using UnityEngine;
using TMPro;
using System.Collections;

public class MemoryPuzzle : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI displayText;
    [TextArea(3, 10)]
    public string storyWithNumbers;
    public TMP_InputField answerInput;

    [Header("Puzzle Settings")]
    public string correctAnswer = "742";

    [Header("Rewards")]
    public GameObject noteLeft;
    public LightController doorLight;
    public Sprite noteLeftIcon;
    private bool solved = false;

    void Start()
    {
        if (noteLeft != null)
            noteLeft.SetActive(false);
    }

    public void OpenPuzzle()
    {
        if (solved) return;

        gameObject.SetActive(true);

        if (displayText != null)
            displayText.text = storyWithNumbers;

        if (answerInput != null)
            answerInput.text = "";
    }

    public void TryAnswer()
    {
        if (solved) return;

        string playerInput = answerInput != null ? answerInput.text : "";

        if (playerInput == correctAnswer)
        {
            solved = true;
            if (noteLeft != null)
                noteLeft.SetActive(true);

            if (doorLight != null)
                doorLight.SetGreen();

            Debug.Log("Memory puzzle solved!");
            if (displayText != null)
                StartCoroutine(ShowCorrectAndClose("Correct!", 1f)); // ⏱ แค่ 1 วินาที
        }
        else
        {
            Debug.Log("Wrong answer: " + playerInput);
            if (displayText != null)
                StartCoroutine(ShowTemporaryMessage("Wrong code!", 3f));
        }
    }

    private IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        string originalText = storyWithNumbers;
        displayText.text = message;

        yield return new WaitForSeconds(duration);

        if (!solved)
            displayText.text = originalText;
    }

    private IEnumerator ShowCorrectAndClose(string message, float duration)
    {
        displayText.text = message;
        yield return new WaitForSeconds(duration);

        gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        if (!solved)
            gameObject.SetActive(false);
    }
}
