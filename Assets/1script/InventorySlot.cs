using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI clueText;

    [HideInInspector] public ItemData currentItem;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;

    // ✅ สูตรผสมของ
    public static Dictionary<(string, string), ItemData> combinationRecipes
        = new Dictionary<(string, string), ItemData>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (nameText != null)
            nameText.text = item.itemName;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (clueText != null)
        {
            clueText.text = item.description;
            clueText.gameObject.SetActive(false); // ซ่อน clue ไว้ก่อน
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        originalPos = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;
        canvasGroup.blocksRaycasts = true;

        Debug.Log($"[OnEndDrag] Dropping {currentItem.itemName}");

        // --- 1) เช็ค combine กับ slot อื่น ---
        if (eventData.pointerEnter != null)
        {
            InventorySlot otherSlot = eventData.pointerEnter.GetComponentInParent<InventorySlot>();
            if (otherSlot != null && otherSlot != this && otherSlot.currentItem != null)
            {
                var key = (currentItem.itemName, otherSlot.currentItem.itemName);
                if (combinationRecipes.ContainsKey(key))
                {
                    ItemData newItem = combinationRecipes[key];
                    Debug.Log($"[Combine] {currentItem.itemName} + {otherSlot.currentItem.itemName} → {newItem.itemName}");

                    Destroy(otherSlot.gameObject);
                    Destroy(gameObject);

                    InventoryManager.Instance.AddItem(newItem.itemName, newItem.icon, newItem.description);
                    return;
                }
            }
        }

        // --- 2) ยิง Raycast2D ไปที่ world object ---
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            IItemReceiver receiver = hit.collider.GetComponent<IItemReceiver>();
            if (receiver != null)
            {
                Debug.Log($"[OnEndDrag] Sending item: {currentItem.itemName} to {hit.collider.gameObject.name}");
                receiver.OnItemUsed(currentItem.itemName);

                // ✅ อยากให้ key ยังอยู่ต่อ → แค่รีเซ็ตตำแหน่ง
                rectTransform.anchoredPosition = originalPos;
                return;
            }
        }

        // --- 3) ไม่โดนอะไรเลย → reset กลับ ---
        rectTransform.anchoredPosition = originalPos;
    }



    // ✅ toggle clue ด้วยคลิกขวา
    void Update()
    {
        if (currentItem != null && Input.GetMouseButtonDown(1)) // Right Click
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var r in results)
            {
                InventorySlot slot = r.gameObject.GetComponentInParent<InventorySlot>();
                if (slot == this) // ถ้า raycast เจอ slot ตัวเอง
                {
                    ToggleClue();
                    return;
                }
            }

            Debug.Log("[RightClick] Raycast did not hit this slot.");
        }
    }

    private void ToggleClue()
    {
        if (clueText == null)
        {
            Debug.LogWarning("[Clue] clueText is NULL in prefab!");
            return;
        }

        bool isActive = clueText.gameObject.activeSelf;
        clueText.gameObject.SetActive(!isActive);

        Debug.Log($"[Clue] {currentItem.itemName} → {(isActive ? "HIDE" : "SHOW")}");
    }

    // ✅ สูตรรวม
    public static void InitRecipes()
    {
        Sprite combinedSprite = Resources.Load<Sprite>("Icons/NoteCombined");
        var combinedItem = new ItemData("NoteCombined", combinedSprite, "The scraps align... Code: 4567.");

        combinationRecipes.Add(("NoteLeft", "NoteRight"), combinedItem);
        combinationRecipes.Add(("NoteRight", "NoteLeft"), combinedItem);
    }
}
