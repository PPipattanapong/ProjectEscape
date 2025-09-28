using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Slot Positions (Empty GameObjects)")]
    public Transform[] slotPositions;   // ช่องว่างที่เตรียมไว้ใน Canvas
    public GameObject slotPrefab;

    private InventorySlot[] slots;      // เก็บ reference แต่ละช่อง

    void Awake()
    {
        Instance = this;
        slots = new InventorySlot[slotPositions.Length];

        // สร้าง slot ตามตำแหน่งที่กำหนดไว้
        for (int i = 0; i < slotPositions.Length; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotPositions[i]);
            slotObj.transform.localPosition = Vector3.zero;

            slots[i] = slotObj.GetComponent<InventorySlot>();
            slots[i].ClearSlot(); // ให้เริ่มว่างทั้งหมด
        }
    }

    // ✅ เพิ่มของลงช่องว่าง
    public void AddItem(ItemData item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty())   // ถ้าว่าง → ใส่ตรงนี้
            {
                slots[i].SetItem(item);
                return;
            }
        }

        // ถ้าเต็มหมด → เขียนทับช่องแรก
        slots[0].SetItem(item);
    }

    public InventorySlot[] GetSlots()
    {
        return slots;
    }
}
