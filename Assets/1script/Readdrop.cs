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
        // 🚫 กัน panel ถูกบังคับเปิดหลัง solved
        if (solved && panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
        }

        // ✅ ปิดเมื่อคลิกนอก panel (ก่อน solved)
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
        Debug.Log($"[BookPuzzle] OpenPuzzle called | solved = {solved}");

        if (solved || panel == null) return; // 🚫 ถ้าผ่านแล้วหรือ panel ถูกเซ็ต null → ห้ามเปิด

        panel.SetActive(true);
        currentPage = 0;
        StartCoroutine(RefreshPageAfterOpen());
    }

    private IEnumerator RefreshPageAfterOpen()
    {
        yield return new WaitForEndOfFrame(); // รอ 1 เฟรม
        ShowPage();

        if (hintText != null) hintText.ForceMeshUpdate();
        if (pageText != null) pageText.ForceMeshUpdate();
    }

    // ✅ เรียกจากปุ่ม Next
    public void NextPage()
    {
        if (solved) return;

        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            ShowPage();

            if (currentPage == pages.Length - 1)
                sawLastPage = true; // เห็นหน้าสุดท้ายแล้ว
        }
    }

    // ✅ เรียกจากปุ่ม Prev
    public void PrevPage()
    {
        if (solved) return;

        if (currentPage > 0)
        {
            currentPage--;
            ShowPage();

            // เงื่อนไขชนะ: เคยเห็นหน้าสุดท้าย + กลับมาที่หน้า 2 (index = 1)
            if (sawLastPage && currentPage == 1)
                PuzzleSolved();
        }
    }

    // แสดงเนื้อหาหน้านั้น
    private void ShowPage()
    {
        if (pageText != null)
            pageText.text = $"Page {currentPage + 1} / {pages.Length}";

        if (hintText != null && currentPage < pages.Length)
            hintText.text = pages[currentPage];

        Debug.Log($"[BookPuzzle] Now at Page {currentPage + 1}");
    }

    private void PuzzleSolved()
    {
        solved = true;

        if (noteLeft != null)
            noteLeft.SetActive(true);

        if (doorLight != null)
            doorLight.SetGreen();

        Debug.Log("Book puzzle solved! Reward unlocked.");

        // ✅ ลบ panel ออกไปเลย
        if (panel != null)
        {
            Destroy(panel);
            panel = null;
        }

        // ✅ ปิด collider ของ object นี้
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }


    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    // ตรวจว่าคลิกโดน panel หรือ element ข้างในมั้ย
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
