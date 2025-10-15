using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BombTime : MonoBehaviour
{
    [Header("Clock Settings")]
    public float countdownTime = 300f;
    public TextMeshPro clockText;

    [Header("UI Feedback")]
    public TextMeshProUGUI stageText;
    [Tooltip("ข้อความที่จะแสดงในแต่ละช่วงเวลา (เช่น Stage 1-5)")]
    public List<string> stageMessages;
    public string timeUpMessage = "Time's up!";

    [Header("Raycast Settings")]
    public LayerMask targetLayers;

    [Header("Debug / Time Skip")]
    public float skipTime = 30f;
    public KeyCode skipKey = KeyCode.R;

    [Header("Scene Settings")]
    public string nextSceneName;

    private int currentStage = -1;

    // ✅ เพิ่มตัวแปร freeze เพื่อหยุดเวลาโดย WireCutPuzzle
    private bool isFrozen = false;

    void Start()
    {
        if (stageText != null)
            stageText.text = "";

        ApplyStageMessage(); // เซ็ตข้อความเริ่มต้น
    }

    void Update()
    {
        // ✅ ถ้าโดน freeze อยู่ — หยุดอัปเดตเวลาและข้อความทั้งหมด
        if (isFrozen) return;

        // --- Countdown ---
        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;

        int minutes = Mathf.FloorToInt(countdownTime / 60);
        int seconds = Mathf.FloorToInt(countdownTime % 60);

        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // --- Stage update ---
        ApplyStageMessage();

        // --- Click Raycast ---
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, targetLayers);

            if (hit.collider != null)
                Debug.Log("💣 Clicked on: " + hit.collider.gameObject.name);
            else
                Debug.Log("💣 Missed click at " + worldPos);
        }

        // --- Skip Time ---
        if (Input.GetKeyDown(skipKey))
        {
            countdownTime -= skipTime;
            if (countdownTime < 0) countdownTime = 0;

            ApplyStageMessage();
            Debug.Log($"⏩ Time skipped {skipTime} sec → Remaining: {countdownTime}");
        }

        // --- When Time's Up ---
        if (countdownTime <= 0f)
        {
            if (stageText != null)
                stageText.text = timeUpMessage;

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("⏱ Time's up! Loading scene: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void ApplyStageMessage()
    {
        // คำนวณ stage จากเวลาที่เหลือ
        float elapsed = 300f - countdownTime;
        int stage = Mathf.Clamp(Mathf.FloorToInt(elapsed / 60f) + 1, 1, 5);

        if (stage != currentStage)
        {
            currentStage = stage;
            if (stageText != null && stageMessages != null && stage - 1 < stageMessages.Count)
                stageText.text = stageMessages[stage - 1];
        }
    }

    // 👇 ฟังก์ชันให้ระบบอื่นเรียกลดเวลาได้ เช่น puzzle ผิด
    public void ReduceTime(float amount)
    {
        countdownTime -= amount;
        if (countdownTime < 0) countdownTime = 0;

        int m = Mathf.FloorToInt(countdownTime / 60);
        int s = Mathf.FloorToInt(countdownTime % 60);
        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", m, s);

        ApplyStageMessage();
        Debug.Log($"⏰ Time reduced by {amount} seconds → Remaining: {countdownTime}");
    }

    // ✅ ฟังก์ชันใหม่ — ให้ WireCutPuzzle เรียกเพื่อหยุดเวลา
    public void FreezeTimer()
    {
        isFrozen = true;
        Debug.Log("[BombTime] ⏸ Timer frozen by external event.");
    }

    // ✅ ฟังก์ชันเสริม — ปลด freeze ถ้าจำเป็น (เช่นใช้ใน scene ต่อไป)
    public void UnfreezeTimer()
    {
        isFrozen = false;
        Debug.Log("[BombTime] ▶ Timer resumed.");
    }
}
