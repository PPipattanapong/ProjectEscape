using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WallCountdownWithImages : MonoBehaviour
{
    [Header("Clock Settings")]
    public float countdownTime = 300f;
    public TextMeshPro clockText;

    [Header("Image Settings")]
    public List<GameObject> images;
    public float changeInterval = 60f;
    public float fadeDuration = 2f;

    [Header("UI Feedback")]
    public TextMeshProUGUI stageText;
    public List<string> stageMessages;

    [Header("Raycast Settings")]
    public LayerMask targetLayers;

    [Header("Debug / Time Skip")]
    public float skipTime = 30f;
    public KeyCode skipKey = KeyCode.R;

    [Header("Scene Settings")]
    public string nextSceneName;

    // ---- Added Post Processing ----
    private Volume volume;
    private ChromaticAberration ca;
    private FilmGrain grain;

    private int currentImageIndex = 0;
    private float nextChangeTime;
    private bool isFading = false;

    void Start()
    {
        // ----- GET PP EFFECTS -----
        volume = FindObjectOfType<Volume>();
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out ca);
            volume.profile.TryGet(out grain);
        }

        // ปิดทุกภาพยกเว้นอันแรก
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

        if (stageText != null) stageText.text = "";
    }

    void Update()
    {
        // --- นับเวลาถอยหลัง ---
        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;

        if (clockText != null)
            clockText.text = $"{Mathf.FloorToInt(countdownTime / 60):00}:{Mathf.FloorToInt(countdownTime % 60):00}";

        // --- เปลี่ยนภาพอัตโนมัติ ---
        if (!isFading && currentImageIndex < images.Count - 1 && countdownTime <= nextChangeTime)
        {
            StartCoroutine(FadeToNextImage());
            currentImageIndex++;
            nextChangeTime = countdownTime - changeInterval;
        }

        // ---- Apply PP Aging Effect ----
        ApplyAgingPostProcessing();

        // --- Raycast ---
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, targetLayers);

            Debug.Log(hit.collider != null
                ? "Clicked: " + hit.collider.gameObject.name
                : "Missed worldPos=" + worldPos);
        }

        // --- Skip Time ---
        if (Input.GetKeyDown(skipKey))
        {
            countdownTime -= skipTime;
            if (countdownTime < 0) countdownTime = 0;

            ApplyAgingPostProcessing();
            ForceUpdateBackgroundImage();

            Debug.Log($"Skipped {skipTime} sec → {countdownTime} left");
        }

        // --- หมดเวลา ---
        if (countdownTime <= 0 && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void ApplyAgingPostProcessing()
    {
        if (ca == null || grain == null) return;

        float elapsed = 300f - countdownTime;
        float percent = elapsed / 300f; // 0 → 1

        ca.intensity.value = Mathf.Lerp(0f, 0.5f, percent);
        grain.intensity.value = Mathf.Lerp(0f, 0.7f, percent);
    }

    // --- Fade system ---
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
            float a = t / fadeDuration;

            if (current != null)
                current.color = new Color(current.color.r, current.color.g, current.color.b, 1f - a);

            if (next != null)
                next.color = new Color(next.color.r, next.color.g, next.color.b, a);

            yield return null;
        }

        currentGO.SetActive(false);

        isFading = false;
    }

    public void ReduceTime(float amount)
    {
        countdownTime -= amount;
        if (countdownTime < 0) countdownTime = 0;

        if (clockText != null)
        {
            int m = Mathf.FloorToInt(countdownTime / 60);
            int s = Mathf.FloorToInt(countdownTime % 60);
            clockText.text = string.Format("{0:00}:{1:00}", m, s);
        }

        // อัปเดตเอฟเฟกต์แก่ (สีเพี้ยน/เกรน)
        ApplyAgingPostProcessing();

        Debug.Log($"Time reduced by {amount} sec → {countdownTime}");
    }


    // --- Jump stage ---
    private void ForceUpdateBackgroundImage()
    {
        float elapsed = 300f - countdownTime;

        int targetIndex = Mathf.Clamp(
            Mathf.FloorToInt(elapsed / changeInterval),
            0,
            images.Count - 1
        );

        for (int i = 0; i < images.Count; i++)
        {
            bool active = (i == targetIndex);
            images[i].SetActive(active);

            if (images[i].TryGetComponent<Image>(out var img))
                img.color = new Color(img.color.r, img.color.g, img.color.b, active ? 1f : 0f);
        }

        currentImageIndex = targetIndex;
    }
}
