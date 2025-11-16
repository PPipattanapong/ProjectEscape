using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class Readdrop : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI pageTextLeft;
    public TextMeshProUGUI pageTextRight;
    public TextMeshProUGUI hintTextLeft;
    public TextMeshProUGUI hintTextRight;
    public GameObject panel;

    [Header("Book Pages")]
    [TextArea(2, 5)]
    public string[] pages;
    private int currentPage = 0;

    [Header("Rewards")]
    public GameObject noteLeft;
    public LightController doorLight;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Extra Object To Destroy")]
    public GameObject destroyWhenSolved;

    [Header("Tooltip To Disable After Solved")]
    public List<GameObject> tooltipObjects = new List<GameObject>();

    private bool solved = false;
    private bool sawLastPage = false;

    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (noteLeft != null)
            noteLeft.SetActive(false);

        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    void Update()
    {
        if (!solved && panel != null && panel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject(panel))
                ClosePanel();
        }
    }

    public void OpenPuzzle()
    {
        if (solved || panel == null) return;

        panel.SetActive(true);
        currentPage = 0;

        ShowPagesInstant();
        StartCoroutine(RefreshPageAfterOpen());
    }

    private IEnumerator RefreshPageAfterOpen()
    {
        yield return new WaitForEndOfFrame();
        ShowPagesInstant();
    }

    public void NextPage()
    {
        if (solved) return;

        if (currentPage + 2 < pages.Length)
        {
            currentPage += 2;
            ShowPagesInstant();

            if (currentPage + 1 >= pages.Length - 1)
                sawLastPage = true;
        }
    }

    public void PrevPage()
    {
        if (solved) return;

        if (currentPage - 2 >= 0)
        {
            currentPage -= 2;
            ShowPagesInstant();

            if (sawLastPage && currentPage <= 0)
                PuzzleSolved();
        }
    }

    private void ShowPagesInstant()
    {
        if (pageTextLeft != null)
            pageTextLeft.text = $"{currentPage + 1}/{pages.Length}";

        if (pageTextRight != null)
        {
            if (currentPage + 1 < pages.Length)
                pageTextRight.text = $"{currentPage + 2}/{pages.Length}";
            else
                pageTextRight.text = "-";
        }

        if (hintTextLeft != null && currentPage < pages.Length)
            hintTextLeft.text = ColorAndUnderlineDigits(pages[currentPage]);

        if (hintTextRight != null)
        {
            if (currentPage + 1 < pages.Length)
                hintTextRight.text = ColorAndUnderlineDigits(pages[currentPage + 1]);
            else
                hintTextRight.text = "";
        }
    }

    private string ColorAndUnderlineDigits(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return System.Text.RegularExpressions.Regex.Replace(
            text,
            @"\b\d+\b",
            m => $"<color=red><u>{m.Value}</u></color>"
        );
    }

    private void PuzzleSolved()
    {
        solved = true;

        // 🔥 ลบ Tooltip ออกจาก objects ที่กำหนด
        foreach (GameObject obj in tooltipObjects)
        {
            if (obj == null) continue;

            Tooltip t = obj.GetComponent<Tooltip>();
            if (t != null)
                Destroy(t);    // ลบสคริปต์ Tooltip ออกไปเลย
        }

        if (noteLeft != null && panel != null)
            StartCoroutine(FadeInNoteAndFadeOutPanel(noteLeft, panel, fadeDuration));

        if (doorLight != null)
            doorLight.SetGreen();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (destroyWhenSolved != null)
            StartCoroutine(FadeAndDestroy(destroyWhenSolved, fadeDuration));

        Debug.Log("Book puzzle solved! Reward unlocked.");
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        Image img = target.GetComponent<Image>();

        if (sr == null && img == null)
        {
            Destroy(target);
            yield break;
        }

        float t = 0f;
        Color originalColor = sr ? sr.color : img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, t / duration);

            if (sr) sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            if (img) img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        Destroy(target);
    }

    private IEnumerator FadeInNoteAndFadeOutPanel(GameObject noteObj, GameObject panelObj, float duration)
    {
        noteObj.SetActive(true);

        SpriteRenderer sr = noteObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        Image img = panelObj.GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = progress;
                sr.color = c;
            }

            if (img != null)
            {
                Color c = img.color;
                c.a = 1f - progress;
                img.color = c;
            }

            yield return null;
        }

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }

        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        Destroy(panelObj);
        panel = null;
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private bool IsPointerOverUIObject(GameObject targetPanel)
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = Input.mousePosition;

        var results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == targetPanel || r.gameObject.transform.IsChildOf(targetPanel.transform))
                return true;
        }
        return false;
    }
}
