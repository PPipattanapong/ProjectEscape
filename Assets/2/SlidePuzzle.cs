using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class SlidePuzzle4x4 : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject puzzlePanel;
    public List<Button> tiles;             // ปุ่ม 1–15
    public RectTransform emptySlot;        // ช่องว่าง
    public GameObject puzzleObject;
    public Button shuffleButton;

    [Header("Cheat Settings")]
    public Button cheatButton;
    public TextMeshProUGUI cheatButtonText;

    [Header("Wire Reference")]
    public WireCutPuzzle wireCutPuzzle;    // เรียกตอนชนะ

    [Header("Extra Object To Destroy")]
    public GameObject destroyWhenSolved;

    [Header("Success Text")]
    [Tooltip("ข้อความ SUCCESS ที่จะโชว์ตอนผ่านพัซเซิล")]
    public TextMeshProUGUI successText;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    private Vector2[] positions = new Vector2[16];
    private int emptyIndex = 15;
    private bool isOpen = false;
    private bool puzzleSolved = false;
    private RectTransform panelRect;

    void Start()
    {
        // ✅ ปิด panel และซ่อนข้อความ SUCCESS ตอนเริ่ม
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
            panelRect = puzzlePanel.GetComponent<RectTransform>();
        }

        if (successText != null)
        {
            successText.gameObject.SetActive(false); // ซ่อนไว้จนกว่าจะผ่าน
        }

        if (puzzleObject != null && puzzleObject.GetComponent<Collider2D>() == null)
            puzzleObject.AddComponent<BoxCollider2D>();

        SetupGrid();
        SetupButtons();

        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ShuffleTiles);

        ShuffleTiles();

        if (cheatButton != null)
            cheatButton.onClick.AddListener(CheatSolve);

        if (cheatButtonText != null)
            cheatButtonText.text = "CHEAT!";
    }

    void SetupGrid()
    {
        RectTransform parent = puzzlePanel.GetComponent<RectTransform>();

        foreach (var t in tiles)
        {
            RectTransform r = t.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.SetParent(parent, false);
        }

        emptySlot.anchorMin = emptySlot.anchorMax = new Vector2(0.5f, 0.5f);
        emptySlot.pivot = new Vector2(0.5f, 0.5f);
        emptySlot.SetParent(parent, false);

        float cellSize = 100f;
        float spacing = 5f;
        float totalWidth = (4 * cellSize) + (3 * spacing);
        float totalHeight = (4 * cellSize) + (3 * spacing);

        Vector2 topLeft = new Vector2(-totalWidth / 2 + cellSize / 2, totalHeight / 2 - cellSize / 2);

        int idx = 0;
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                positions[idx] = topLeft + new Vector2(x * (cellSize + spacing), -y * (cellSize + spacing));
                idx++;
            }
        }
    }

    void SetupButtons()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            int index = i;
            tiles[i].onClick.RemoveAllListeners();
            tiles[i].onClick.AddListener(() => MoveTile(index));
        }
    }

    void Update()
    {
        // ✅ เปิด panel เมื่อคลิกที่วัตถุ
        if (Input.GetMouseButtonDown(0) && !isOpen && !puzzleSolved)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == puzzleObject)
            {
                puzzlePanel.SetActive(true);
                isOpen = true;
                return;
            }
        }

        // ✅ ปิด panel เมื่อคลิกนอก panel (หลังผ่านก็ยังคลิกนอกเพื่อปิดได้)
        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverPanel())
            {
                CloseImmediately();
            }
        }
    }

    bool IsPointerOverPanel()
    {
        if (panelRect == null) return false;
        Vector2 mousePos = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, null);
    }

    void MoveTile(int index)
    {
        if (puzzleSolved) return; // หลังผ่านแล้วห้ามกดอีก

        RectTransform tileRect = tiles[index].GetComponent<RectTransform>();
        int tileIndex = GetNearestGridIndex(tileRect.anchoredPosition);

        if (IsAdjacent(tileIndex, emptyIndex))
        {
            Vector2 temp = tileRect.anchoredPosition;
            tileRect.anchoredPosition = positions[emptyIndex];
            emptySlot.anchoredPosition = temp;
            emptyIndex = tileIndex;

            if (CheckWin())
            {
                TriggerSolved();
            }
        }
    }

    void CheatSolve()
    {
        if (puzzleSolved) return;
        Debug.Log("🧩 Cheat button pressed — puzzle instantly solved!");
        TriggerSolved();
    }

    void TriggerSolved()
    {
        puzzleSolved = true;
        Debug.Log("[SlidePuzzle4x4] Puzzle Solved!");

        // ✅ เรียกใช้ WireCutPuzzle.ApplySqColor()
        if (wireCutPuzzle != null)
        {
            wireCutPuzzle.ApplySqColor();
            Debug.Log("[SlidePuzzle4x4] Called wireCutPuzzle.ApplySqColor()");
        }

        // ✅ ทำ fade และลบ object ที่ตั้งไว้
        if (destroyWhenSolved != null)
            StartCoroutine(FadeAndDestroy(destroyWhenSolved, fadeDuration));

        // ✅ แสดงข้อความ SUCCESS ทันที
        if (successText != null)
        {
            successText.text = "SUCCESS";
            successText.gameObject.SetActive(true);
        }

        // ❌ ไม่ปิด panel เอง
        // ❌ ห้ามกด tile หลังผ่านแล้ว (จัดการไว้แล้วใน MoveTile)
    }

    IEnumerator FadeAndDestroy(GameObject target, float duration)
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

            if (sr)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            if (img)
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        Destroy(target);
    }

    int GetNearestGridIndex(Vector2 pos)
    {
        float minDist = float.MaxValue;
        int bestIndex = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            float dist = Vector2.Distance(pos, positions[i]);
            if (dist < minDist)
            {
                minDist = dist;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    bool IsAdjacent(int a, int b)
    {
        int ax = a % 4, ay = a / 4;
        int bx = b % 4, by = b / 4;
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) == 1;
    }

    void ShuffleTiles()
    {
        List<int> order = new List<int>();
        for (int i = 0; i < 16; i++) order.Add(i);

        do
        {
            for (int i = 0; i < order.Count; i++)
            {
                int rand = Random.Range(i, order.Count);
                (order[i], order[rand]) = (order[rand], order[i]);
            }
        } while (!IsSolvable(order));

        for (int i = 0; i < 15; i++)
        {
            tiles[i].GetComponent<RectTransform>().anchoredPosition = positions[order[i]];
        }

        emptyIndex = order[15];
        emptySlot.anchoredPosition = positions[emptyIndex];
        puzzleSolved = false;
    }

    bool IsSolvable(List<int> order)
    {
        int inversions = 0;
        for (int i = 0; i < 15; i++)
        {
            for (int j = i + 1; j < 15; j++)
            {
                if (order[i] < 15 && order[j] < 15 && order[i] > order[j])
                    inversions++;
            }
        }

        int emptyRowFromBottom = 4 - (order.IndexOf(15) / 4);
        bool evenGrid = 4 % 2 == 0;

        if (evenGrid)
        {
            if (emptyRowFromBottom % 2 == 0)
                return inversions % 2 == 1;
            else
                return inversions % 2 == 0;
        }
        else
        {
            return inversions % 2 == 0;
        }
    }

    bool CheckWin()
    {
        for (int i = 0; i < 15; i++)
        {
            RectTransform tileRect = tiles[i].GetComponent<RectTransform>();
            if (Vector2.Distance(tileRect.anchoredPosition, positions[i]) > 0.1f)
                return false;
        }
        return Vector2.Distance(emptySlot.anchoredPosition, positions[15]) <= 0.1f;
    }

    void CloseImmediately()
    {
        puzzlePanel.SetActive(false);
        isOpen = false;
        Debug.Log("❌ Puzzle closed by clicking outside the panel.");
    }
}
