using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WallCountdownWithImages : MonoBehaviour
{
    [Header("Clock Settings")]
    public float countdownTime = 300f; // เริ่มต้น 5 นาที
    public TextMeshPro clockText; // 👉 ใช้ 3D TMP (world space)

    [Header("Image Settings")]
    public List<GameObject> images; // 👉 เก็บเป็น GameObject
    public float changeInterval = 60f;
    public float fadeDuration = 2f;

    private int currentImageIndex = 0;
    private float nextChangeTime;
    private bool isFading = false;

    void Start()
    {
        // ปิดทุกรูป ยกเว้นอันแรก
        for (int i = 0; i < images.Count; i++)
        {
            images[i].SetActive(i == 0);
            if (images[i].TryGetComponent<Image>(out var img))
            {
                Color c = img.color;
                c.a = (i == 0 ? 1f : 0f);
                img.color = c;
            }
        }

        currentImageIndex = 0;
        nextChangeTime = countdownTime - changeInterval;
    }

    void Update()
    {
        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;

        // อัปเดตนาฬิกา
        int minutes = Mathf.FloorToInt(countdownTime / 60);
        int seconds = Mathf.FloorToInt(countdownTime % 60);
        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // เปลี่ยนรูปตามเวลา
        if (!isFading && currentImageIndex < images.Count - 1 && countdownTime <= nextChangeTime)
        {
            StartCoroutine(FadeToNextImage());
            currentImageIndex++;
            nextChangeTime = countdownTime - changeInterval;
        }

        // ปุ่ม R → ลดเวลาไป 30 วิ
        if (Input.GetKeyDown(KeyCode.R))
        {
            countdownTime -= 30f;
            if (countdownTime < 0) countdownTime = 0;
            Debug.Log("Time adjusted: " + countdownTime);
        }
    }

    IEnumerator FadeToNextImage()
    {
        isFading = true;

        GameObject currentGO = images[currentImageIndex];
        GameObject nextGO = images[currentImageIndex + 1];

        Image current = currentGO.GetComponent<Image>();
        Image next = nextGO.GetComponent<Image>();

        nextGO.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;

            if (current != null)
                current.color = new Color(current.color.r, current.color.g, current.color.b, 1f - alpha);
            if (next != null)
                next.color = new Color(next.color.r, next.color.g, next.color.b, alpha);

            yield return null;
        }

        // จบแล้ว
        if (current != null)
        {
            current.color = new Color(current.color.r, current.color.g, current.color.b, 0f);
        }
        currentGO.SetActive(false);

        if (next != null)
        {
            next.color = new Color(next.color.r, next.color.g, next.color.b, 1f);
        }

        isFading = false;
    }
}
