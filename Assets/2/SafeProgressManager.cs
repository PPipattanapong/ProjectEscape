using UnityEngine;
using System.Collections;

public class SafeProgressLight : MonoBehaviour
{
    [Header("Light (Single SpriteRenderer)")]
    public SpriteRenderer lightRenderer;  // ไฟดวงเดียว
    public Color redColor = Color.red;
    public Color greenColor = Color.green;

    [Header("Objects to Fade")]
    public SpriteRenderer safeRenderer;   // ตัวตู้เซฟ
    public SpriteRenderer newObject;      // ของใหม่ที่จะโผล่หลังปลดล็อก

    [Header("Extra Object To Destroy")]
    [Tooltip("วัตถุที่จะค่อยๆจางหายหลังเซฟถูกปลดล็อก")]
    public GameObject destroyWhenUnlocked;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private bool unlocked = false;
    private bool puzzleCompleted = false;

    void Start()
    {
        unlocked = false;
        puzzleCompleted = false;

        // ตั้งไฟเริ่มต้นเป็นแดง
        if (lightRenderer != null)
            lightRenderer.color = redColor;

        // เซ็ตสถานะเริ่มต้นของวัตถุ
        if (newObject != null)
        {
            Color c = newObject.color;
            c.a = 0f;
            newObject.color = c;
            newObject.gameObject.SetActive(false);
        }

        if (safeRenderer != null)
        {
            Color c = safeRenderer.color;
            c.a = 1f;
            safeRenderer.color = c;
        }

        if (destroyWhenUnlocked != null)
        {
            // ซ่อน object นี้ไว้ก่อน (ถ้ายังไม่ได้ปลด)
            destroyWhenUnlocked.SetActive(true);
        }
    }

    // ✅ เรียกจาก puzzle อื่นเมื่อผ่านแล้ว
    public void MarkPuzzleComplete()
    {
        if (unlocked || puzzleCompleted) return;

        puzzleCompleted = true;

        // เปลี่ยนไฟเป็นเขียว
        if (lightRenderer != null)
            lightRenderer.color = greenColor;

        // ปลดล็อกเซฟ
        StartCoroutine(UnlockSafe());
    }

    IEnumerator UnlockSafe()
    {
        unlocked = true;
        yield return new WaitForSeconds(0.3f);

        // Fade out safe
        if (safeRenderer != null)
            yield return StartCoroutine(FadeSprite(safeRenderer, 1f, 0f));

        // Fade in new object
        if (newObject != null)
        {
            newObject.gameObject.SetActive(true);
            yield return StartCoroutine(FadeSprite(newObject, 0f, 1f));
        }

        // ✅ ลบ object ที่ตั้งไว้ใน Inspector แบบ fade
        if (destroyWhenUnlocked != null)
            StartCoroutine(FadeAndDestroy(destroyWhenUnlocked, fadeDuration));
    }

    IEnumerator FadeSprite(SpriteRenderer sr, float from, float to)
    {
        if (sr == null) yield break;

        float t = 0f;
        Color c = sr.color;
        c.a = from;
        sr.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            sr.color = c;
            yield return null;
        }

        c.a = to;
        sr.color = c;
    }

    IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image img = target.GetComponent<UnityEngine.UI.Image>();

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

    [ContextMenu("Reset Progress")]
    public void ResetProgress()
    {
        unlocked = false;
        puzzleCompleted = false;

        if (lightRenderer != null)
            lightRenderer.color = redColor;

        if (newObject != null)
        {
            Color c = newObject.color;
            c.a = 0f;
            newObject.color = c;
            newObject.gameObject.SetActive(false);
        }

        if (safeRenderer != null)
        {
            Color c = safeRenderer.color;
            c.a = 1f;
            safeRenderer.color = c;
        }

        if (destroyWhenUnlocked != null)
        {
            var sr = destroyWhenUnlocked.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
            destroyWhenUnlocked.SetActive(true);
        }

        Debug.Log("🔄 Safe light reset");
    }
}
