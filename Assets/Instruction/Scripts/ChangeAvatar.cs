using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequentialFadeObjects : MonoBehaviour
{
    [Header("Objects to Fade")]
    public List<GameObject> objects; // ใส่ GameObject หลายตัวใน Inspector

    [Header("Settings")]
    public float durationPerObject = 60f; // เวลาที่ให้แต่ละ object อยู่ก่อน fade out
    public float fadeDuration = 2f;       // เวลาที่ใช้ในการ fade out

    private int currentIndex = 0;

    private void Start()
    {
        // เริ่มด้วยการเปิดเฉพาะ object แรก
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].SetActive(i == 0);
        }

        // เริ่ม Coroutine สลับ object
        if (objects.Count > 1)
            StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        while (currentIndex < objects.Count - 1)
        {
            yield return new WaitForSeconds(durationPerObject);

            GameObject currentObject = objects[currentIndex];
            GameObject nextObject = objects[currentIndex + 1];

            // เปิดตัวถัดไปแต่ซ่อน alpha ไว้
            nextObject.SetActive(true);
            SetObjectAlpha(nextObject, 0f);

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                SetObjectAlpha(currentObject, alpha);
                SetObjectAlpha(nextObject, 1 - alpha);
                yield return null;
            }

            SetObjectAlpha(currentObject, 0f);
            SetObjectAlpha(nextObject, 1f);
            currentObject.SetActive(false);

            currentIndex++;
        }
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        // ถ้า object มี SpriteRenderer
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // ถ้า object มี CanvasGroup (UI) ใช้แทนได้
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
        }
    }
}
