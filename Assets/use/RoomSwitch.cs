using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    [Header("Rooms")]
    public GameObject leftRoom;
    public GameObject middleRoom;
    public GameObject rightRoom;

    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;

    private GameObject currentRoom;

    void Start()
    {
        // เริ่มเกมที่ห้องกลาง
        OpenRoom(middleRoom);
    }

    public void GoLeft()
    {
        if (currentRoom == middleRoom)
            OpenRoom(leftRoom);   // จากกลางไปซ้าย
        else if (currentRoom == rightRoom)
            OpenRoom(middleRoom); // จากขวากลับกลาง
    }

    public void GoRight()
    {
        if (currentRoom == middleRoom)
            OpenRoom(rightRoom);  // จากกลางไปขวา
        else if (currentRoom == leftRoom)
            OpenRoom(middleRoom); // จากซ้ายกลับกลาง
    }

    private void OpenRoom(GameObject roomToOpen)
    {
        // ปิดห้องเก่า
        if (currentRoom != null)
            currentRoom.SetActive(false);

        // เปิดห้องใหม่
        roomToOpen.SetActive(true);
        currentRoom = roomToOpen;

        // จัดการปุ่ม
        if (currentRoom == middleRoom)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else if (currentRoom == leftRoom)
        {
            leftButton.gameObject.SetActive(false);   // ไม่สามารถไปซ้ายได้
            rightButton.gameObject.SetActive(true);   // กลับกลางได้
        }
        else if (currentRoom == rightRoom)
        {
            leftButton.gameObject.SetActive(true);    // กลับกลางได้
            rightButton.gameObject.SetActive(false);  // ไม่สามารถไปขวาได้
        }
    }
}
