using UnityEngine;
using TMPro;

public class RoomCameraController : MonoBehaviour
{
    [Header("Room Positions")]
    public Transform leftPos;
    public Transform middlePos;
    public Transform rightPos;
    public float speed = 5f;

    [Header("UI")]
    public TextMeshProUGUI roomText; // 👉 UI TMP (ใน Canvas)

    private Transform target;

    void Start()
    {
        target = middlePos;
        UpdateRoomText();
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
    }

    public void GoRight()
    {
        if (target == middlePos) target = rightPos;
        else if (target == leftPos) target = middlePos;

        UpdateRoomText();
    }

    private void UpdateRoomText()
    {
        if (roomText == null) return;

        if (target == leftPos) roomText.text = "Left";
        else if (target == middlePos) roomText.text = "Middle";
        else if (target == rightPos) roomText.text = "Right";
    }
}
