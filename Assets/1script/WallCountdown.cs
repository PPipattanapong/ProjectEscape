using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 👉 ต้องใช้สำหรับโหลด Scene
using System.Collections;
using System.Collections.Generic;

public class WallCountdownWithImages : MonoBehaviour
{
    [Header("Clock Settings")]
    public float countdownTime = 300f;
    public TextMeshPro clockText;

    [Header("Image Settings")]
    public List<GameObject> images;
    public float changeInterval = 60f;
    public float fadeDuration = 2f;

    [Header("Aging Effect Settings")]
    public float maxShakeStrength = 50f;
    public float maxSlowdown = 0.4f;

    [Header("UI Feedback")]
    public TextMeshProUGUI stageText;
    public List<string> stageMessages;

    [Header("Raycast Settings")]
    public LayerMask targetLayers;

    [Header("Debug / Time Skip")]
    public float skipTime = 30f;
    public KeyCode skipKey = KeyCode.R;

    [Header("Scene Settings")]
    public string nextSceneName; // 👉 ตั้งชื่อ Scene ที่จะโหลดใน Inspector

    private int currentImageIndex = 0;
    private float nextChangeTime;
    private bool isFading = false;

    private Vector3 fakeMouseOffset = Vector3.zero;
    private float currentMouseSpeed = 1f;
    private bool invertX = false;
    private bool invertY = false;

    private int currentStage = -1;

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

        if (stageText != null) stageText.text = "";
    }

    void Update()
    {
        // --- นับเวลา ---
        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;

        int minutes = Mathf.FloorToInt(countdownTime / 60);
        int seconds = Mathf.FloorToInt(countdownTime % 60);
        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (!isFading && currentImageIndex < images.Count - 1 && countdownTime <= nextChangeTime)
        {
            StartCoroutine(FadeToNextImage());
            currentImageIndex++;
            nextChangeTime = countdownTime - changeInterval;
        }

        // --- กำหนดขั้นความแก่ ---
        ApplyAgingStage();

        // --- Raycast click ---
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 agedMouse = GetAgedMousePosition();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(agedMouse);

            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, targetLayers);
            if (hit.collider != null)
                Debug.Log("👴 Clicked (aged): " + hit.collider.gameObject.name);
            else
                Debug.Log("👴 Missed (aged) worldPos=" + worldPos);
        }

        // --- ปุ่มเร่งเวลา ---
        if (Input.GetKeyDown(skipKey))
        {
            countdownTime -= skipTime;
            if (countdownTime < 0) countdownTime = 0;

            ApplyAgingStage();

            int m = Mathf.FloorToInt(countdownTime / 60);
            int s = Mathf.FloorToInt(countdownTime % 60);
            if (clockText != null)
                clockText.text = string.Format("{0:00}:{1:00}", m, s);

            Debug.Log($"⏩ Time skipped {skipTime} sec → {countdownTime} left");
        }

        // --- เวลาเป็นศูนย์ โหลด Scene ใหม่ ---
        if (countdownTime <= 0f && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("⏱ Time's up! Loading scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void ApplyAgingStage()
    {
        float elapsed = 300f - countdownTime;
        int stage = Mathf.Clamp(Mathf.FloorToInt(elapsed / 60f) + 1, 1, 5);

        if (stage != currentStage)
        {
            currentStage = stage;

            if (stageText != null && stageMessages != null && stage - 1 < stageMessages.Count)
                stageText.text = stageMessages[stage - 1];
        }

        switch (stage)
        {
            case 1:
                currentMouseSpeed = 1f;
                fakeMouseOffset = Vector3.zero;
                invertX = invertY = false;
                break;
            case 2:
                currentMouseSpeed = 0.9f;
                fakeMouseOffset = Random.insideUnitCircle * (maxShakeStrength * 0.1f);
                invertX = false; invertY = false;
                break;
            case 3:
                currentMouseSpeed = 0.75f;
                fakeMouseOffset = Random.insideUnitCircle * (maxShakeStrength * 0.3f);
                invertX = false; invertY = true;
                break;
            case 4:
                currentMouseSpeed = 0.6f;
                fakeMouseOffset = Random.insideUnitCircle * (maxShakeStrength * 0.6f);
                invertX = true; invertY = true;
                break;
            case 5:
                currentMouseSpeed = maxSlowdown;
                fakeMouseOffset = Random.insideUnitCircle * maxShakeStrength;
                invertX = true; invertY = true;
                break;
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

        currentGO.SetActive(false);
        if (current != null)
            current.color = new Color(current.color.r, current.color.g, current.color.b, 0f);
        if (next != null)
            next.color = new Color(next.color.r, next.color.g, next.color.b, 1f);

        isFading = false;
    }

    public Vector3 GetAgedMousePosition()
    {
        Vector3 raw = Input.mousePosition;

        if (invertX) raw.x = Screen.width - raw.x;
        if (invertY) raw.y = Screen.height - raw.y;

        raw += fakeMouseOffset * Time.deltaTime;
        return Vector3.Lerp(raw, raw + fakeMouseOffset, 1f - currentMouseSpeed);
    }

    void OnGUI()
    {
        Vector3 raw = Input.mousePosition;
        Vector3 aged = GetAgedMousePosition();

        GUI.color = Color.green;
        GUI.Label(new Rect(raw.x, Screen.height - raw.y, 100, 20), "RAW");

        GUI.color = Color.red;
        GUI.Label(new Rect(aged.x, Screen.height - aged.y, 100, 20), "AGED");
    }
}
