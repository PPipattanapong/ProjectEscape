using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SafeProgressLight : MonoBehaviour
{
    [Header("Light (Single SpriteRenderer)")]
    public SpriteRenderer lightRenderer;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;

    [Header("Objects to Fade")]
    public SpriteRenderer safeRenderer;
    public SpriteRenderer newObject;

    [Header("Extra Object To Destroy")]
    [Tooltip("วัตถุที่จะค่อยๆจางหายหลังเซฟถูกปลดล็อก")]
    public GameObject destroyWhenUnlocked;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Tooltip To Remove On Unlock")]
    [Tooltip("รายการวัตถุที่ต้องลบ Tooltip ทิ้งเมื่อปลดล็อกสำเร็จ")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    private bool unlocked = false;
    private bool puzzleCompleted = false;

    void Start()
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
            destroyWhenUnlocked.SetActive(true);
    }

    // เรียกจาก puzzle อื่นเมื่อผ่านแล้ว
    public void MarkPuzzleComplete()
    {
        if (unlocked || puzzleCompleted) return;

        puzzleCompleted = true;

        if (lightRenderer != null)
            lightRenderer.color = greenColor;

        StartCoroutine(UnlockSafe());
    }

    IEnumerator UnlockSafe()
    {
        unlocked = true;
        yield return new WaitForSeconds(0.3f);

        // Fade out safe
        if (safeRenderer != null)
            yield return StartCoroutine(FadeSprite(safeRenderer, 1f, 0f));

        // Fade in the new object
        if (newObject != null)
        {
            newObject.gameObject.SetActive(true);
            yield return StartCoroutine(FadeSprite(newObject, 0f, 1f));
        }

        // ลบ object fade-out
        if (destroyWhenUnlocked != null)
            StartCoroutine(FadeAndDestroy(destroyWhenUnlocked, fadeDuration));

        // ⭐ ลบ Tooltip ทั้งหมดที่กำหนด
        foreach (var obj in objectsToRemoveTooltip)
        {
            if (obj != null)
            {
                Tooltip t = obj.GetComponent<Tooltip>();
                if (t != null)
                {
                    Destroy(t);
                    Debug.Log("[SafeProgressLight] Removed Tooltip from: " + obj.name);
                }
            }
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
        Color oc = sr ? sr.color : img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(oc.a, 0f, t / duration);

            if (sr) sr.color = new Color(oc.r, oc.g, oc.b, alpha);
            if (img) img.color = new Color(oc.r, oc.g, oc.b, alpha);

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
