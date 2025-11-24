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
    public List<Button> tiles;
    public RectTransform emptySlot;
    public GameObject puzzleObject;
    public Button shuffleButton;

    [Header("Close Button")]
    public Button closeButton;

    [Header("Audio Settings")]
    public AudioSource openSound;
    public AudioSource winSound;

    [Header("Wire Reference")]
    public WireCutPuzzle wireCutPuzzle;

    [Header("Extra Object To Destroy")]
    public GameObject destroyWhenSolved;

    [Header("Success Text")]
    public TextMeshProUGUI successText;

    [Header("Success Color")]
    public Color successColor = Color.green;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Tooltip To Remove On Success")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    private Vector2[] positions = new Vector2[16];
    private int emptyIndex = 15;
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

        if (puzzleObject != null && puzzleObject.GetComponent<Collider2D>() == null)
            puzzleObject.AddComponent<BoxCollider2D>();

        SetupGrid();
        SetupButtons();

        if (shuffleButton != null)
        {
            shuffleButton.onClick.RemoveAllListeners();
            shuffleButton.onClick.AddListener(ResetTilesToStart);

            TextMeshProUGUI btnText = shuffleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = "RESET";
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        ResetTilesToStart();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isOpen && !puzzleSolved)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == puzzleObject)
            {
                puzzlePanel.SetActive(true);
                isOpen = true;

                if (openSound != null)
                    openSound.Play();

                return;
            }
        }

        // Cheat
        if (!puzzleSolved && Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🧩 CHEAT: Solved by pressing T");
            TriggerSolved();
        }
    }

    void ClosePanel()
    {
        puzzlePanel.SetActive(false);
        isOpen = false;
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

        Vector2 topLeft = new Vector2(
            -totalWidth / 2 + cellSize / 2,
            totalHeight / 2 - cellSize / 2
        );

        int idx = 0;
        for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
                positions[idx++] = topLeft + new Vector2(x * (cellSize + spacing), -y * (cellSize + spacing));
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
                TriggerSolved();
        }
    }

    void TriggerSolved()
    {
        puzzleSolved = true;
        Debug.Log("[SlidePuzzle4x4] Puzzle Solved!");

        if (winSound != null)
            winSound.Play();

        if (wireCutPuzzle != null)
            wireCutPuzzle.ApplySqColor();

        if (destroyWhenSolved != null)
            StartCoroutine(FadeAndDestroy(destroyWhenSolved, fadeDuration));

        if (successText != null)
        {
            successText.text = "SUCCESS";
            successText.color = successColor;
            successText.gameObject.SetActive(true);
        }

        foreach (var obj in objectsToRemoveTooltip)
        {
            if (obj != null)
            {
                Tooltip t = obj.GetComponent<Tooltip>();
                if (t != null)
                    Destroy(t);
            }
        }

        // 🔥 ปิด RESET หลังชนะ (ของจริงอยู่ตรงนี้)
        if (shuffleButton != null)
            shuffleButton.interactable = false;

        DisablePuzzleForever();
    }

    void DisablePuzzleForever()
    {
        if (puzzleObject != null)
        {
            Collider2D col = puzzleObject.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }

        isOpen = true;
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

    void ResetTilesToStart()
    {
        Debug.Log("🔁 Reset puzzle to custom starting layout.");

        // แถว 1: 1 2 3 4
        tiles[0].GetComponent<RectTransform>().anchoredPosition = positions[0];
        tiles[1].GetComponent<RectTransform>().anchoredPosition = positions[1];
        tiles[2].GetComponent<RectTransform>().anchoredPosition = positions[2];
        tiles[3].GetComponent<RectTransform>().anchoredPosition = positions[3];

        // แถว 2: 5 6 7 8
        tiles[4].GetComponent<RectTransform>().anchoredPosition = positions[4];
        tiles[5].GetComponent<RectTransform>().anchoredPosition = positions[5];
        tiles[6].GetComponent<RectTransform>().anchoredPosition = positions[6];
        tiles[7].GetComponent<RectTransform>().anchoredPosition = positions[7];

        // แถว 3: 9 emp 15 14
        tiles[8].GetComponent<RectTransform>().anchoredPosition = positions[8];   // 9
        emptySlot.anchoredPosition = positions[9];                                // emp
        emptyIndex = 9;

        tiles[14].GetComponent<RectTransform>().anchoredPosition = positions[10]; // 15
        tiles[13].GetComponent<RectTransform>().anchoredPosition = positions[11]; // 14

        // แถว 4: 13 12 11 10
        tiles[12].GetComponent<RectTransform>().anchoredPosition = positions[12]; // 13
        tiles[11].GetComponent<RectTransform>().anchoredPosition = positions[13]; // 12
        tiles[10].GetComponent<RectTransform>().anchoredPosition = positions[14]; // 11
        tiles[9].GetComponent<RectTransform>().anchoredPosition = positions[15];  // 10

        puzzleSolved = false;

        if (successText != null)
            successText.gameObject.SetActive(false);
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
}
