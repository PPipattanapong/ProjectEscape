using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RotatePuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Puzzle UI")]
    public GameObject rotatePanel;
    public GameObject other;         // ตัวที่จะหมุนใน UI
    public GameObject og;            // ตำแหน่งเป้าหมาย
    public GameObject unlockObject;  // Object ใหม่ที่จะ active เมื่อผ่าน

    [Header("Accepted Items")]
    public string[] acceptedItems;   // รายชื่อไอเทมที่ใช้เปิดได้

    private Quaternion targetRotation;
    private bool isPanelActive;

    void Start()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(false);

        if (og != null)
            targetRotation = og.transform.rotation;
    }

    // ✅ รับไอเทมจาก InventorySlot
    public void OnItemUsed(string itemName)
    {
        Debug.Log("[RotatePuzzle] Received item: " + itemName);

        bool accept = (acceptedItems == null || acceptedItems.Length == 0);

        if (!accept)
        {
            foreach (var allowed in acceptedItems)
            {
                if (itemName == allowed)
                {
                    accept = true;
                    break;
                }
            }
        }

        if (accept)
        {
            Debug.Log("[RotatePuzzle] Correct item used → Opening panel");
            OpenPanel();
        }
        else
        {
            Debug.Log("[RotatePuzzle] Wrong item: " + itemName);
        }
    }

    public void OpenPanel()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(true);

        isPanelActive = true;
    }

    void Update()
    {
        if (isPanelActive && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerInsidePanel())
            {
                ClosePanel();
            }
        }
    }

    // ✅ ใช้ EventSystem ตรวจว่า pointer อยู่บน panel หรือ child
    private bool IsPointerInsidePanel()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            if (r.gameObject == rotatePanel || r.gameObject.transform.IsChildOf(rotatePanel.transform))
                return true;
        }
        return false;
    }

    public void RotateLeft() => Rotate(90);   // ⬅ ซ้าย = +90
    public void RotateRight() => Rotate(-90); // ➡ ขวา = -90

    void Rotate(float angle)
    {
        if (other == null || og == null) return;

        other.transform.Rotate(0, 0, angle);

        if (Quaternion.Angle(other.transform.rotation, targetRotation) < 1f)
        {
            Debug.Log("[RotatePuzzle] Puzzle Solved!");
            ClosePanel();

            // ✅ เปิด object ใหม่
            if (unlockObject != null)
                unlockObject.SetActive(true);

            // ✅ ทำให้ item "Rotate" หายจาก inventory
            InventorySlot[] slots = FindObjectsOfType<InventorySlot>();
            foreach (var slot in slots)
            {
                if (slot.currentItem != null && slot.currentItem.itemName == "Rotate")
                {
                    slot.gameObject.SetActive(false);
                    Debug.Log("[RotatePuzzle] Removed Rotate item from inventory");
                }
            }

            // ✅ Sync RotateInBG (ตัวนี้ติดสคริปต์ RotatePuzzle) ให้เหมือน other
            transform.rotation = other.transform.rotation;

            // ✅ เปลี่ยนสีเป็นขาว (ถ้ามี SpriteRenderer หรือ SpriteRenderer2D)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }
    }

    void ClosePanel()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(false);

        isPanelActive = false;
    }
}
