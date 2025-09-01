using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownTimerWithMouseSlow : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Drag your TextMeshProUGUI here
    public float countdownTime = 60f; // 1 minute in seconds
    public float mouseSpeed = 1f;     // initial mouse speed
    public float slowAmount = 0.1f;   // reduce speed every 10 seconds

    private float remainingTime;
    private float lastSlowTime;
    private float initialMouseSpeed;

    void Start()
    {
        if (!Application.isPlaying) return; // ทำเฉพาะ Play Mode

        initialMouseSpeed = mouseSpeed;    // เก็บความเร็วเริ่มต้น
        remainingTime = countdownTime;
        lastSlowTime = remainingTime;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        while (remainingTime >= 0)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // ทุก 10 วินาที ทำให้เมาส์ช้าลง
            if (Mathf.FloorToInt(remainingTime / 10) < Mathf.FloorToInt(lastSlowTime / 10))
            {
                mouseSpeed = Mathf.Max(0.1f, mouseSpeed - slowAmount);
                lastSlowTime = remainingTime;
                Debug.Log("Mouse slowed down! Current speed: " + mouseSpeed);
            }

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        timerText.text = "00:00";
        OnTimerEnd();
    }

    void OnTimerEnd()
    {
        Debug.Log("Time's up!");
        // คืนความเร็วเมาส์เป็นปกติ
        mouseSpeed = initialMouseSpeed;
        Debug.Log("Mouse speed restored: " + mouseSpeed);
    }
}
