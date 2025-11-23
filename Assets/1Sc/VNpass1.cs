using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Vnpass1 : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerText;
    public Button skipButton;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;
    public string nextSceneName = "GameScene";

    [Header("Voice Settings")]
    public AudioSource voiceSource;            // ★ เสียงพูด
    public List<AudioClip> voiceClips;         // ★ เสียงตรงตาม index ของ dialogueLines

    [Header("Effect References")]
    public Image Black;
    public Image Scene1;
    public Image Scene2;
    public Image Scene3;
    public Image Scene4;

    public float fadeDuration = 2f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool skipTyping = false;

    void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipToGame);

        // เริ่มด้วย Scene1 โผล่ทันที
        SetAlpha(Black, 0f);
        SetAlpha(Scene1, 1f);
        SetAlpha(Scene2, 0f);
        SetAlpha(Scene3, 0f);
        SetAlpha(Scene4, 0f);

        Scene1.gameObject.SetActive(true);
        Scene2.gameObject.SetActive(false);
        Scene3.gameObject.SetActive(false);
        Scene4.gameObject.SetActive(false);

        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnDialogueClick();
    }

    void OnDialogueClick()
    {
        // ถ้ากำลังพิมพ์ → ข้ามไปให้โชว์เต็ม แต่ห้ามตัดเสียง
        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        // จะไปประโยคใหม่ → ต้องตัดเสียงเก่า
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        // ทำ fade เปลี่ยนฉากก่อนค่อยเล่นเสียง
        if (currentLineIndex == 1)
            yield return StartCoroutine(FadeImages(Scene1, Scene2));
        else if (currentLineIndex == 4)
            yield return StartCoroutine(FadeImages(Scene2, Scene3));
        else if (currentLineIndex == 6)
            yield return StartCoroutine(FadeImages(Scene3, Scene4));

        // ★ เล่นเสียงประโยคนี้หลัง Fade เสร็จ
        PlayVoiceForLine(currentLineIndex);

        // --------------------------------------
        // แยก speaker : message
        // --------------------------------------
        string line = dialogueLines[currentLineIndex];
        string speaker = "";
        string message = line;

        int colonIndex = line.IndexOf(':');
        if (colonIndex > 0)
        {
            speaker = line.Substring(0, colonIndex).Trim();
            message = line.Substring(colonIndex + 1).Trim();
        }

        speakerText.text = speaker;

        SetTextAlpha(1f);

        // --------------------------------------
        // Typewriter effect
        // --------------------------------------
        foreach (char c in message)
        {
            if (skipTyping)
            {
                dialogueText.text = message;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    // ★ เล่นเสียงตาม index
    void PlayVoiceForLine(int index)
    {
        if (voiceSource == null) return;
        if (voiceClips == null) return;
        if (index >= voiceClips.Count) return;
        if (voiceClips[index] == null) return;

        voiceSource.Stop();
        voiceSource.clip = voiceClips[index];
        voiceSource.Play();
    }

    IEnumerator FadeImages(Image fromImage, Image toImage)
    {
        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        Black.gameObject.SetActive(true);
        float timer = 0f;

        // Fade-in ดำ
        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / (fadeDuration / 2f));
            SetAlpha(Black, alpha);
            yield return null;
        }

        // สลับภาพ
        SetAlpha(fromImage, 0f);
        fromImage.gameObject.SetActive(false);

        toImage.gameObject.SetActive(true);
        SetAlpha(toImage, 1f);

        // Fade-out ดำ
        timer = 0f;
        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / (fadeDuration / 2f));
            SetAlpha(Black, alpha);
            yield return null;
        }

        SetAlpha(Black, 0f);
        Black.gameObject.SetActive(false);
    }

    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SetTextAlpha(float alpha)
    {
        if (dialogueText != null)
        {
            Color c = dialogueText.color;
            c.a = alpha;
            dialogueText.color = c;
        }

        if (speakerText != null)
        {
            Color c = speakerText.color;
            c.a = alpha;
            speakerText.color = c;
        }
    }

    void SkipToGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
