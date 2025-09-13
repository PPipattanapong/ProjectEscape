using UnityEngine;

public class DoorController : MonoBehaviour, IItemReceiver
{
    public LightController leftLight;
    public LightController rightLight;
    public LightController centerLight;

    private bool keyInserted = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();
    }

    public void OnItemUsed(string itemName)
    {
        Debug.Log($"Door received item: {itemName}");

        if (itemName == "Key")
        {
            keyInserted = true;
            centerLight.SetGreen();
            CheckDoor();
        }
        else
        {
            Debug.Log("Door does not accept this item!");
        }
    }


    void CheckDoor()
    {
        Debug.Log($"CheckDoor → left:{leftLight.isGreen}, right:{rightLight.isGreen}, key:{keyInserted}");

        if (leftLight.isGreen && rightLight.isGreen && keyInserted)
        {
            Debug.Log("Door opened!");

            // ✅ Move door behind everything (sorting order)
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // ✅ Disable collider so it no longer blocks
            if (doorCollider != null)
                doorCollider.enabled = false;
        }
    }
}
