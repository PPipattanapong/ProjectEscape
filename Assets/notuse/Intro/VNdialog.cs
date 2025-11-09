using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VNDialogueManager : MonoBehaviour
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

        // ✅ ตั้งค่าเริ่มต้น
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

        // 🔹 ตรวจว่าเป็นบรรทัดที่มีการเปลี่ยนภาพไหม
        bool shouldFade = (currentLineIndex == 2 || currentLineIndex == 6 || currentLineIndex == 10);

        if (currentLineIndex == 2)
            yield return StartCoroutine(FadeOutOnly(blackScreenImage));

        if (currentLineIndex == 6)
            yield return StartCoroutine(FadeImages(eyeImage, sceneImage1));

        if (currentLineIndex == 10)
            yield return StartCoroutine(FadeImages(sceneImage1, sceneImage2));

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

        // 🔹 Fade in เฉพาะกรณีที่มีการสลับภาพ
        if (shouldFade)
            yield return StartCoroutine(FadeInText());
        else
            SetTextAlpha(1f); // ปกติให้แสดงทันที

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

    IEnumerator FadeOutOnly(Image image)
    {
        // 🔹 ซ่อนข้อความและชื่อผู้พูดก่อนเริ่ม fade
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
        // 🔹 ซ่อนข้อความและชื่อผู้พูดก่อนเริ่ม fade
        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        // ✅ ขั้นแรก: ทำให้ภาพดำ fade-in ปิดจอ
        blackScreenImage.gameObject.SetActive(true);
        float timer = 0f;

        while (timer < fadeDuration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / (fadeDuration / 2f));
            SetAlpha(blackScreenImage, alpha);
            yield return null;
        }

        // ✅ พอจอดำแล้ว: ปิดภาพเก่า เปิดภาพใหม่
        SetAlpha(fromImage, 0f);
        fromImage.gameObject.SetActive(false);
        toImage.gameObject.SetActive(true);
        SetAlpha(toImage, 1f);

        // ✅ แล้วค่อย fade-out จากดำกลับมาภาพใหม่
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
