using UnityEngine;
using System.Collections.Generic;

public class DoorController_Simple : MonoBehaviour, IItemReceiver
{
    [Header("Settings")]
    public string requiredItem = "Key";      // 🗝 Item name needed to unlock the door
    public GameObject unlockTarget;          // Object to activate when unlocked
    public GameObject objectToDisable;       // Object to deactivate when unlocked

    [Header("Tooltip To Remove On Unlock")]
    [Tooltip("ใส่วัตถุที่มี Tooltip ที่ต้องลบทิ้งเมื่อประตูปลดล็อก")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    private bool unlocked = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // Hide unlockTarget at start
        if (unlockTarget != null)
            unlockTarget.SetActive(false);
    }

    public void OnItemUsed(string itemName)
    {
        Debug.Log($"Door received item: {itemName}");

        if (unlocked) return; // Already unlocked

        if (itemName == requiredItem)
        {
            Debug.Log("✅ Key fits! Door unlocked.");
            unlocked = true;

            // Disable collider (door no longer blocks)
            if (doorCollider != null)
                doorCollider.enabled = false;

            // Lower sorting order (door moves behind other objects)
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // Disable another object if assigned
            if (objectToDisable != null)
                objectToDisable.SetActive(false);

            // Activate target object immediately
            if (unlockTarget != null)
                unlockTarget.SetActive(true);

            // ⭐ ลบ Tooltip ทั้งหมดที่กำหนด
            foreach (var obj in objectsToRemoveTooltip)
            {
                if (obj == null) continue;

                Tooltip t = obj.GetComponent<Tooltip>();
                if (t != null)
                {
                    Destroy(t);
                    Debug.Log("[DoorController_Simple] Removed Tooltip from: " + obj.name);
                }
            }
        }
        else
        {
            Debug.Log("❌ Wrong item. Door remains locked.");
        }
    }
}
