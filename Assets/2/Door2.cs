using UnityEngine;

public class DoorController_Simple : MonoBehaviour, IItemReceiver
{
    [Header("Settings")]
    public string requiredItem = "Key";     // 🗝 ชื่อไอเท็มที่ใช้ไขประตู
    public GameObject unlockTarget;         // วัตถุที่จะโผล่หลังประตูเปิด (ตั้งใน Inspector)
    public float fadeDuration = 1.5f;       // เวลา fade-in unlockTarget

    private bool unlocked = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // ปิด unlockTarget ตั้งแต่แรก
        if (unlockTarget != null)
            unlockTarget.SetActive(false);
    }

    public void OnItemUsed(string itemName)
    {
        Debug.Log($"Door received item: {itemName}");

        if (unlocked) return; // ถ้าเปิดไปแล้วไม่ต้องทำซ้ำ

        if (itemName == requiredItem)
        {
            Debug.Log("✅ Key fits! Door unlocked.");

            unlocked = true;

            // ปิด collider ไม่ให้ขวาง
            if (doorCollider != null)
                doorCollider.enabled = false;

            // ลด sorting order (ให้ประตูอยู่ข้างหลัง)
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // แสดงของรางวัลหรือประตูใหม่แบบ fade-in
            if (unlockTarget != null)
                StartCoroutine(FadeInObject(unlockTarget, fadeDuration));
        }
        else
        {
            Debug.Log("❌ Wrong item. Door remains locked.");
        }
    }

    private System.Collections.IEnumerator FadeInObject(GameObject obj, float duration)
    {
        obj.SetActive(true);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image img = obj.GetComponent<UnityEngine.UI.Image>();

        float t = 0f;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                sr.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
        }
        else if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;

            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                img.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
        }
    }
}
