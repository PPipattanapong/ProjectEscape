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

        Debug.Log("🔄 Safe light reset");
    }
}
