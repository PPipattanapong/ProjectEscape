using UnityEngine;
using UnityEngine.UI;      // ✅ ต้องมีบรรทัดนี้
using System.Collections;
using System.Collections.Generic;


public class LineConnector : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCamera;           // กล้องหลัก
    public LineRenderer linePrefab;     // พรีแฟบของเส้น (LineRenderer)
    public Transform lineParent;        // ที่เก็บเส้นทั้งหมด
    public GameObject rewardObject;     // 🎁 วัตถุที่จะโผล่มาหลังจับคู่ครบ
    public float fadeDuration = 1.5f;   // ระยะเวลาที่ใช้ fade in

    [Header("Debug")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;

    private LineRenderer currentLine;
    private Transform startPoint;
    private Dictionary<string, string> correctPairs = new Dictionary<string, string>();
    private List<string> usedPoints = new List<string>();
    private bool puzzleSolved = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        // ✅ ตั้งชื่อคู่ที่ถูกต้องไว้ที่นี่
        correctPairs.Add("A", "1");
        correctPairs.Add("B", "2");
        correctPairs.Add("C", "3");
        correctPairs.Add("D", "4");
        correctPairs.Add("E", "5");

        if (rewardObject != null)
            rewardObject.SetActive(false); // ซ่อนของรางวัลไว้ก่อน
    }

    void Update()
    {
        if (puzzleSolved) return;

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
                currentLine.SetPosition(0, startPoint.position);
                currentLine.SetPosition(1, startPoint.position);
            }
        }

        if (Input.GetMouseButton(0) && currentLine != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            currentLine.SetPosition(1, mousePos);
        }

        if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.CompareTag("EndPoint"))
            {
                string startName = startPoint.name;
                string endName = hit.name;

                if (correctPairs.ContainsKey(startName) && correctPairs[startName] == endName)
                {
                    // ✅ ถูก
                    currentLine.startColor = correctColor;
                    currentLine.endColor = correctColor;
                    currentLine.SetPosition(1, hit.transform.position);

                    // ปิด GameObject ทั้งสองจุด
                    startPoint.gameObject.SetActive(false);
                    hit.gameObject.SetActive(false);

                    // เพิ่มลงใน used list
                    usedPoints.Add(startName);
                    usedPoints.Add(endName);

                    // ✅ ถ้าจับคู่ครบทั้งหมด → เรียก FadeIn
                    if (usedPoints.Count >= correctPairs.Count * 2 && !puzzleSolved)
                    {
                        puzzleSolved = true;
                        if (rewardObject != null)
                            StartCoroutine(FadeInObject(rewardObject, fadeDuration));
                    }
                }
                else
                {
                    // ❌ ผิด
                    StartCoroutine(DeleteLineAfter(currentLine, 0.3f));
                }
            }
            else
            {
                // ไม่ได้ปล่อยใส่จุด
                Destroy(currentLine.gameObject);
            }

            startPoint = null;
            currentLine = null;
        }
    }

    private IEnumerator FadeInObject(GameObject obj, float duration)
    {
        obj.SetActive(true);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Image img = obj.GetComponent<Image>();

        float t = 0f;
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;

            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                sr.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
        }
        else if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;

            while (t < duration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / duration);
                img.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
        }
    }

    private IEnumerator DeleteLineAfter(LineRenderer line, float delay)
    {
        line.startColor = wrongColor;
        line.endColor = wrongColor;
        yield return new WaitForSeconds(delay);
        Destroy(line.gameObject);
    }
}
