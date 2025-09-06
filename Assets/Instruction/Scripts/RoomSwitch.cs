using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviour
{
    [Header("Rooms")]
    public GameObject leftRoom;
    public GameObject middleRoom;
    public GameObject rightRoom;

    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;

    [Header("UI")]
    public TextMeshProUGUI roomText; // TextMeshPro UI that shows the current room

    private GameObject currentRoom;

    void Start()
    {
        // Start the game in the middle room
        OpenRoom(middleRoom);
    }

    public void GoLeft()
    {
        if (currentRoom == middleRoom)
            OpenRoom(leftRoom);   // From middle to left
        else if (currentRoom == rightRoom)
            OpenRoom(middleRoom); // From right back to middle
    }

    public void GoRight()
    {
        if (currentRoom == middleRoom)
            OpenRoom(rightRoom);  // From middle to right
        else if (currentRoom == leftRoom)
            OpenRoom(middleRoom); // From left back to middle
    }

    private void OpenRoom(GameObject roomToOpen)
    {
        // Close the previous room
        if (currentRoom != null)
            currentRoom.SetActive(false);

        // Open the new room
        roomToOpen.SetActive(true);
        currentRoom = roomToOpen;

        // Manage button visibility
        if (currentRoom == middleRoom)
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }
        else if (currentRoom == leftRoom)
        {
            leftButton.gameObject.SetActive(false);   // Can't go further left
            rightButton.gameObject.SetActive(true);   // Can go back to middle
        }
        else if (currentRoom == rightRoom)
        {
            leftButton.gameObject.SetActive(true);    // Can go back to middle
            rightButton.gameObject.SetActive(false);  // Can't go further right
        }

        // Update the room text display
        UpdateRoomText();
    }

    private void UpdateRoomText()
    {
        if (currentRoom == leftRoom)
            roomText.text = "Left";
        else if (currentRoom == middleRoom)
            roomText.text = "Middle";
        else if (currentRoom == rightRoom)
            roomText.text = "Right";
    }
}
