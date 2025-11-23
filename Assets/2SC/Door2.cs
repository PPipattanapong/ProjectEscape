using UnityEngine;
using System.Collections.Generic;

public class DoorController_Simple : MonoBehaviour, IItemReceiver
{
    [Header("Settings")]
    public string requiredItem = "Key";
    public GameObject unlockTarget;
    public GameObject objectToDisable;

    [Header("Tooltip To Remove On Unlock")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    [Header("Audio Settings")]
    public AudioSource keySwipeSound;        // เสียงตอนรูดคีย์การ์ด
    public AudioSource disableObjectSound;   // เสียงตอนปิด objectToDisable

    private bool unlocked = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        if (unlockTarget != null)
            unlockTarget.SetActive(false);
    }

    public void OnItemUsed(string itemName)
    {
        Debug.Log($"Door received item: {itemName}");

        if (unlocked) return;

        if (itemName == requiredItem)
        {
            Debug.Log("✅ Key fits! Door unlocked.");
            unlocked = true;

            // 🔊 เล่นเสียงรูดคีย์การ์ดทันที
            if (keySwipeSound != null)
                keySwipeSound.Play();

            if (doorCollider != null)
                doorCollider.enabled = false;

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // 🔊 เล่นเสียงตอน objectToDisable กำลังปิด
            if (objectToDisable != null)
            {
                if (disableObjectSound != null)
                    disableObjectSound.Play();

                objectToDisable.SetActive(false);
            }

            if (unlockTarget != null)
                unlockTarget.SetActive(true);

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
