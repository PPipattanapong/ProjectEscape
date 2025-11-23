using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class VNDialogueManager : MonoBehaviour
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
    public AudioSource voiceSource;           // 👈 ลาก AudioSource ใส่ตรงนี้
    public List<AudioClip> voiceClips;        // 👈 รายการเสียงพากย์ ตำแหน่งตรงกับ dialogueLines

    [Header("Effect References")]
    public Image blackScreenImage;
    public Image eyeImage;
    public Image sceneImage1;
    public Image sceneImage2;

    public float fadeDuration = 2f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool skipTyping = false;

    void Start()
    {
        skipButton.onClick.AddListener(SkipToGame);

        SetAlpha(blackScreenImage, 1f);
        SetAlpha(eyeImage, 1f);
        SetAlpha(sceneImage1, 0f);
        SetAlpha(sceneImage2, 0f);

        sceneImage1.gameObject.SetActive(false);
        sceneImage2.gameObject.SetActive(false);

        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnDialogueClick();
    }

    void OnDialogueClick()
    {
        // ถ้ากำลังพิมพ์ → ให้โชว์ข้อความเต็ม แต่ "อย่าตัดเสียง"
        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        // ถ้าพิมพ์จบแล้ว → กำลังจะไปบรรทัดใหม่
        // ตรงนี้แหละที่ต้องตัดเสียงเก่าทิ้ง
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        // ไปบรรทัดถัดไป
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

        // 🔥❗ 1) อย่าเพิ่งเล่นเสียงจนกว่า Fade จะจบ
        //---- Fade logic ----
        if (currentLineIndex == 2)
            yield return StartCoroutine(FadeOutOnly(blackScreenImage));
        else if (currentLineIndex == 6)
            yield return StartCoroutine(FadeImages(eyeImage, sceneImage1));
        else if (currentLineIndex == 10)
            yield return StartCoroutine(FadeImages(sceneImage1, sceneImage2));

        // 🟦❗ 2) Fade เสร็จแล้ว ค่อยเล่นเสียง
        PlayVoiceForLine(currentLineIndex);

        //---- Speaker / Message split ----
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

        //---- Typewriter effect ----
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


    // 🎤 เล่นเสียงตาม line index
    void PlayVoiceForLine(int index)
    {
        if (voiceSource == null) return;
        if (voiceClips == null) return;
        if (index >= voiceClips.Count) return;
        if (voiceClips[index] == null) return;

        voiceSource.Stop();              // กันเสียงเก่าค้าง
        voiceSource.clip = voiceClips[index];
        voiceSource.Play();
    }

    IEnumerator FadeOutOnly(Image image)
    {
        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetAlpha(image, alpha);
            yield return null;
        }

        SetAlpha(image, 0f);
        image.gameObject.SetActive(false);
    }

    IEnumerator FadeImages(Image fromImage, Image toImage)
    {
        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        blackScreenImage.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / (fadeDuration / 2f));
            SetAlpha(blackScreenImage, alpha);
            yield return null;
        }

        SetAlpha(fromImage, 0f);
        fromImage.gameObject.SetActive(false);

        toImage.gameObject.SetActive(true);
        SetAlpha(toImage, 1f);

        timer = 0f;
        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / (fadeDuration / 2f));
            SetAlpha(blackScreenImage, alpha);
            yield return null;
        }

        SetAlpha(blackScreenImage, 0f);
        blackScreenImage.gameObject.SetActive(false);
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
            var c = dialogueText.color;
            c.a = alpha;
            dialogueText.color = c;
        }
        if (speakerText != null)
        {
            var c = speakerText.color;
            c.a = alpha;
            speakerText.color = c;
        }
    }

    void SkipToGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
