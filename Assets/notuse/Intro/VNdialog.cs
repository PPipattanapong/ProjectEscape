using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VNDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public Button skipButton;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;
    public string nextSceneName = "GameScene";

    [Header("Effect References")]
    public Image blackScreenImage;   // รูปดำ
    public Image eyeImage;           // ตาปรือ
    public Image sceneImage1;        // ภาพหลังลืมตา
    public Image sceneImage2;        // ภาพสุดท้าย

    public float fadeDuration = 2f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool skipTyping = false;

    void Start()
    {
        skipButton.onClick.AddListener(SkipToGame);

        // แสดง eyeImage ตั้งแต่ต้น (Opacity 1), รูปอื่นโปร่งใส ยกเว้นดำ
        SetAlpha(blackScreenImage, 1f); // เริ่มมืด
        SetAlpha(eyeImage, 1f);         // โชว์ตั้งแต่แรก
        SetAlpha(sceneImage1, 0f);
        SetAlpha(sceneImage2, 0f);

        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnDialogueClick();
        }
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

            // ✅ ถ้าเกินบรรทัดสุดท้าย → โหลดฉาก
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

        // ลืมตา (แค่ fade out ภาพดำ)
        if (currentLineIndex == 2)
        {
            yield return StartCoroutine(FadeOutOnly(blackScreenImage));
        }

        // เปลี่ยนภาพ (จากตาปรือ → sceneImage1)
        if (currentLineIndex == 8)
        {
            yield return StartCoroutine(FadeImages(eyeImage, sceneImage1));
        }

        // เปลี่ยนภาพ (จาก sceneImage1 → sceneImage2)
        if (currentLineIndex == 11)
        {
            yield return StartCoroutine(FadeImages(sceneImage1, sceneImage2));
        }

        string line = dialogueLines[currentLineIndex];

        foreach (char c in line)
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    IEnumerator FadeOutOnly(Image image)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            SetAlpha(image, alpha);
            yield return null;
        }
        SetAlpha(image, 0f);
    }

    IEnumerator FadeImages(Image fromImage, Image toImage)
    {
        float timer = 0f;
        SetAlpha(toImage, 0f); // เริ่มจากโปร่งใส

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            SetAlpha(fromImage, Mathf.Lerp(1f, 0f, t));
            SetAlpha(toImage, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        SetAlpha(fromImage, 0f);
        SetAlpha(toImage, 1f);
    }

    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void SkipToGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
