using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ObjectClickOpener : MonoBehaviour
{
    [Header("Object to Open")]
    public GameObject targetObject; // GameObject ที่จะเปิด/ปิด
    private Camera mainCam;

    [Header("Extra Objects To Destroy (Optional)")]
    [Tooltip("รายการวัตถุที่จะค่อยๆจางหายและถูกทำลายเมื่อเปิด targetObject")]
    public List<GameObject> destroyWhenOpened = new List<GameObject>();

    [Header("Fade Settings")]
    [Tooltip("เวลาจางหาย (วินาที)")]
    public float fadeDuration = 1.5f;

    void Start()
    {
        mainCam = Camera.main;

        if (targetObject != null)
            targetObject.SetActive(false); // ปิดไว้ก่อน

        // เปิดทุก object ในลิสต์ (ให้มองเห็นก่อน)
        foreach (var obj in destroyWhenOpened)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 🛡️ ถ้ามี UI อยู่ใต้เมาส์ เช่น ปุ่ม / Panel → หยุดทำงานทันที
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // ตรวจว่าคลิกโดน GameObject นี้ไหม
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                ToggleTarget(); // คลิกที่วัตถุนี้ → เปิด/ปิด
            }
            else
            {
                // ตรวจว่าคลิกนอก targetObject หรือไม่
                if (targetObject != null && targetObject.activeSelf)
                {
                    if (!IsPointerOverTarget())
                        targetObject.SetActive(false); // คลิกนอก → ปิด
                }
            }
        }
    }

    void ToggleTarget()
    {
        if (targetObject == null) return;

        bool isActive = targetObject.activeSelf;
        targetObject.SetActive(!isActive);

        // ✅ ถ้าเพิ่งเปิด → ทำลาย object ในลิสต์ (fade out ก่อน)
        if (!isActive)
        {
            foreach (var obj in destroyWhenOpened)
            {
                if (obj != null)
                    StartCoroutine(FadeAndDestroy(obj, fadeDuration));
            }
        }
    }

    bool IsPointerOverTarget()
    {
        // ใช้ Raycast2D เพื่อเช็กว่าคลิกโดน targetObject หรือไม่
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider == null) return false;

        return hit.collider.gameObject == targetObject || hit.collider.transform.IsChildOf(targetObject.transform);
    }

    IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        if (target == null) yield break;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image img = target.GetComponent<UnityEngine.UI.Image>();

        // ถ้าไม่มี Renderer ก็ลบทันที
        if (sr == null && img == null)
        {
            Destroy(target);
            yield break;
        }

        float t = 0f;
        Color originalColor = sr ? sr.color : img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, t / duration);

            if (sr)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            if (img)
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        Destroy(target);
    }
}
