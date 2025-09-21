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

    // ✅ เฉพาะไฟฉาย
    [Header("Flashlight Settings")]
    public Color idleColor = Color.white;
    public Color activeColor = Color.cyan;
    private bool isFlashlight => currentItem != null && currentItem.itemName == "UVFlashlight";

    // ✅ Expand settings
    [Header("Expand Settings")]
    public Vector2 expandedSize = new Vector2(400, 400); // ขนาดตอนขยาย
    private Vector2 originalSize;
    private Transform originalParent;
    private bool isExpanded = false;

    private float originalNameFontSize;
    private float originalClueFontSize;

    private Vector2 originalNameSize;
    private Vector2 originalClueSize;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalSize = rectTransform.sizeDelta;

        if (nameText != null) originalNameSize = nameText.rectTransform.sizeDelta;
        if (clueText != null) originalClueSize = clueText.rectTransform.sizeDelta;
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (nameText != null)
            nameText.text = item.itemName;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            if (isFlashlight) iconImage.color = idleColor;
        }

        if (clueText != null)
        {
            clueText.text = item.description;
            clueText.gameObject.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return; // ห้าม drag ตอนขยาย
        originalPos = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;

        if (isFlashlight && iconImage != null)
        {
            iconImage.color = activeColor;
            Debug.Log("[Flashlight] ON");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (currentItem == null || isExpanded) return;
        canvasGroup.blocksRaycasts = true;

        Debug.Log($"[OnEndDrag] Dropping {currentItem.itemName}");

        if (isFlashlight && iconImage != null)
        {
            iconImage.color = idleColor;
            Debug.Log("[Flashlight] OFF");
        }

        bool used = false;

        // --- 1) เช็ค combine ---
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

        // --- 2) ยิง Raycast บน UI ---
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
                Debug.Log($"[OnEndDrag] Sending item: {currentItem.itemName} to {r.gameObject.name} (UI)");
                receiverUI.OnItemUsed(currentItem.itemName);
                used = true;
                break;
            }
        }

        // --- 3) ยิง Raycast2D world object ---
        if (!used)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log($"[OnEndDrag] Raycast hit {hit.collider.gameObject.name}");

                IItemReceiver receiver = hit.collider.GetComponent<IItemReceiver>();
                if (receiver == null)
                    receiver = hit.collider.GetComponentInParent<IItemReceiver>();

                if (receiver != null)
                {
                    Debug.Log($"[OnEndDrag] Sending item: {currentItem.itemName} to {hit.collider.gameObject.name} (World/Parent)");
                    receiver.OnItemUsed(currentItem.itemName);
                    used = true;
                }
                else
                {
                    Debug.Log("[OnEndDrag] No IItemReceiver found on hit object or parent.");
                }
            }
            else
            {
                Debug.Log("[OnEndDrag] Raycast hit nothing.");
            }
        }

        // --- 4) ถ้าไม่โดนอะไรเลย → reset ---
        rectTransform.anchoredPosition = originalPos;
    }

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
                if (slot == this)
                {
                    ToggleExpand();
                    return;
                }
            }
        }
    }

    // ✅ Expand slot ไปกลางจอ
    private void ToggleExpand()
    {
        if (!isExpanded)
        {
            // เก็บค่าปกติ
            originalPos = rectTransform.anchoredPosition;
            originalSize = rectTransform.sizeDelta;
            originalParent = transform.parent;
            if (nameText != null) originalNameFontSize = nameText.fontSize;
            if (clueText != null) originalClueFontSize = clueText.fontSize;

            // ย้ายไป Canvas root
            transform.SetParent(canvas.transform, true);

            // ตั้ง anchor/pivot ให้อยู่กลาง
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            // 👉 ใหญ่ 80% ของจอ
            float targetWidth = Screen.width * 0.8f;
            float targetHeight = Screen.height * 0.8f;
            rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

            rectTransform.SetAsLastSibling();

            // --- NameText (บนสุด) ---
            if (nameText != null)
            {
                nameText.gameObject.SetActive(true);
                nameText.fontSize = originalNameFontSize * 3f;
                nameText.rectTransform.anchorMin = new Vector2(0f, 0.85f);
                nameText.rectTransform.anchorMax = new Vector2(1f, 1f);
                nameText.rectTransform.offsetMin = Vector2.zero;
                nameText.rectTransform.offsetMax = Vector2.zero;
                nameText.alignment = TextAlignmentOptions.Center;
            }

            // --- IconImage (กลาง) ---
            if (iconImage != null)
            {
                iconImage.rectTransform.anchorMin = new Vector2(0f, 0.25f);
                iconImage.rectTransform.anchorMax = new Vector2(1f, 0.85f);
                iconImage.rectTransform.offsetMin = Vector2.zero;
                iconImage.rectTransform.offsetMax = Vector2.zero;
                iconImage.preserveAspect = true;
                iconImage.gameObject.SetActive(true);
            }

            // --- ClueText (ล่างสุด) ---
            if (clueText != null)
            {
                clueText.gameObject.SetActive(true);
                clueText.fontSize = originalClueFontSize * 2f;
                clueText.rectTransform.anchorMin = new Vector2(0f, 0f);
                clueText.rectTransform.anchorMax = new Vector2(1f, 0.25f);
                clueText.rectTransform.offsetMin = Vector2.zero;
                clueText.rectTransform.offsetMax = Vector2.zero;
                clueText.alignment = TextAlignmentOptions.Center;
            }

            isExpanded = true;
            Debug.Log($"[Expand] {currentItem.itemName} enlarged to BIG center");
        }
        else
        {
            // กลับค่าปกติ
            transform.SetParent(originalParent, true);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = originalPos;
            rectTransform.sizeDelta = originalSize;

            // reset Name
            if (nameText != null)
            {
                nameText.fontSize = originalNameFontSize;
                nameText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                nameText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                nameText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                nameText.rectTransform.sizeDelta = originalNameSize; // ✅ คืนขนาดเดิม
                nameText.alignment = TextAlignmentOptions.Left;
            }

            // reset Clue
            if (clueText != null)
            {
                clueText.fontSize = originalClueFontSize;
                clueText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                clueText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                clueText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                clueText.rectTransform.sizeDelta = originalClueSize; // ✅ คืนขนาดเดิม
                clueText.alignment = TextAlignmentOptions.Left;
                clueText.gameObject.SetActive(false);
            }

            // reset Icon
            if (iconImage != null)
            {
                iconImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                iconImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                iconImage.rectTransform.sizeDelta = new Vector2(100, 100);
            }

            isExpanded = false;
            Debug.Log($"[Expand] {currentItem.itemName} restored");
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
