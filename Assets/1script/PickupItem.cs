using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string clueText; // clue ของ item (เช่น First two numbers are 4 and 5)

    private bool collected = false;

    private void OnMouseDown()
    {
        if (collected) return; // ป้องกันกดซ้ำ
        if (!Input.GetMouseButtonDown(0)) return; // ต้องกดคลิกซ้าย

        collected = true;

        // เพิ่มเข้า inventory
        InventoryManager.Instance.AddItem(itemName, itemIcon, clueText);

        // ซ่อนไอเท็มจากฉาก
        gameObject.SetActive(false);

        Debug.Log(itemName + " collected!");
    }
}
