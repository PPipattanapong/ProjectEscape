using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI clueText;

    [HideInInspector] public ItemData currentItem;

    [Header("Expand Slot Offset")]
    public Vector2 slotExpandOffset = Vector2.zero; // เลื่อนตำแหน่งตอน expand

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    // Drag เฉพาะ icon
    private RectTransform iconRect;
    private Vector2 iconOriginalPos;
    private Transform iconOriginalParent;
    private Vector2 savedIconSize;

    // Expand
    private bool isExpanded = false;

    private float originalNameFontSize;
    private float originalClueFontSize;
    private Vector2 originalNameSize;
    private Vector2 originalClueSize;

    // เก็บค่าเดิมของ SlotPrefab ทั้งกล่อง
    private RectTransform rectTransform;
    private Transform slotOriginalParent;
    private Vector2 slotOriginalPos;
    private Vector2 slotOriginalSize;
    private int slotOriginalIndex;
    private Vector2 slotOriginalAnchorMin;
    private Vector2 slotOriginalAnchorMax;
    private Vector2 slotOriginalPivot;

    [Header("Expand Settings")]
    public Vector2 slotExpandSize = new Vector2(1000, 600);   // ขนาด slot ตอนขยาย
    public Vector2 iconExpandSize = new Vector2(500, 250);    // ขนาด icon ตอนขยาย
    public float nameExpandFontSize = 48f;                    // ขนาดฟอนต์ชื่อ
    public float clueExpandFontSize = 28f;                    // ขนาดฟอนต์คำใบ้

    [Header("Expand Layout Anchors")]
    public Vector2 nameAnchorMin = new Vector2(0f, 0.85f);
    public Vector2 nameAnchorMax = new Vector2(1f, 1f);
    public Vector2 clueAnchorMin = new Vector2(0f, 0f);
    public Vector2 clueAnchorMax = new Vector2(1f, 0.2f);

    // ✅ สูตรผสม
    public static Dictionary<(string, string), ItemData> combinationRecipes
        = new Dictionary<(string, string), ItemData>();

    // ✅ ไฟฉาย
    [Header("Flashlight Settings")]
    public Color idleColor = Color.white;
    public Color activeColor = Color.cyan;
    private bool isFlashlight => currentItem != null && currentItem.itemName == "UVFlashlight";

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage != null) iconRect = iconImage.GetComponent<RectTransform>();
        if (nameText != null)
        {
            originalNameSize = nameText.rectTransform.sizeDelta;
            originalNameFontSize = nameText.fontSize;
        }
        if (clueText != null)
        {
            originalClueSize = clueText.rectTransform.sizeDelta;
            originalClueFontSize = clueText.fontSize;
        }
    }

    // ✅ เคลียร์ช่อง
    public void ClearSlot()
    {
        currentItem = null;
        if (nameText != null) nameText.text = "";
        if (iconImage != null) iconImage.sprite = null;
        if (clueText != null) clueText.text = "";
    }

    // ✅ ใช้เฉพาะตอนยังไม่ขยาย เพื่อ "เปิด"
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isExpanded &&
            eventData.button == PointerEventData.InputButton.Right &&
            RectTransformUtility.RectangleContainsScreenPoint(iconRect, eventData.position, eventData.pressEventCamera))
        {
            ToggleExpand();
        }
    }

    void Update()
    {
        // ✅ ถ้าอยู่ในสถานะขยาย ให้กดขวาที่ไหนก็ได้เพื่อปิด
        if (isExpanded && Input.GetMouseButtonDown(1))
        {
            ToggleExpand();
        }
    }

    public bool IsEmpty() => currentItem == null;

    // ✅ ใส่ของใหม่
    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.color = isFlashlight ? idleColor : Color.white;
        }

        if (nameText != null) nameText.text = item.itemName;
        if (clueText != null) clueText.text = item.description;

        // ปกติแสดงแค่ icon
        if (nameText != null) nameText.gameObject.SetActive(false);
        if (clueText != null) clueText.gameObject.SetActive(false);
    }

    // ✅ Drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return;

        iconOriginalPos = iconRect.anchoredPosition;
        iconOriginalParent = iconRect.parent;
        savedIconSize = iconRect.sizeDelta;

        iconRect.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        if (isFlashlight) iconImage.color = activeColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return;
        iconRect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return;
        canvasGroup.blocksRaycasts = true;

        // กลับ parent เดิม
        iconRect.SetParent(iconOriginalParent, true);
        iconRect.anchoredPosition = iconOriginalPos;
        iconRect.sizeDelta = savedIconSize;

        if (isFlashlight) iconImage.color = idleColor;

        bool used = false;
        // --- 1) Combine ---
        PointerEventData combinePointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> combineResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(combinePointer, combineResults);

        foreach (var r in combineResults)
        {
            InventorySlot otherSlot = r.gameObject.GetComponentInParent<InventorySlot>();
            if (otherSlot != null && otherSlot != this && otherSlot.currentItem != null)
            {
                Debug.Log($"[CombineTry] Current = {currentItem.itemName}, Other = {otherSlot.currentItem.itemName}");

                var key = (currentItem.itemName, otherSlot.currentItem.itemName);
                if (combinationRecipes.ContainsKey(key))
                {
                    ItemData newItem = combinationRecipes[key];
                    Debug.Log($"[CombineSuccess] {currentItem.itemName} + {otherSlot.currentItem.itemName} => {newItem.itemName}");

                    // ✅ otherSlot ได้ item ใหม่
                    otherSlot.SetItem(newItem);

                    // ✅ ช่องปัจจุบัน clear แต่ไม่ถูกทำลาย
                    ClearSlot();

                    return;
                }
                else
                {
                    Debug.Log($"[CombineFail] ไม่มีสูตรสำหรับ {currentItem.itemName} + {otherSlot.currentItem.itemName}");
                }
            }
        }


        // --- 2) Raycast UI ---
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            IItemReceiver receiverUI = r.gameObject.GetComponent<IItemReceiver>();
            if (receiverUI != null)
            {
                receiverUI.OnItemUsed(currentItem.itemName);
                used = true;
                break;
            }
        }

        // --- 3) Raycast World ---
        if (!used)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                IItemReceiver receiver = hit.collider.GetComponent<IItemReceiver>();
                if (receiver == null)
                    receiver = hit.collider.GetComponentInParent<IItemReceiver>();

                if (receiver != null)
                {
                    receiver.OnItemUsed(currentItem.itemName);
                    used = true;
                }
            }
        }
    }
    private void ToggleExpand()
    {
        if (!isExpanded)
        {
            // ✅ เก็บค่าต้นฉบับของ slot
            slotOriginalParent = rectTransform.parent;
            slotOriginalPos = rectTransform.anchoredPosition;
            slotOriginalSize = rectTransform.sizeDelta;
            slotOriginalIndex = rectTransform.GetSiblingIndex();
            slotOriginalAnchorMin = rectTransform.anchorMin;
            slotOriginalAnchorMax = rectTransform.anchorMax;
            slotOriginalPivot = rectTransform.pivot;

            // ✅ ย้าย slot ไปกลางจอ + Offset ที่กำหนด
            rectTransform.SetParent(canvas.transform, true);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = slotExpandOffset;
            rectTransform.sizeDelta = slotExpandSize;
            rectTransform.SetAsLastSibling();

            // ✅ ปรับ icon
            if (iconRect != null)
            {
                savedIconSize = iconRect.sizeDelta;
                iconRect.sizeDelta = iconExpandSize;
                iconRect.anchoredPosition = Vector2.zero;
            }

            // ✅ แสดงชื่อ
            if (nameText != null)
            {
                nameText.gameObject.SetActive(true);
                nameText.fontSize = nameExpandFontSize;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.rectTransform.anchorMin = nameAnchorMin;
                nameText.rectTransform.anchorMax = nameAnchorMax;
                nameText.rectTransform.offsetMin = Vector2.zero;
                nameText.rectTransform.offsetMax = Vector2.zero;
            }

            // ✅ แสดง clue
            if (clueText != null)
            {
                clueText.gameObject.SetActive(true);
                clueText.fontSize = clueExpandFontSize;
                clueText.alignment = TextAlignmentOptions.Center;
                clueText.rectTransform.anchorMin = clueAnchorMin;
                clueText.rectTransform.anchorMax = clueAnchorMax;
                clueText.rectTransform.offsetMin = Vector2.zero;
                clueText.rectTransform.offsetMax = Vector2.zero;
            }

            isExpanded = true;
        }
        else
        {
            // ✅ คืนค่า slot กลับตำแหน่งเดิม
            rectTransform.SetParent(slotOriginalParent, true);
            rectTransform.SetSiblingIndex(slotOriginalIndex);
            rectTransform.anchorMin = slotOriginalAnchorMin;
            rectTransform.anchorMax = slotOriginalAnchorMax;
            rectTransform.pivot = slotOriginalPivot;
            rectTransform.anchoredPosition = slotOriginalPos;
            rectTransform.sizeDelta = slotOriginalSize;

            // คืน icon
            if (iconRect != null)
            {
                iconRect.sizeDelta = savedIconSize;
            }

            // ซ่อนชื่อ+clue
            if (nameText != null) nameText.gameObject.SetActive(false);
            if (clueText != null) clueText.gameObject.SetActive(false);

            isExpanded = false;
        }
    }

    public static void InitRecipes()
    {
        Sprite combinedSprite = Resources.Load<Sprite>("Icons/NoteCombined");
        var combinedItem = new ItemData("NoteCombined", combinedSprite, "The scraps align... Code: 4567.");

        combinationRecipes.Add(("NoteLeft", "NoteRight"), combinedItem);
        combinationRecipes.Add(("NoteRight", "NoteLeft"), combinedItem);
    }
}
