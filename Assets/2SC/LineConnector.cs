using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LineConnector : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;
    public LineRenderer linePrefab;
    public Transform lineParent;
    public WireCutPuzzle wireCutPuzzle;

    [Header("Line Colors Per Pair")]
    public Color[] pairColors;

    [Header("Success Text (3D TMP)")]
    public TextMeshPro successText;

    [Header("Success Color")]
    public Color successColor = Color.green;

    [Header("Tooltip To Remove On Success")]
    public List<GameObject> objectsToRemoveTooltip = new List<GameObject>();

    [Header("Audio Settings")]
    public AudioSource connectSuccessSound;
    public AudioSource puzzleCompleteSound;

    private LineRenderer currentLine;
    private Transform startPoint;

    private Dictionary<string, string> correctPairs = new();
    private Dictionary<string, Color> pairColorMap = new();
    private List<string> usedPoints = new();

    private bool puzzleSolved = false;


    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (successText != null)
            successText.gameObject.SetActive(false);

        // คู่ที่ถูกต้อง
        correctPairs.Add("A", "1");
        correctPairs.Add("B", "2");
        correctPairs.Add("C", "3");
        correctPairs.Add("D", "4");
        correctPairs.Add("E", "5");

        // สีตามลำดับคู่
        int index = 0;
        foreach (var pair in correctPairs)
        {
            Color c = Color.green;
            if (pairColors != null && index < pairColors.Length)
                c = pairColors[index];

            pairColorMap[pair.Key] = c;
            index++;
        }
    }

    void Update()
    {
        if (puzzleSolved) return;

        // ---------------- CLICK DOWN ----------------
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("StartPoint") && !usedPoints.Contains(hit.name))
            {
                startPoint = hit.transform;

                // สร้างเส้นใหม่
                currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, lineParent);
                currentLine.positionCount = 2;
                currentLine.useWorldSpace = true;

                // เริ่มด้วยสีดำ (ยังไม่เชื่อม)
                Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.black, 0f),
                        new GradientColorKey(Color.black, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                );

                currentLine.colorGradient = g;

                // ⭐ Clone material จาก sharedMaterial ไม่แตะ prefab โดยตรง
                currentLine.material = new Material(linePrefab.sharedMaterial);
                currentLine.material.color = Color.black;

                currentLine.SetPosition(0, startPoint.position);
                currentLine.SetPosition(1, startPoint.position);
            }
        }

        // ---------------- DRAG ----------------
        if (Input.GetMouseButton(0) && currentLine != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            currentLine.SetPosition(1, mousePos);
        }

        // ---------------- RELEASE ----------------
        if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

            Collider2D endPointHit = null;

            foreach (var h in hits)
            {
                if (h.collider != null && h.collider.CompareTag("EndPoint"))
                {
                    endPointHit = h.collider;
                    break;
                }
            }

            if (endPointHit != null)
            {
                string startName = startPoint.name;
                string endName = endPointHit.name;

                if (correctPairs.ContainsKey(startName) && correctPairs[startName] == endName)
                {
                    // เสียงสำเร็จทีละเส้น
                    connectSuccessSound?.Play();

                    // เปลี่ยนสีเส้นตามคู่ที่ถูกต้อง
                    Color pairColor = pairColorMap[startName];

                    Gradient successGradient = new Gradient();
                    successGradient.SetKeys(
                        new GradientColorKey[] {
                            new GradientColorKey(pairColor, 0f),
                            new GradientColorKey(pairColor, 1f)
                        },
                        new GradientAlphaKey[] {
                            new GradientAlphaKey(1f, 0f),
                            new GradientAlphaKey(1f, 1f)
                        }
                    );

                    currentLine.colorGradient = successGradient;

                    // ⭐ เปลี่ยนสี material ของ instance ได้เลย
                    currentLine.material.color = pairColor;

                    currentLine.SetPosition(1, endPointHit.transform.position);

                    // ปิด node
                    startPoint.gameObject.SetActive(false);
                    endPointHit.gameObject.SetActive(false);

                    usedPoints.Add(startName);
                    usedPoints.Add(endName);

                    // ---- Puzzle Completed ----
                    if (usedPoints.Count >= correctPairs.Count * 2)
                    {
                        puzzleSolved = true;

                        puzzleCompleteSound?.Play();
                        wireCutPuzzle?.ApplyStarColor();

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
                                if (t != null) Destroy(t);
                            }
                        }
                    }
                }
                else
                {
                    Destroy(currentLine.gameObject);
                }
            }
            else
            {
                Destroy(currentLine.gameObject);
            }

            startPoint = null;
            currentLine = null;
        }
    }
}
