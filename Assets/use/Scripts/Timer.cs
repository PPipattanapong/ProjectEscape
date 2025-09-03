using UnityEngine;
using TMPro;
using System.Collections;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText; // Drag your TextMeshProUGUI here

    [Header("Timer Settings")]
    public float countdownTime = 60f; // 1 minute in seconds

    [Header("Mouse Speed Settings")]
    public float mouseSpeed = 1f;     // initial mouse speed
    public float slowAmount = 0.1f;   // reduce speed every 10 seconds

    [Header("Mouse Jitter (สั่น)")]
    public float jitterStartTime = 40f; // เริ่มสั่นตอนเวลานี้
    public float jitterAmount = 0.2f;   // แรงสั่น

    [Header("Mouse Invert (สะท้อน)")]
    public float invertStartTime = 20f; // เริ่มสะท้อนตอนเวลานี้
    public bool invertX = true;         // กลับแกน X
    public bool invertY = false;        // กลับแกน Y

    private float remainingTime;
    private float lastSlowTime;
    private float initialMouseSpeed;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        initialMouseSpeed = mouseSpeed;
        remainingTime = countdownTime;
        lastSlowTime = remainingTime;
        StartCoroutine(StartCountdown());
    }

    void Update()
    {
        // แค่คำนวณเมาส์เอ๋อ แต่ไม่แตะข้อความ
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        if (remainingTime <= invertStartTime)
        {
            if (invertX) mousePos.x = -mousePos.x;
            if (invertY) mousePos.y = -mousePos.y;
        }

        if (remainingTime <= jitterStartTime)
        {
            mousePos += new Vector3(
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount),
                0
            );
        }

        // **ไม่ต้องทำอะไรกับ transform ของข้อความ**
        // transform.position = Vector3.Lerp(transform.position, mousePos, Time.deltaTime * mouseSpeed);
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
        mouseSpeed = initialMouseSpeed;
        Debug.Log("Mouse speed restored: " + mouseSpeed);
    }
}
