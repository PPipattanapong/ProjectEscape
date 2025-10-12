using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    public GameObject destroyWhenSolved;   // 👈 ใส่ใน Inspector ได้เลย (เช่น hint วิบวับ)

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

        if (hintTextLeft != null && currentPage < pages.Length)
            hintTextLeft.text = pages[currentPage];

        if (pageTextRight != null)
        {
            if (currentPage + 1 < pages.Length)
                pageTextRight.text = $"{currentPage + 2}/{pages.Length}";
            else
                pageTextRight.text = "-";
        }

        if (hintTextRight != null)
        {
            if (currentPage + 1 < pages.Length)
                hintTextRight.text = pages[currentPage + 1];
            else
                hintTextRight.text = "";
        }
    }

    private void PuzzleSolved()
    {
        solved = true;

        if (noteLeft != null && panel != null)
            StartCoroutine(FadeInNoteAndFadeOutPanel(noteLeft, panel, fadeDuration));

        if (doorLight != null)
            doorLight.SetGreen();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 🟡 เรียกให้ลบ object ที่ตั้งไว้ใน Inspector (แบบ fade-out)
        if (destroyWhenSolved != null)
            StartCoroutine(FadeAndDestroy(destroyWhenSolved, fadeDuration));

        Debug.Log("Book puzzle solved! Reward unlocked.");
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        // รองรับทั้ง SpriteRenderer และ Image
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
        Color noteColor = Color.white;
        if (sr != null)
        {
            noteColor = sr.color;
            noteColor.a = 0f;
            sr.color = noteColor;
        }

        Image img = panelObj.GetComponent<Image>();
        Color panelColor = Color.white;
        if (img != null)
        {
            panelColor = img.color;
            panelColor.a = 1f;
            img.color = panelColor;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = Mathf.Clamp01(t / duration);

            if (sr != null)
                sr.color = new Color(noteColor.r, noteColor.g, noteColor.b, progress);

            if (img != null)
                img.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1f - progress);

            yield return null;
        }

        if (sr != null)
            sr.color = new Color(noteColor.r, noteColor.g, noteColor.b, 1f);

        if (img != null)
            img.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0f);

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

        var results = new System.Collections.Generic.List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject == targetPanel || r.gameObject.transform.IsChildOf(targetPanel.transform))
                return true;
        }
        return false;
    }
}
