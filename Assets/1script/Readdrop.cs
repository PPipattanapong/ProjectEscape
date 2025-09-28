using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Readdrop : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI pageText;   // โชว์ว่าอยู่หน้าอะไร
    public TextMeshProUGUI hintText;   // เนื้อหาในหน้านั้น
    public GameObject panel;           // Panel หลักของ Puzzle

    [Header("Book Pages")]
    [TextArea(2, 5)]
    public string[] pages;             // เนื้อหาของแต่ละหน้า
    private int currentPage = 0;

    [Header("Rewards")]
    public GameObject noteLeft;        // ของรางวัล
    public LightController doorLight;  // ไฟประตู (optional)

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;  // เวลา fade in/out

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
        // ❌ ไม่ต้องไปปิด panel ใน Update หลัง solved
        if (!solved && panel != null && panel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject(panel))
            {
                ClosePanel();
            }
        }
    }

    // เปิด puzzle
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
        ShowPage();

        if (hintText != null) hintText.ForceMeshUpdate();
        if (pageText != null) pageText.ForceMeshUpdate();
    }

    public void NextPage()
    {
        if (solved) return;

        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            ShowPage();

            if (currentPage == pages.Length - 1)
                sawLastPage = true;
        }
    }

    public void PrevPage()
    {
        if (solved) return;

        if (currentPage > 0)
        {
            currentPage--;
            ShowPage();

            if (sawLastPage && currentPage == 1)
                PuzzleSolved();
        }
    }

    private void ShowPage()
    {
        if (pageText != null)
            pageText.text = $"Page {currentPage + 1} / {pages.Length}";

        if (hintText != null && currentPage < pages.Length)
            hintText.text = pages[currentPage];
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

        Debug.Log("Book puzzle solved! Reward unlocked.");
    }

    private IEnumerator FadeInNoteAndFadeOutPanel(GameObject noteObj, GameObject panelObj, float duration)
    {
        noteObj.SetActive(true);

        // เตรียม Note (SpriteRenderer)
        SpriteRenderer sr = noteObj.GetComponent<SpriteRenderer>();
        Color noteColor = Color.white;
        if (sr != null)
        {
            noteColor = sr.color;
            noteColor.a = 0f;
            sr.color = noteColor;
        }

        // เตรียม Panel (Image)
        Image img = panelObj.GetComponent<Image>();
        Color panelColor = Color.white;
        if (img != null)
        {
            panelColor = img.color;
            panelColor.a = 1f;
            img.color = panelColor;
        }

        // Fade พร้อมกัน
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
