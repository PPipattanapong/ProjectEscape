using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public Transform slotParent;
    public GameObject slotPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void AddSlot(ItemData item)
    {
        GameObject slot = Instantiate(slotPrefab, slotParent);
        InventorySlot slotScript = slot.GetComponent<InventorySlot>();
        slotScript.SetItem(item);
    }
}
