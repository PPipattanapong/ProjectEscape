using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ColorConnectPuzzle : MonoBehaviour
{
    [Header("Grid Settings")]
    [Min(2)] public int gridSize = 5;
    public GameObject cellPrefab;
    public Transform gridParent;

    [Header("Colors (0=Red,1=Blue,2=Yellow,3=Green)")]
    public Color[] colors = new Color[4];

    [Header("Line Settings")]
    public LineRenderer linePrefab;
    public Transform lineParent;

    [Header("Panel / Trigger")]
    public GameObject puzzlePanel;
    public GameObject puzzleObject;

    [Header("Reward Settings")]
    public TextMeshPro rewardText;           // ✅ เพิ่มข้อความรางวัล
    public float rewardFadeDuration = 1.5f;  // ความเร็วในการ fade

    [Header("Close Settings")]
    public float fadeDuration = 0.25f;

    private Cell[,] cells;
    private int[,] occupied;
    private Dictionary<int, List<Cell>> lockedPaths = new();

    private bool isDragging = false;
    private int dragColorIndex = -1;
    private List<Cell> currentPath = new();
    private LineRenderer currentLine;

    private const int totalPairs = 4;
    private int solvedPairs = 0;
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

        // 🔹 ซ่อน reward ไว้ก่อน
        if (rewardText != null)
        {
            Color c = rewardText.color;
            c.a = 0f;
            rewardText.color = c;
            rewardText.gameObject.SetActive(false);
        }

        for (int i = 0; i < colors.Length; i++)
        {
            var c = colors[i];
            if (c.a <= 0f) { c.a = 1f; colors[i] = c; }
        }

        GenerateGrid();
        PlaceFixedColorPoints_Wuwa();
        InitOccupancy();
    }

    void Update()
    {
        // เปิด panel เมื่อคลิกวัตถุ
        if (Input.GetMouseButtonDown(0) && !isOpen && !puzzleSolved)
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == puzzleObject)
            {
                puzzlePanel.SetActive(true);
                isOpen = true;
                return;
            }
        }

        // ปิด panel เมื่อคลิคนอก
        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverPanel())
                CloseImmediately();
        }
    }

    void GenerateGrid()
    {
        cells = new Cell[gridSize, gridSize];
        for (int y = gridSize - 1; y >= 0; y--) // 👈 เริ่มจากบนสุดลงล่าง
        {
            for (int x = 0; x < gridSize; x++)
            {
                var go = Instantiate(cellPrefab, gridParent);
                var cell = go.GetComponent<Cell>();
                cell.Setup(x, y, this);
                cells[x, y] = cell;
            }
        }
    }

    void InitOccupancy()
    {
        occupied = new int[gridSize, gridSize];
        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                occupied[x, y] = -1;

        for (int y = 0; y < gridSize; y++)
            for (int x = 0; x < gridSize; x++)
                if (cells[x, y].isEndpoint)
                    occupied[x, y] = cells[x, y].colorIndex;
    }

    // จุด start-end แบบ Wuthering Waves
    void PlaceFixedColorPoints_Wuwa()
    {
        // 5x5 grid (0,0) = ล่างซ้าย / (4,4) = บนขวา

        // 🔵 Blue
        SetEndpoint(0, 3, 1);
        SetEndpoint(2, 4, 1);

        // 🔴 Red
        SetEndpoint(3, 4, 0);
        SetEndpoint(1, 1, 0);

        // 🟢 Green
        SetEndpoint(0, 1, 3);
        SetEndpoint(2, 2, 3);

        // 🟡 Yellow
        SetEndpoint(3, 1, 2);
        SetEndpoint(4, 2, 2);
    }

    void SetEndpoint(int x, int y, int colorIdx)
    {
        if (x < 0 || y < 0 || x >= gridSize || y >= gridSize) return;
        var c = cells[x, y];
        c.SetEndpoint(colors[colorIdx], colorIdx);
    }

    // ==== Drag ====
    public void OnCellDown(Cell cell)
    {
        if (!isOpen || !cell.isEndpoint) return;
        if (lockedPaths.ContainsKey(cell.colorIndex)) return;

        AbortCurrentDrag();

        isDragging = true;
        dragColorIndex = cell.colorIndex;
        currentPath.Clear();
        currentPath.Add(cell);

        currentLine = Instantiate(linePrefab, lineParent);
        currentLine.useWorldSpace = true;
        currentLine.startColor = currentLine.endColor = colors[dragColorIndex];
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, cell.transform.position);
    }

    public void OnCellEnter(Cell cell)
    {
        if (!isDragging) return;
        var last = currentPath[currentPath.Count - 1];
        if (!IsAdjacent(last, cell)) return;
        if (occupied[cell.x, cell.y] != -1 && occupied[cell.x, cell.y] != dragColorIndex) return;

        if (currentPath.Count >= 2 && cell == currentPath[currentPath.Count - 2])
        {
            RemoveLastStep();
            return;
        }

        if (cell.isEndpoint && cell.colorIndex == dragColorIndex && cell != currentPath[0])
        {
            AddStep(cell);
            LockCurrentPath();
            return;
        }

        if (cell.isEndpoint) return;
        AddStep(cell);
    }

    public void OnCellUp(Cell cell)
    {
        if (!isDragging) return;
        if (!(cell.isEndpoint && cell.colorIndex == dragColorIndex && cell != currentPath[0]))
        {
            AbortCurrentDrag();
            CloseImmediately();
        }
    }

    void AddStep(Cell cell)
    {
        currentPath.Add(cell);
        if (!cell.isEndpoint)
        {
            cell.SetTempColor(colors[dragColorIndex]);
            occupied[cell.x, cell.y] = dragColorIndex;
        }

        currentLine.positionCount = currentPath.Count;
        currentLine.SetPosition(currentPath.Count - 1, cell.transform.position);
    }

    void RemoveLastStep()
    {
        var last = currentPath[^1];
        if (!last.isEndpoint)
        {
            last.ResetTempColor();
            occupied[last.x, last.y] = -1;
        }
        currentPath.RemoveAt(currentPath.Count - 1);
        currentLine.positionCount = currentPath.Count;
    }

    void LockCurrentPath()
    {
        foreach (var c in currentPath)
        {
            if (!c.isEndpoint)
            {
                c.SetPermanentColor(colors[dragColorIndex]);
                occupied[c.x, c.y] = dragColorIndex;
            }
        }

        lockedPaths[dragColorIndex] = new List<Cell>(currentPath);

        isDragging = false;
        dragColorIndex = -1;
        currentPath.Clear();
        currentLine = null;

        solvedPairs++;
        if (solvedPairs >= totalPairs)
        {
            puzzleSolved = true;
            StartCoroutine(ShowReward());
            StartCoroutine(ClosePanel());

            var light = FindObjectOfType<SafeProgressLight>();
            if (light != null)
                light.MarkPuzzleComplete();
        }
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
        Vector3 targetPos = startPos + new Vector3(0, 0.25f, 0); // ค่อยๆลอยขึ้น

        while (t < rewardFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / rewardFadeDuration);
            rewardText.color = new Color(c.r, c.g, c.b, alpha);
            rewardText.transform.position = Vector3.Lerp(startPos, targetPos, alpha);
            yield return null;
        }
    }

    void AbortCurrentDrag()
    {
        if (currentLine) Destroy(currentLine.gameObject);
        isDragging = false;
        dragColorIndex = -1;
        currentPath.Clear();
        currentLine = null;
    }

    bool IsAdjacent(Cell a, Cell b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    bool IsPointerOverPanel()
    {
        if (panelRect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, null);
    }

    IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(fadeDuration);
        puzzlePanel.SetActive(false);
        isOpen = false;
    }

    void ResetPuzzle()
    {
        lockedPaths.Clear();
        solvedPairs = 0;
        isDragging = false;
        dragColorIndex = -1;
        currentPath.Clear();

        if (lineParent != null)
        {
            foreach (Transform child in lineParent)
                Destroy(child.gameObject);
        }

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var c = cells[x, y];
                if (!c.isEndpoint)
                {
                    c.ResetTempColor();
                    occupied[x, y] = -1;
                }
                else
                {
                    c.SetEndpoint(colors[c.colorIndex], c.colorIndex);
                    occupied[x, y] = c.colorIndex;
                }
            }
        }
    }

    void CloseImmediately()
    {
        ResetPuzzle();
        puzzlePanel.SetActive(false);
        isOpen = false;
        AbortCurrentDrag();
    }
}
