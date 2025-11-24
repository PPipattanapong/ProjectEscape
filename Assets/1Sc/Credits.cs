using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditRollTMPOnly : MonoBehaviour
{
    [Header("Credit Texts (Move Up)")]
    public RectTransform credit1;
    public RectTransform credit2;
    public float scrollSpeed = 50f;

    [Header("Trigger Y Position")]
    public float triggerY = 2520f;

    [Header("Intro Credit (Fade In)")]
    public TextMeshProUGUI introCredit;
    public float introStartTime = 0f;
    public float introFadeDuration = 1f;

    [Header("Final Credit (Fade In/Out)")]
    public TextMeshProUGUI finalCredit;
    public float finalFadeDuration = 1.5f;
    public float finalHoldTime = 5f;

    [Header("Audio Fade Out")]
    public AudioSource bgmAudio;          // <<< เพิ่มช่องใส่ AudioSource
    public float audioFadeDuration = 1.5f;

    [Header("Scene Settings")]
    public string menuScene = "MainMenu";
    public string settingsScene = "Settings";

    void Start()
    {
        if (introCredit != null)
        {
            SetAlpha(introCredit, 0);
            introCredit.gameObject.SetActive(false);
        }

        SetAlpha(finalCredit, 0);
        finalCredit.gameObject.SetActive(false);

        StartCoroutine(RunCredits());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(settingsScene);
        }
    }

    IEnumerator RunCredits()
    {
        float timer = 0f;
        bool introPlayed = false;

        while (credit1.anchoredPosition.y < triggerY ||
               credit2.anchoredPosition.y < triggerY)
        {
            timer += Time.deltaTime;

            if (!introPlayed && timer >= introStartTime)
            {
                introPlayed = true;

                if (introCredit != null)
                {
                    introCredit.gameObject.SetActive(true);
                    StartCoroutine(FadeTMP(introCredit, 0, 1, introFadeDuration));
                }
            }

            credit1.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
            credit2.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

            yield return null;
        }

        if (introCredit != null)
            introCredit.gameObject.SetActive(false);

        credit1.gameObject.SetActive(false);
        credit2.gameObject.SetActive(false);

        finalCredit.gameObject.SetActive(true);
        yield return StartCoroutine(FadeTMP(finalCredit, 0, 1, finalFadeDuration));

        yield return new WaitForSeconds(finalHoldTime);

        // fade out final credit
        StartCoroutine(FadeTMP(finalCredit, 1, 0, finalFadeDuration));

        // fade out audio (ถ้ามี)
        if (bgmAudio != null)
            yield return StartCoroutine(FadeAudio(bgmAudio, audioFadeDuration));

        SceneManager.LoadScene(menuScene);
    }

    // ───────────────────── Fade TMP ─────────────────────
    IEnumerator FadeTMP(TextMeshProUGUI tmp, float from, float to, float time)
    {
        float t = 0;
        Color c = tmp.color;

        while (t < time)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / time);
            tmp.color = c;
            yield return null;
        }

        c.a = to;
        tmp.color = c;
    }

    // ───────────────────── Fade Audio ─────────────────────
    IEnumerator FadeAudio(AudioSource audioSrc, float time)
    {
        float start = audioSrc.volume;
        float t = 0;

        while (t < time)
        {
            t += Time.deltaTime;
            audioSrc.volume = Mathf.Lerp(start, 0f, t / time);
            yield return null;
        }

        audioSrc.volume = 0f;
        audioSrc.Stop();
    }

    void SetAlpha(TextMeshProUGUI tmp, float a)
    {
        Color c = tmp.color;
        c.a = a;
        tmp.color = c;
    }
}
