using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MouseClickRipple : MonoBehaviour
{
    public Canvas parentCanvas;      // Canvas ที่ให้โชว์ effect
    public Image ripplePrefab;       // Prefab รูปวงกลม

    public float duration = 0.4f;
    public float startSize = 0.1f;
    public float endSize = 1.0f;
    public float startAlpha = 0.6f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SpawnRipple();
        }
    }

    void SpawnRipple()
    {
        Image ripple = Instantiate(ripplePrefab, parentCanvas.transform);
        ripple.transform.position = Input.mousePosition;

        ripple.transform.localScale = Vector3.one * startSize;

        StartCoroutine(AnimateRipple(ripple));
    }

    IEnumerator AnimateRipple(Image ripple)
    {
        float t = 0f;
        Color c = ripple.color;

        while (t < duration)
        {
            t += Time.deltaTime;

            float progress = t / duration;

            ripple.transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, progress);
            c.a = Mathf.Lerp(startAlpha, 0f, progress);
            ripple.color = c;

            yield return null;
        }

        Destroy(ripple.gameObject);
    }
}
