using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class HintSparkleGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;
    public float speed = 2f;
    public float startDelay = 5f;

    [Header("Particle Cycle Settings")]
    [Tooltip("ลิสต์ของพาร์ติเคิลที่จะโผล่มาทีละอันตามลำดับ")]
    public List<GameObject> sparkleParticles = new List<GameObject>();

    [Tooltip("เวลาที่จะให้พาร์ติเคิลอันแรกโผล่มา (วินาที)")]
    public float firstAppearDelay = 15f;

    private SpriteRenderer sr;
    private float timer;
    private bool started = false;

    private int currentIndex = -1;
    private bool sequenceActive = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // ปิดทุก particle ตอนเริ่ม
        foreach (var p in sparkleParticles)
        {
            if (p != null)
                p.SetActive(false);
        }

        // เริ่มเอฟเฟกต์วิบวับ (glow)
        if (startDelay > 0)
            Invoke(nameof(StartGlowing), startDelay);
        else
            started = true;

        // ตั้งเวลาสำหรับเริ่ม particle sequence
        if (firstAppearDelay > 0)
            Invoke(nameof(StartParticleSequence), firstAppearDelay);
        else
            StartParticleSequence();
    }

    void StartGlowing()
    {
        started = true;
    }

    void Update()
    {
        if (!started || sr == null) return;

        // ทำให้ sprite วิบวับ (sin wave)
        timer += Time.deltaTime * speed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(timer) + 1f) / 2f);

        Color c = sr.color;
        c.a = alpha;
        sr.color = c;

        // ตรวจสอบว่าพาร์ติเคิลปัจจุบันหายหรือยัง
        if (sequenceActive && currentIndex >= 0 && currentIndex < sparkleParticles.Count)
        {
            GameObject current = sparkleParticles[currentIndex];
            if (current == null || !current.activeSelf)
            {
                NextParticle();
            }
        }
    }

    void StartParticleSequence()
    {
        sequenceActive = true;
        currentIndex = -1;
        NextParticle();
    }

    void NextParticle()
    {
        currentIndex++;

        // ถ้าหมดลิสต์แล้ว → จบ
        if (currentIndex >= sparkleParticles.Count)
        {
            sequenceActive = false;
            Debug.Log("[HintSparkleGlow] Particle sequence finished.");
            return;
        }

        // เปิดอันถัดไป
        GameObject next = sparkleParticles[currentIndex];
        if (next != null)
        {
            next.SetActive(true);
            ParticleSystem ps = next.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();

            Debug.Log($"[HintSparkleGlow] Activated particle #{currentIndex + 1}");
        }
    }
}
