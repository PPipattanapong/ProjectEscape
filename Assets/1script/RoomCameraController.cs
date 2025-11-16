using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomCameraController : MonoBehaviour
{
    [Header("Room Positions")]
    public Transform leftPos;
    public Transform middlePos;
    public Transform rightPos;
    public float speed = 5f;

    [Header("UI")]
    public TextMeshProUGUI roomText;

    [Header("Buttons")]
    public GameObject leftButton;   // ปุ่มเดินซ้าย
    public GameObject rightButton;  // ปุ่มเดินขวา

    private Transform target;

    void Start()
    {
        target = middlePos;
        UpdateRoomText();
        UpdateButtons();
    }

    void Update()
    {
        if (target != null)
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }

    public void GoLeft()
    {
        if (target == middlePos) target = leftPos;
        else if (target == rightPos) target = middlePos;

        UpdateRoomText();
        UpdateButtons();
    }

    public void GoRight()
    {
        if (target == middlePos) target = rightPos;
        else if (target == leftPos) target = middlePos;

        UpdateRoomText();
        UpdateButtons();
    }

    private void UpdateRoomText()
    {
        if (!roomText) return;

        if (target == leftPos) roomText.text = "LEFT";
        else if (target == middlePos) roomText.text = "MIDDLE";
        else if (target == rightPos) roomText.text = "RIGHT";
    }

    private void UpdateButtons()
    {
        if (!leftButton || !rightButton) return;

        // อยู่ซ้ายสุด → ซ่อนปุ่มซ้าย
        if (target == leftPos)
        {
            leftButton.SetActive(false);
            rightButton.SetActive(true);
        }
        // อยู่ขวาสุด → ซ่อนปุ่มขวา
        else if (target == rightPos)
        {
            leftButton.SetActive(true);
            rightButton.SetActive(false);
        }
        // อยู่กลาง → ปุ่มออกครบ
        else
        {
            leftButton.SetActive(true);
            rightButton.SetActive(true);
        }
    }
}
