using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;   // <<< เพิ่มบรรทัดนี้
using UnityEngine.UI;


public class VNGood : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI speakerText;
    public Button skipButton;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;

    [Header("Voice Settings")]
    public AudioSource voiceSource;
    public List<AudioClip> voiceClips;

    [Header("Effect Images (index-based)")]
    public List<Image> fadeImages;

    [Header("Black Screen for Fade")]
    public Image blackScreen;

    [Header("Fade Settings")]
    public float fadeDuration = 2f;
    public float endFadeDuration = 2f;

    [System.Serializable]
    public struct FadeRule
    {
        public int dialogueIndex;
        public int fromImageIndex;
        public int toImageIndex;
    }

    [Header("Fade Rules")]
    public List<FadeRule> fadeRules;

    [Header("Final Scene Clean Up")]
    public Transform canvasRoot;
    public List<GameObject> finalUIToShow;

    [Header("Final Timing Settings")]
    public float finalDisplayTime = 5f;

    [Header("Next Scene After End")]
    public string finalSceneName = "GameScene";

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool skipTyping = false;

    private bool blockClick = false; // ห้ามผู้เล่นคลิกตอนฉาก Final ทำงาน


    void Start()
    {
        skipButton.onClick.AddListener(SkipToGame);

        for (int i = 0; i < fadeImages.Count; i++)
        {
            bool active = (i == 0);
            fadeImages[i].gameObject.SetActive(active);
            SetAlpha(fadeImages[i], active ? 1f : 0f);
        }

        SetAlpha(blackScreen, 0f);
        blackScreen.gameObject.SetActive(true);

        StartCoroutine(TypeLine());
    }


    void Update()
    {
        if (blockClick) return;

        if (Input.GetMouseButtonDown(0))
            OnDialogueClick();
    }


    void OnDialogueClick()
    {
        if (blockClick) return;

        if (isTyping)
        {
            skipTyping = true;
            return;
        }

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            StartCoroutine(EndSequence());
            return;
        }

        StartCoroutine(TypeLine());
    }


    IEnumerator TypeLine()
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        foreach (var rule in fadeRules)
        {
            if (rule.dialogueIndex == currentLineIndex)
            {
                yield return StartCoroutine(FadeImagesWithBlack(rule.fromImageIndex, rule.toImageIndex));
                break;
            }
        }

        PlayVoiceForLine(currentLineIndex);

        string line = dialogueLines[currentLineIndex];
        string speaker = "";
        string message = line;

        int colon = line.IndexOf(':');
        if (colon > 0)
        {
            speaker = line.Substring(0, colon).Trim();
            message = line.Substring(colon + 1).Trim();
        }

        speakerText.text = speaker;
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


    IEnumerator FadeImagesWithBlack(int fromIndex, int toIndex)
    {
        Image from = fadeImages[fromIndex];
        Image to = fadeImages[toIndex];

        dialogueText.text = "";
        speakerText.text = "";
        SetTextAlpha(0f);

        blackScreen.gameObject.SetActive(true);
        float half = fadeDuration / 2f;
        float timer = 0f;

        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            SetAlpha(blackScreen, Mathf.Lerp(0f, 1f, t));
            yield return null;
        }

        SetAlpha(from, 0f);
        from.gameObject.SetActive(false);

        to.gameObject.SetActive(true);
        SetAlpha(to, 1f);

        timer = 0f;
        while (timer < half)
        {
            timer += Time.deltaTime;
            float t = timer / half;
            SetAlpha(blackScreen, Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        SetAlpha(blackScreen, 0f);
    }


    IEnumerator EndSequence()
    {
        blockClick = true; // ห้ามคลิกเพิ่ม

        float timer = 0f;
        while (timer < endFadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / endFadeDuration;
            SetAlpha(blackScreen, t);
            yield return null;
        }

        foreach (Transform child in canvasRoot)
            child.gameObject.SetActive(false);

        if (finalUIToShow.Count > 0)
        {
            finalUIToShow[0].SetActive(true);

            if (finalUIToShow[0].TryGetComponent<Image>(out Image img0))
                SetAlpha(img0, 1f);

            if (finalUIToShow[0].TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmp0))
            {
                var c = tmp0.color; c.a = 1f;
                tmp0.color = c;
            }
        }

        GameObject ui1 = null;
        if (finalUIToShow.Count > 1)
            ui1 = finalUIToShow[1];

        if (ui1 != null)
        {
            if (ui1.TryGetComponent<Image>(out Image img1))
                SetAlpha(img1, 0f);

            if (ui1.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmp1))
            {
                var c = tmp1.color; c.a = 0f;
                tmp1.color = c;
            }

            ui1.SetActive(true);

            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;

                if (ui1.TryGetComponent<Image>(out Image img))
                    SetAlpha(img, t);

                if (ui1.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmp))
                {
                    var c = tmp.color; c.a = t; tmp.color = c;
                }

                yield return null;
            }

            yield return new WaitForSeconds(finalDisplayTime);

            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;

                if (ui1.TryGetComponent<Image>(out Image img))
                    SetAlpha(img, 1f - t);

                if (ui1.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI tmp2))
                {
                    var c = tmp2.color; c.a = 1f - t; tmp2.color = c;
                }

                yield return null;
            }
        }

        SceneManager.LoadScene(finalSceneName);
    }


    void PlayVoiceForLine(int index)
    {
        if (voiceSource == null) return;
        if (index >= voiceClips.Count) return;
        if (voiceClips[index] == null) return;

        voiceSource.Stop();
        voiceSource.clip = voiceClips[index];
        voiceSource.Play();
    }


    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color; c.a = alpha;
        img.color = c;
    }


    void SetTextAlpha(float alpha)
    {
        var a = dialogueText.color; a.a = alpha; dialogueText.color = a;
        var b = speakerText.color; b.a = alpha; speakerText.color = b;
    }


    // ===========================
    //      NEW SKIP BUTTON
    // ===========================
    void SkipToGame()
    {
        if (blockClick) return;

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        isTyping = false;
        skipTyping = true;

        currentLineIndex = dialogueLines.Length - 1;

        StartCoroutine(EndSequence());
    }
}
