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

    [Header("Extra Object To Activate When Unlocked")]
    public GameObject extraObjectToActivate;   // ⭐ อันใหม่

    [Header("Extra Object To Destroy")]
    public GameObject destroyWhenUnlocked;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Tooltip To Remove On Unlock")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    [Header("Audio")]
    public AudioSource safeOpenSound;

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

        // ⭐ extraObject ไม่ยุ่ง alpha — แค่ปิดไว้เฉย ๆ
        if (extraObjectToActivate != null)
            extraObjectToActivate.SetActive(false);

        if (destroyWhenUnlocked != null)
            destroyWhenUnlocked.SetActive(true);
    }

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

        // 🔊 เสียงเปิดเซฟ
        if (safeOpenSound != null)
            safeOpenSound.Play();

        yield return new WaitForSeconds(0.3f);

        // ⭐ โผล่ตรงนี้เลย ก่อน fade ทั้งหมด
        if (extraObjectToActivate != null)
        {
            extraObjectToActivate.SetActive(true);
            Debug.Log("[SafeProgressLight] Extra object activated instantly: " + extraObjectToActivate.name);
        }

        // Fade out safe
        if (safeRenderer != null)
            yield return StartCoroutine(FadeSprite(safeRenderer, 1f, 0f));

        // Fade in new object
        if (newObject != null)
        {
            newObject.gameObject.SetActive(true);
            yield return StartCoroutine(FadeSprite(newObject, 0f, 1f));
        }

        if (destroyWhenUnlocked != null)
            StartCoroutine(FadeAndDestroy(destroyWhenUnlocked, fadeDuration));

        // ลบ tooltip ทั้งหมด
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

        // ⭐ reset extra object
        if (extraObjectToActivate != null)
            extraObjectToActivate.SetActive(false);

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
