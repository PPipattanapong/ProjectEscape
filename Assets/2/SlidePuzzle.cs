using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;

public class SlidePuzzle3x3 : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject puzzlePanel;        // Panel หลัก
    public List<Button> tiles;            // ปุ่ม 1–8
    public RectTransform emptySlot;       // ช่องว่าง
    public GameObject puzzleObject;       // ของที่ต้องคลิกเพื่อเปิด
    public Button shuffleButton;          // ปุ่ม Shuffle
    public float fadeDuration = 0.5f;

    [Header("Reward Settings")]
    public TextMeshPro rewardText;        // ✅ ข้อความรางวัล
    public float rewardFadeDuration = 1.5f;

    [Header("Cheat Settings")]
    public Button cheatButton;            // ✅ ปุ่มโกง (UI TMP Button)
    public TextMeshProUGUI cheatButtonText; // ข้อความบนปุ่ม (optional)

    private Vector2[] positions = new Vector2[9];
    private int emptyIndex = 8;
    private bool isOpen = false;
    private bool puzzleSolved = false;
    private RectTransform panelRect;

    void Start()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
            panelRect = puzzlePanel.GetComponent<RectTransform>();
        }

        if (puzzleObject != null && puzzleObject.GetComponent<Collider2D>() == null)
            puzzleObject.AddComponent<BoxCollider2D>();

        SetupGrid();
        SetupButtons();

        if (shuffleButton != null)
            shuffleButton.onClick.AddListener(ShuffleTiles);

        ShuffleTiles();

        // 🔹 ซ่อน reward ไว้ก่อน
        if (rewardText != null)
        {
            Color c = rewardText.color;
            c.a = 0f;
            rewardText.color = c;
            rewardText.gameObject.SetActive(false);
        }

        // ✅ ตั้งค่าปุ่มโกง
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
        float totalWidth = (3 * cellSize) + (2 * spacing);
        float totalHeight = (3 * cellSize) + (2 * spacing);

        Vector2 topLeft = new Vector2(-totalWidth / 2 + cellSize / 2, totalHeight / 2 - cellSize / 2);

        int idx = 0;
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
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
        // เปิด puzzle เมื่อคลิกวัตถุ
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

        // ปิด puzzle เมื่อคลิคนอก panel
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
        if (puzzleSolved) return;

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
        StartCoroutine(ShowReward());
        StartCoroutine(ClosePanel());

        var light = FindObjectOfType<SafeProgressLight>();
        if (light != null)
            light.MarkPuzzleComplete();
    }

    IEnumerator ShowReward()
    {
        if (rewardText == null) yield break;

        rewardText.gameObject.SetActive(true);
        Color c = rewardText.color;
        c.a = 0f;
        rewardText.color = c;

        float t = 0f;
        Vector3 startPos = rewardText.transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 0.2f, 0); // ลอยขึ้นนิดหน่อย

        while (t < rewardFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / rewardFadeDuration);
            rewardText.color = new Color(c.r, c.g, c.b, a);
            rewardText.transform.position = Vector3.Lerp(startPos, targetPos, a);
            yield return null;
        }
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
        int ax = a % 3, ay = a / 3;
        int bx = b % 3, by = b / 3;
        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) == 1;
    }

    void ShuffleTiles()
    {
        List<int> order = new List<int>();
        for (int i = 0; i < 9; i++) order.Add(i);

        do
        {
            for (int i = 0; i < order.Count; i++)
            {
                int rand = Random.Range(i, order.Count);
                (order[i], order[rand]) = (order[rand], order[i]);
            }
        } while (!IsSolvable(order));

        for (int i = 0; i < 8; i++)
        {
            tiles[i].GetComponent<RectTransform>().anchoredPosition = positions[order[i]];
        }

        emptyIndex = order[8];
        emptySlot.anchoredPosition = positions[emptyIndex];
        puzzleSolved = false;
    }

    bool IsSolvable(List<int> tiles)
    {
        int inversions = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = i + 1; j < 8; j++)
            {
                if (tiles[i] < 8 && tiles[j] < 8 && tiles[i] > tiles[j])
                    inversions++;
            }
        }
        return inversions % 2 == 0;
    }

    bool CheckWin()
    {
        for (int i = 0; i < 8; i++)
        {
            RectTransform tileRect = tiles[i].GetComponent<RectTransform>();
            if (Vector2.Distance(tileRect.anchoredPosition, positions[i]) > 0.1f)
                return false;
        }
        return Vector2.Distance(emptySlot.anchoredPosition, positions[8]) <= 0.1f;
    }

    IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(fadeDuration);
        puzzlePanel.SetActive(false);
        isOpen = false;
    }

    void CloseImmediately()
    {
        puzzlePanel.SetActive(false);
        isOpen = false;
        Debug.Log("❌ Puzzle closed by clicking outside the panel.");
    }
}
