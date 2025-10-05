using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class GlowPulseUI : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("สีที่จะสว่างขึ้นตอนวาบ")]
    public Color glowColor = Color.white;

    [Tooltip("ความเร็วของการเปลี่ยนสี (ยิ่งมากยิ่งเร็ว)")]
    public float pulseSpeed = 1.5f;

    [Tooltip("ค่าความเข้มของการสว่าง (0 = ไม่เปลี่ยน, 1 = เต็มสี glow)")]
    [Range(0f, 1f)]
    public float glowIntensity = 0.6f;

    [Tooltip("ให้เล่นวนอัตโนมัติไหม")]
    public bool autoStart = true;

    [Tooltip("ทำให้การเปลี่ยนค่าสีค่อย ๆ เร่ง (ease)")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Image image;
    private Color originalColor;
    private float timer = 0f;

    void Start()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    void Update()
    {
        if (!autoStart) return;

        timer += Time.deltaTime * pulseSpeed;

        // sin wave 0→1→0 แบบ smooth ไม่มีตัด
        float sinValue = (Mathf.Sin(timer) + 1f) * 0.5f;
        float eased = easeCurve.Evaluate(sinValue);

        Color target = Color.Lerp(originalColor, glowColor, eased * glowIntensity);
        image.color = target;
    }
}
