using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class RotatePuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Puzzle UI")]
    public GameObject rotatePanel;
    public GameObject other;         // ตัวที่จะหมุนใน UI
    public GameObject og;            // ตำแหน่งเป้าหมาย
    public GameObject unlockObject;  // Object ใหม่ที่จะ active เมื่อผ่าน

    [Header("Accepted Items")]
    public string[] acceptedItems;   // รายชื่อไอเทมที่ใช้เปิดได้

    [Header("Effects")]
    public Color startColor = Color.red;    // สีเริ่มต้น (เช่นแดง)
    public Color solvedColor = Color.white; // สีเป้าหมาย (เช่นขาว)
    public float fadeDuration = 2f;         // ระยะเวลา fade

    private Quaternion targetRotation;
    private bool isPanelActive;

    void Start()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(false);

        if (og != null)
            targetRotation = og.transform.rotation;

        // เซ็ตสีเริ่มต้น (ถ้ามี SpriteRenderer)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = startColor;

        if (unlockObject != null)
            unlockObject.SetActive(false);
    }

    // ✅ รับไอเทมจาก InventorySlot
    public void OnItemUsed(string itemName)
    {
        Debug.Log("[RotatePuzzle] Received item: " + itemName);

        bool accept = (acceptedItems == null || acceptedItems.Length == 0);

        if (!accept)
        {
            foreach (var allowed in acceptedItems)
            {
                if (itemName == allowed)
                {
                    accept = true;
                    break;
                }
            }
        }

        if (accept)
        {
            Debug.Log("[RotatePuzzle] Correct item used → Opening panel");
            OpenPanel();
        }
        else
        {
            Debug.Log("[RotatePuzzle] Wrong item: " + itemName);
        }
    }

    public void OpenPanel()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(true);

        isPanelActive = true;
    }

    void Update()
    {
        if (isPanelActive && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerInsidePanel())
            {
                ClosePanel();
            }
        }
    }

    // ✅ ใช้ EventSystem ตรวจว่า pointer อยู่บน panel หรือ child
    private bool IsPointerInsidePanel()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            if (r.gameObject == rotatePanel || r.gameObject.transform.IsChildOf(rotatePanel.transform))
                return true;
        }
        return false;
    }

    public void RotateLeft() => Rotate(90);   // ⬅ ซ้าย = +90
    public void RotateRight() => Rotate(-90); // ➡ ขวา = -90

    void Rotate(float angle)
    {
        if (other == null || og == null) return;

        other.transform.Rotate(0, 0, angle);

        if (Quaternion.Angle(other.transform.rotation, targetRotation) < 1f)
        {
            Debug.Log("[RotatePuzzle] Puzzle Solved!");
            ClosePanel();

            // ✅ ทำเอฟเฟกต์ fade
            StartCoroutine(FadeSolvedEffects());

            // ✅ ทำให้ item "Rotate" หายจาก inventory (แต่ช่องยังอยู่)
            InventorySlot[] slots = FindObjectsOfType<InventorySlot>();
            foreach (var slot in slots)
            {
                if (slot.currentItem != null && slot.currentItem.itemName == "Rotate")
                {
                    slot.ClearSlot(); // ❌ ไม่ปิด slot, แค่ล้างของ
                    Debug.Log("[RotatePuzzle] Cleared Rotate item from inventory");
                }
            }

            // ✅ Sync RotateInBG (ตัวนี้ติดสคริปต์ RotatePuzzle) ให้เหมือน other
            transform.rotation = other.transform.rotation;
        }
    }

    IEnumerator FadeSolvedEffects()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        float t = 0f;

        if (unlockObject != null)
        {
            unlockObject.SetActive(true);
            SpriteRenderer srUnlock = unlockObject.GetComponent<SpriteRenderer>();
            if (srUnlock != null)
            {
                Color c = srUnlock.color;
                c.a = 0f;
                srUnlock.color = c;

                // fade ทั้งสี object นี้ และ fade unlock object พร้อมกัน
                while (t < fadeDuration)
                {
                    t += Time.deltaTime;
                    float progress = t / fadeDuration;

                    if (sr != null)
                        sr.color = Color.Lerp(startColor, solvedColor, progress);

                    srUnlock.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, progress));

                    yield return null;
                }

                // set ค่า final
                if (sr != null) sr.color = solvedColor;
                srUnlock.color = new Color(c.r, c.g, c.b, 1f);
            }
        }
        else
        {
            // ถ้าไม่มี unlockObject → แค่ fade สีตัวเอง
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float progress = t / fadeDuration;

                if (sr != null)
                    sr.color = Color.Lerp(startColor, solvedColor, progress);

                yield return null;
            }

            if (sr != null) sr.color = solvedColor;
        }
    }

    void ClosePanel()
    {
        if (rotatePanel != null)
            rotatePanel.SetActive(false);

        isPanelActive = false;
    }
}
