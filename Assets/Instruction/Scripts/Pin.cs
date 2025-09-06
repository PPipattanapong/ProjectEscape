using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemPinSingleText : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pinPanel;              // Panel PIN (Panel 1)
    public TextMeshProUGUI pinDisplay;       // Displays both numbers and error messages
    public Button submitButton;              // Submit button

    [Header("Extra Panel")]
    public GameObject otherPanel;            // Panel 2 ที่จะโชว์แทน
    public Button button1;                   // ปุ่มแรกใน panel
    public Button button2;                   // ปุ่มสองใน panel
    public TextMeshProUGUI targetText;       // TMP ที่จะโชว์ข้อความจาก Inspector
    [TextArea] public string message;        // ข้อความจาก Inspector

    [Header("Settings")]
    public string correctPin = "4567";       // Correct 4-digit PIN

    private string inputPin = "";
    private bool isClicked = false;

    void Start()
    {
        pinPanel.SetActive(false);
        if (otherPanel != null) otherPanel.SetActive(false); // ปิด panel 2 ตอนเริ่มเกม
        pinDisplay.text = "";
        submitButton.onClick.AddListener(SubmitPin);

        // ปุ่มแรก → ใส่ข้อความลงใน target TMP
        if (button1 != null)
        {
            button1.onClick.AddListener(() =>
            {
                if (targetText != null)
                    targetText.text = message;
            });
        }

        // ปุ่มสอง → ปิด pinPanel แล้วเปิด otherPanel
        if (button2 != null)
        {
            button2.onClick.AddListener(() =>
            {
                ClosePanel();
                if (otherPanel != null)
                    otherPanel.SetActive(true);
            });
        }
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

        // ตำแหน่ง panel ใกล้เมาส์
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
