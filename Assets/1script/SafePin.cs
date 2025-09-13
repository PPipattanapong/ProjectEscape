using UnityEngine;
using TMPro;
using System.Collections;

public class SafePin : MonoBehaviour
{
    [Header("Safe Settings")]
    public string correctPin = "4567";
    public TextMeshProUGUI pinDisplay;
    public GameObject keyReward;

    [Header("UI Panel")]
    public GameObject safePanel;   // drag your SafePanel here

    private string input = "";
    private bool solved = false;
    private SpriteRenderer safeRenderer;
    public LightController centerLight; // set in Inspector

    void Start()
    {
        if (keyReward != null) keyReward.SetActive(false);
        if (pinDisplay != null) pinDisplay.text = "";
        if (safePanel != null) safePanel.SetActive(false); // hide panel at start

        safeRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (!solved && safePanel != null)
            safePanel.SetActive(true); // open panel on click
    }

    public void PressNumber(string num)
    {
        if (solved) return;
        input += num;
        pinDisplay.text = input;
    }

    public void Submit()
    {
        if (solved) return;

        if (input == correctPin &&
            InventoryManager.Instance.HasItem("NoteCombined"))
        {
            solved = true;

            if (keyReward != null) keyReward.SetActive(true);

            Debug.Log("Safe opened!");

            if (safeRenderer != null)
                safeRenderer.sortingOrder = 0;

            if (centerLight != null)
                centerLight.SetGreen();

            // ✅ Show "Correct!" then close after 1s
            if (pinDisplay != null)
                StartCoroutine(ShowCorrectAndClose("Correct!", 1f));

            input = "";
        }
        // case: correct code but requirements missing
        else if (input == correctPin)
        {
            pinDisplay.text = "Requirements not passed!";
            input = "";
        }
        // case: wrong code
        else
        {
            pinDisplay.text = "Wrong code!";
            input = "";
        }
    }

    private IEnumerator ShowCorrectAndClose(string message, float duration)
    {
        pinDisplay.text = message;
        yield return new WaitForSeconds(duration);

        if (safePanel != null)
            safePanel.SetActive(false);
    }

    public void ClosePanel()
    {
        if (safePanel != null)
            safePanel.SetActive(false);
    }
}
