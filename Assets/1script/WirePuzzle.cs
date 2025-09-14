using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WirePuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Puzzle Parts")]
    public GameObject fieldObject;
    public GameObject startObject;
    public List<GameObject> pathObjects;
    public GameObject endObject;
    public GameObject noteRight;
    public LightController doorLight;

    [Header("Background")]
    public SpriteRenderer bgRenderer;
    public Color startColor = new Color(0.2f, 0.23f, 0.22f);
    public Color solvedColor = new Color(0f, 1f, 0.51f);
    public float fadeDuration = 2f;

    [Header("Requirements")]
    public string requiredItem = "Screwdriver";

    private bool isDragging = false;
    private bool solved = false;
    private bool startedOnStart = false;
    private bool activated = false; // 👉 ต้องใช้ไขควงก่อนถึงจะเล่น puzzle ได้

    private Collider2D fieldCollider;
    private Collider2D startCollider;
    private List<Collider2D> pathColliders = new List<Collider2D>();
    private Collider2D endCollider;

    private int currentPathIndex = 0;

    void Start()
    {
        // ซ่อนทั้งหมดตอนเริ่ม
        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects) pathObj.SetActive(false);
        endObject.SetActive(false);
        noteRight.SetActive(false);

        // set สีเริ่ม BG
        if (bgRenderer != null)
            bgRenderer.color = startColor;

        // ดึง collider
        fieldCollider = fieldObject.GetComponent<Collider2D>();
        startCollider = startObject.GetComponent<Collider2D>();
        foreach (var pathObj in pathObjects)
        {
            var col = pathObj.GetComponent<Collider2D>();
            if (col != null) pathColliders.Add(col);
        }
        endCollider = endObject.GetComponent<Collider2D>();
    }

    // 👉 ฟังก์ชันที่ถูกเรียกตอนใช้ item จาก inventory มาลงบน fusebox
    public void OnItemUsed(string itemName)
    {
        // ถ้าแก้ puzzle ได้แล้ว → ไม่รับ item อีก
        if (solved)
        {
            Debug.Log("Fusebox already solved, no need to use items anymore.");
            return;
        }

        // ยังไม่ solved → รับ item ได้
        if (itemName == requiredItem)
        {
            Debug.Log("Fusebox activated with " + itemName);
            activated = true;

            // เปิด puzzle (ซ่อน/โชว์ field ได้เรื่อย ๆ)
            fieldObject.SetActive(true);
            startObject.SetActive(true);
            foreach (var pathObj in pathObjects) pathObj.SetActive(true);
            endObject.SetActive(true);

            currentPathIndex = 0;
            startedOnStart = false;
        }
        else
        {
            Debug.Log("Wrong item: " + itemName);
        }
    }


    void Update()
    {
        if (solved || !activated) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (startCollider.OverlapPoint(mousePos))
            {
                isDragging = true;
                startedOnStart = true;
                Debug.Log("Started puzzle from StartPoint ✅");
            }
            else
            {
                isDragging = false;
                startedOnStart = false;
            }
        }

        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging && startedOnStart)
        {
            if (!fieldCollider.OverlapPoint(mousePos))
            {
                Debug.Log("ออกนอกสนาม → reset");
                ResetPuzzle();
                return;
            }

            if (currentPathIndex < pathColliders.Count)
            {
                if (pathColliders[currentPathIndex].OverlapPoint(mousePos))
                {
                    Debug.Log("ผ่าน Path " + currentPathIndex);
                    currentPathIndex++;
                }
            }

            if (currentPathIndex >= pathColliders.Count && endCollider.OverlapPoint(mousePos))
            {
                Debug.Log("Wire puzzle solved!");
                PuzzleSolved();
            }
        }
    }

    void ResetPuzzle()
    {
        isDragging = false;
        startedOnStart = false;
        currentPathIndex = 0;

        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects) pathObj.SetActive(false);
        endObject.SetActive(false);
    }

    void PuzzleSolved()
    {
        isDragging = false;
        solved = true;

        doorLight.SetGreen();
        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects) pathObj.SetActive(false);
        endObject.SetActive(false);

        Debug.Log("Wire puzzle solved!");

        if (bgRenderer != null)
            StartCoroutine(FadeBackground());
    }

    IEnumerator FadeBackground()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fadeDuration;
            bgRenderer.color = Color.Lerp(startColor, solvedColor, progress);
            yield return null;
        }

        bgRenderer.color = solvedColor;

        // หลังจาก fade เสร็จ → ค่อยๆโผล่ noteRight
        StartCoroutine(FadeIn(noteRight, 1.5f));
    }

    IEnumerator FadeIn(GameObject obj, float duration)
    {
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration);
            sr.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        sr.color = new Color(c.r, c.g, c.b, 1f);
    }
}
