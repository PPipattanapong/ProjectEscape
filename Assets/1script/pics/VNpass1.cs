using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Vnpass1 : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public Button skipButton;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;
    public string nextSceneName = "GameScene";

    [Header("Images")]
    public Image imgPant;   // 0 หอบ
    public Image imgLight;  // 1-2 เจอแสง
    public Image imgHand;   // 3-4 แสดงมือ
    public Image imgDoor;   // 5-7 ประตู

    public float fadeDuration = 1.5f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool skipTyping = false;

    private Image currentImage; // รูปที่แสดงอยู่ตอนนี้

    void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipToGame);

        // ปิดรูปทั้งหมดก่อน
        SetAlpha(imgPant, 0f);
        SetAlpha(imgLight, 0f);
        SetAlpha(imgHand, 0f);
        SetAlpha(imgDoor, 0f);

        // เริ่มด้วยรูปแรกทันที
        currentImage = imgPant;
        SetAlpha(currentImage, 1f);

        // เริ่ม dialogue line แรก
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

        // เช็คว่าต้องเปลี่ยนรูปไหม
        Image targetImage = GetImageForLine(currentLineIndex);
        if (targetImage != null && targetImage != currentImage)
        {
            yield return StartCoroutine(FadeImages(currentImage, targetImage));
            currentImage = targetImage;
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

    Image GetImageForLine(int lineIndex)
    {
        if (lineIndex == 0) return imgPant;
        if (lineIndex >= 1 && lineIndex <= 3) return imgLight;
        if (lineIndex >= 4 && lineIndex <= 4) return imgHand;
        if (lineIndex >= 5 && lineIndex <= 7) return imgDoor;
        return null;
    }

    IEnumerator FadeImages(Image fromImage, Image toImage)
    {
        float timer = 0f;
        if (toImage != null) SetAlpha(toImage, 0f);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            if (fromImage != null) SetAlpha(fromImage, Mathf.Lerp(1f, 0f, t));
            if (toImage != null) SetAlpha(toImage, Mathf.Lerp(0f, 1f, t));

            yield return null;
        }

        if (fromImage != null) SetAlpha(fromImage, 0f);
        if (toImage != null) SetAlpha(toImage, 1f);
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
