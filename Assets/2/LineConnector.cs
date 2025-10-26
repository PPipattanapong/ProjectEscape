using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LineConnector : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;              // กล้องหลัก
    public LineRenderer linePrefab;        // พรีแฟบของเส้น (LineRenderer)
    public Transform lineParent;           // ที่เก็บเส้นทั้งหมด
    public WireCutPuzzle wireCutPuzzle;    // ✅ อ้างถึงสคริปต์ WireCutPuzzle ที่จะเรียกใช้ตอนสำเร็จ

    [Header("Line Colors Per Pair")]
    public Color[] pairColors;             // สีแต่ละคู่ (A-1, B-2, ...)

    private LineRenderer currentLine;
    private Transform startPoint;

    private Dictionary<string, string> correctPairs = new Dictionary<string, string>();
    private Dictionary<string, Color> pairColorMap = new Dictionary<string, Color>();
    private List<string> usedPoints = new List<string>();

    private bool puzzleSolved = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // ✅ ตั้งคู่ที่ถูกต้อง
        correctPairs.Add("A", "1");
        correctPairs.Add("B", "2");
        correctPairs.Add("C", "3");
        correctPairs.Add("D", "4");
        correctPairs.Add("E", "5");

        // ✅ ผูกสีแต่ละคู่
        int index = 0;
        foreach (var pair in correctPairs)
        {
            Color colorToUse = Color.green;
            if (pairColors != null && index < pairColors.Length)
                colorToUse = pairColors[index];

            pairColorMap[pair.Key] = colorToUse;
            index++;
        }
    }

    void Update()
    {
        if (puzzleSolved) return;

        // เริ่มลากจากจุดเริ่มต้น
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("StartPoint") && !usedPoints.Contains(hit.name))
            {
                startPoint = hit.transform;
                currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, lineParent);
                currentLine.positionCount = 2;
                currentLine.useWorldSpace = true;

                // ✅ ตั้งค่าเริ่มเป็นสีขาว
                Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(Color.white, 0f),
                        new GradientColorKey(Color.white, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                );
                currentLine.colorGradient = g;
                currentLine.material = new Material(currentLine.material);
                currentLine.material.color = Color.white;

                currentLine.SetPosition(0, startPoint.position);
                currentLine.SetPosition(1, startPoint.position);
            }
        }

        // ระหว่างลาก
        if (Input.GetMouseButton(0) && currentLine != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            currentLine.SetPosition(1, mousePos);
        }

        // ปล่อยเมาส์
        if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("EndPoint"))
            {
                string startName = startPoint.name;
                string endName = hit.name;

                // ✅ ตรวจจับคู่ถูก
                if (correctPairs.ContainsKey(startName) && correctPairs[startName] == endName)
                {
                    Color pairColor = pairColorMap[startName];

                    // ✅ ตั้ง Gradient ใหม่เป็นสีคู่ที่เลือก
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
                    currentLine.material.color = pairColor;
                    currentLine.SetPosition(1, hit.transform.position);

                    startPoint.gameObject.SetActive(false);
                    hit.gameObject.SetActive(false);

                    usedPoints.Add(startName);
                    usedPoints.Add(endName);

                    // ✅ ถ้าต่อครบทุกคู่
                    if (usedPoints.Count >= correctPairs.Count * 2 && !puzzleSolved)
                    {
                        puzzleSolved = true;

                        Debug.Log("[LineConnector] Puzzle Solved!");

                        // ✅ เรียก WireCutPuzzle.ApplyStarColor()
                        if (wireCutPuzzle != null)
                        {
                            wireCutPuzzle.ApplyStarColor();
                            Debug.Log("[LineConnector] Called wireCutPuzzle.ApplyStarColor()");
                        }
                        else
                        {
                            Debug.LogWarning("[LineConnector] ⚠️ WireCutPuzzle not assigned in Inspector!");
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
