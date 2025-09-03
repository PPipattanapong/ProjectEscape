using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemPinSingleText : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pinPanel;              // The panel containing the PIN UI
    public TextMeshProUGUI pinDisplay;       // Displays both numbers and error messages
    public Button submitButton;              // Submit button

    [Header("Settings")]
    public string correctPin = "4567";       // Correct 4-digit PIN

    private string inputPin = "";
    private bool isClicked = false;

    void Start()
    {
        pinPanel.SetActive(false);
        pinDisplay.text = "";
        submitButton.onClick.AddListener(SubmitPin);
    }

    void Update()
    {
        // Close panel if clicked outside
        if (Input.GetMouseButtonDown(0))
        {
            if (pinPanel.activeSelf)
            {
                if (!IsPointerOverUI() && !IsPointerOverItem())
                {
                    ClosePanel();
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!isClicked)
        {
            OpenPanel();
        }
    }

    // Press a number button
    public void PressNumber(string number)
    {
        if (inputPin.Length < 4)
        {
            inputPin += number;
            pinDisplay.text = inputPin; // Display the numbers pressed
        }
    }

    // Check the PIN when pressing submit
    public void SubmitPin()
    {
        if (inputPin == correctPin)
        {
            gameObject.SetActive(false); // Item disappears
            ClosePanel();
        }
        else
        {
            pinDisplay.text = "Incorrect PIN. Try again!";
            inputPin = "";
        }
    }

    private void OpenPanel()
    {
        pinPanel.SetActive(true);
        inputPin = "";
        pinDisplay.text = "";
        isClicked = true;

        // ตำแหน่ง panel ใกล้เมาส์ตอนเปิดครั้งเดียว
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pinPanel.transform.parent as RectTransform,
            Input.mousePosition,
            null,
            out mousePos
        );
        pinPanel.GetComponent<RectTransform>().localPosition = mousePos + new Vector2(50, 50);
    }

    private void ClosePanel()
    {
        pinPanel.SetActive(false);
        inputPin = "";
        pinDisplay.text = "";
        isClicked = false;
    }

    // Check if pointer is over any UI
    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    // Check if pointer is over this item
    bool IsPointerOverItem()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        return hit != null && hit.gameObject == this.gameObject;
    }
}
