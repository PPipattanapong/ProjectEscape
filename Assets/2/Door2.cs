using UnityEngine;

public class DoorController_Simple : MonoBehaviour, IItemReceiver
{
    [Header("Settings")]
    public string requiredItem = "Key";      // 🗝 Item name needed to unlock the door
    public GameObject unlockTarget;          // Object to activate when unlocked
    public GameObject objectToDisable;       // Object to deactivate when unlocked

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
        }
        else
        {
            Debug.Log("❌ Wrong item. Door remains locked.");
        }
    }
}
