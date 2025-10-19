using UnityEngine;
using System.Collections;

public class SafeProgressLight : MonoBehaviour
{
    [Header("Lights (3 SpriteRenderers)")]
    public SpriteRenderer[] lights;  // ไฟ 3 ดวง 2D Sprite
    public Color redColor = Color.red;
    public Color greenColor = Color.green;

    [Header("Objects to Fade")]
    public SpriteRenderer safeRenderer;   // ตัวตู้เซฟ
    public SpriteRenderer newObject;      // ของใหม่ที่จะโผล่หลังปลดล็อก

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private int progress = 0;
    private bool unlocked = false;

    void Start()
    {
        // ❌ ไม่โหลดจาก PlayerPrefs อีกต่อไป (เริ่มใหม่ทุกรอบ)
        progress = 0;
        unlocked = false;

        // ตั้งไฟเริ่มต้นเป็นแดงหมด
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].color = redColor;
        }

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

    // ✅ เรียกจาก puzzle อื่นเมื่อผ่าน 1 ด่าน
    public void MarkPuzzleComplete()
    {
        if (unlocked) return;

        progress = Mathf.Clamp(progress + 1, 0, lights.Length);
        UpdateLights();

        if (progress >= lights.Length)
            StartCoroutine(UnlockSafe());
    }

    void UpdateLights()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;
            lights[i].color = (i < progress) ? greenColor : redColor;
        }
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
        progress = 0;
        unlocked = false;

        foreach (var l in lights)
            if (l != null) l.color = redColor;

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

        Debug.Log("🔄 Safe light progress reset");
    }
}
