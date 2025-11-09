using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Vnpass1 : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;   // ข้อความบทพูด
    public TextMeshProUGUI speakerText;    // ชื่อผู้พูด
    public Button skipButton;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;
    public string nextSceneName = "GameScene";

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

        // ✅ ตั้งค่าเริ่มต้น — Scene1 โผล่ทันที, ไม่ต้องดำ
        SetAlpha(Black, 0f);
        SetAlpha(Scene1, 1f);
        SetAlpha(Scene2, 0f);
        SetAlpha(Scene3, 0f);
        SetAlpha(Scene4, 0f);

        Scene1.gameObject.SetActive(true);
        Scene2.gameObject.SetActive(false);
        Scene3.gameObject.SetActive(false);
        Scene4.gameObject.SetActive(false);

        // ✅ เริ่มพิมพ์บทพูดเลย
        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnDialogueClick();
    }

    void OnDialogueClick()
    {
        if (isTyping)
        {
            skipTyping = true;
        }
        else
        {
            currentLineIndex++;

            if (currentLineIndex >= dialogueLines.Length)
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            StartCoroutine(TypeLine());
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        // 🔹 ตรวจว่าบรรทัดนี้มีการเปลี่ยนภาพไหม
        bool shouldFade = (currentLineIndex == 1 || currentLineIndex == 4 || currentLineIndex == 6);

        if (currentLineIndex == 1)
            yield return StartCoroutine(FadeImages(Scene1, Scene2));

        if (currentLineIndex == 4)
            yield return StartCoroutine(FadeImages(Scene2, Scene3));

        if (currentLineIndex == 6)
            yield return StartCoroutine(FadeImages(Scene3, Scene4));

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

        // 🔹 fade in ข้อความหลังเปลี่ยนภาพ
        if (shouldFade)
            yield return StartCoroutine(FadeInText());
        else
            SetTextAlpha(1f);

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

    IEnumerator FadeImages(Image fromImage, Image toImage)
    {
        // 🔹 ซ่อนข้อความก่อน
        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        // ✅ ทำให้จอดำ fade-in ปิดจอ
        Black.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / (fadeDuration / 2f));
            SetAlpha(Black, alpha);
            yield return null;
        }

        // ✅ เมื่อดำเต็มแล้ว: ปิดภาพเก่า เปิดภาพใหม่
        SetAlpha(fromImage, 0f);
        fromImage.gameObject.SetActive(false);
        toImage.gameObject.SetActive(true);
        SetAlpha(toImage, 1f);

        // ✅ แล้วค่อย fade-out จอดำออก
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

    IEnumerator FadeInText()
    {
        float timer = 0f;
        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / (fadeDuration / 2f));
            SetTextAlpha(alpha);
            yield return null;
        }
        SetTextAlpha(1f);
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
