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

    [Header("Wire Reference")]
    public WireCutPuzzle wireCutPuzzle;

    [Header("Extra Object To Destroy")]
    public GameObject destroyWhenSolved;

    [Header("Success Display")]
    public TextMeshProUGUI successText;
    public Image successImage;

    [Header("Success Color")]
    public Color successColor = Color.green;

    [Header("Close Settings")]
    public float fadeDuration = 0.25f;

    [Header("Tooltip To Remove On Success")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();


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

        if (successText != null)
            successText.gameObject.SetActive(false);
        if (successImage != null)
            successImage.gameObject.SetActive(false);

        if (puzzleObject != null && puzzleObject.GetComponent<Collider2D>() == null)
            puzzleObject.AddComponent<BoxCollider2D>();

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

        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverPanel())
                CloseImmediately();
        }
    }

    void GenerateGrid()
    {
        cells = new Cell[gridSize, gridSize];
        for (int y = gridSize - 1; y >= 0; y--)
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

    void PlaceFixedColorPoints_Wuwa()
    {
        SetEndpoint(0, 3, 1); SetEndpoint(2, 4, 1);
        SetEndpoint(3, 4, 0); SetEndpoint(1, 1, 0);
        SetEndpoint(0, 1, 3); SetEndpoint(2, 2, 3);
        SetEndpoint(3, 1, 2); SetEndpoint(4, 2, 2);
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
        if (!isOpen || puzzleSolved) return;
        if (!cell.isEndpoint) return;
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
            Debug.Log("[ColorConnectPuzzle] Puzzle Solved!");

            if (wireCutPuzzle != null)
                wireCutPuzzle.ApplyPodiumColor();

            if (destroyWhenSolved != null)
                StartCoroutine(FadeAndDestroy(destroyWhenSolved, fadeDuration));

            if (successText != null)
            {
                successText.text = "SUCCESS";
                successText.color = successColor;
                successText.gameObject.SetActive(true);
            }

            if (successImage != null)
                successImage.gameObject.SetActive(true);

            // ⭐ ลบ Tooltip ของ Object ที่กำหนดไว้
            foreach (var obj in objectsToRemoveTooltip)
            {
                if (obj != null)
                {
                    Tooltip t = obj.GetComponent<Tooltip>();
                    if (t != null)
                    {
                        Destroy(t);
                        Debug.Log("[ColorConnectPuzzle] Removed Tooltip on: " + obj.name);
                    }
                }
            }
        }
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
        Color original = sr ? sr.color : img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(original.a, 0f, t / duration);

            if (sr) sr.color = new Color(original.r, original.g, original.b, alpha);
            if (img) img.color = new Color(original.r, original.g, original.b, alpha);

            yield return null;
        }

        Destroy(target);
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

    void CloseImmediately()
    {
        ResetPuzzle();
        puzzlePanel.SetActive(false);
        isOpen = false;
        AbortCurrentDrag();
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

        if (successText != null)
            successText.gameObject.SetActive(false);
        if (successImage != null)
            successImage.gameObject.SetActive(false);
    }

    void AbortCurrentDrag()
    {
        if (currentLine)
            Destroy(currentLine.gameObject);

        isDragging = false;
        dragColorIndex = -1;
        currentPath.Clear();
        currentLine = null;
    }
}
