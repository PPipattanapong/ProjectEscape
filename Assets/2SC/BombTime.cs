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

    // =============================
    //        TIME WARNING
    // =============================
    [Header("Time Warning Sounds")]
    [Tooltip("List เสียงที่จะเล่นตามเวลาที่กำหนด เช่น 4 นาที 3 นาที 1 นาที")]
    public List<AudioSource> warningSounds;

    [Tooltip("เวลาที่จะเล่นเสียง (เป็นวินาที) เช่น 240 = 4 นาที")]
    public List<float> warningTimes;

    private HashSet<int> triggeredIndexes = new HashSet<int>();

    // Freeze Timer
    private bool isFrozen = false;


    void Start()
    {
        if (stageText != null)
            stageText.text = "";

        ApplyStageMessage(); // เริ่มต้น
    }

    void Update()
    {
        if (isFrozen) return;

        // --- Countdown ---
        countdownTime -= Time.deltaTime;
        if (countdownTime < 0) countdownTime = 0;

        int minutes = Mathf.FloorToInt(countdownTime / 60);
        int seconds = Mathf.FloorToInt(countdownTime % 60);

        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        ApplyStageMessage();

        // --- Mouse Click Raycast ---
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, targetLayers);

            Debug.Log(hit.collider != null
                ? "💣 Clicked: " + hit.collider.gameObject.name
                : "💣 Missed at: " + worldPos);
        }

        // --- Skip Time ---
        if (Input.GetKeyDown(skipKey))
        {
            countdownTime -= skipTime;
            if (countdownTime < 0) countdownTime = 0;

            ApplyStageMessage();
            Debug.Log($"⏩ Skip {skipTime} sec → {countdownTime}");
        }

        // ======================================
        //        ⏳ TIME WARNING SOUND
        // ======================================
        for (int i = 0; i < warningTimes.Count; i++)
        {
            if (!triggeredIndexes.Contains(i) && countdownTime <= warningTimes[i])
            {
                if (warningSounds != null && i < warningSounds.Count && warningSounds[i] != null)
                    warningSounds[i].Play();

                triggeredIndexes.Add(i);
            }
        }

        // --- Time Up ---
        if (countdownTime <= 0f)
        {
            if (stageText != null)
                stageText.text = timeUpMessage;

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("⏱ Time's up → loading scene...");
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void ApplyStageMessage()
    {
        float elapsed = 300f - countdownTime;
        int stage = Mathf.Clamp(Mathf.FloorToInt(elapsed / 60f) + 1, 1, 5);

        if (stage != currentStage)
        {
            currentStage = stage;

            if (stageText != null && stageMessages != null && stage - 1 < stageMessages.Count)
                stageText.text = stageMessages[stage - 1];
        }
    }

    // External reduce time
    public void ReduceTime(float amount)
    {
        countdownTime -= amount;
        if (countdownTime < 0) countdownTime = 0;

        int m = Mathf.FloorToInt(countdownTime / 60);
        int s = Mathf.FloorToInt(countdownTime % 60);

        if (clockText != null)
            clockText.text = string.Format("{0:00}:{1:00}", m, s);

        ApplyStageMessage();
        Debug.Log($"⏰ Time reduced by {amount} sec → {countdownTime}");
    }

    // Freeze
    public void FreezeTimer()
    {
        isFrozen = true;
        Debug.Log("[BombTime] Timer frozen.");
    }

    public void UnfreezeTimer()
    {
        isFrozen = false;
        Debug.Log("[BombTime] Timer resumed.");
    }
}
