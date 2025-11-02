using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WirePuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Puzzle Parts")]
    public GameObject fieldObject;
    public GameObject startObject;
    public List<GameObject> pathObjects;
    public GameObject endObject;
    public GameObject noteRight;
    public LightController doorLight;

    [Header("Drag Settings")]
    public float dragFollowSpeed = 25f;
    public float checkRadius = 0.15f;

    [Header("Requirements")]
    public string requiredItem = "Screwdriver";

    [Header("Penalty Settings")]
    [Tooltip("จำนวนวินาทีที่จะลดเมื่อผู้เล่นลากออกนอกเส้น")]
    public float outOfPathPenalty = 10f;

    [Header("Flash Effect (Penalty)")]
    [Tooltip("Panel สีแดงที่จะใช้ flash ตอนโดนลงโทษ")]
    public GameObject damageFlashPanel;
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    private bool isDragging = false;
    private bool solved = false;
    private bool activated = false;

    private Collider2D startCollider;
    private Collider2D endCollider;
    private Vector3 startOriginalPos;

    void Start()
    {
        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects) pathObj.SetActive(false);
        endObject.SetActive(false);
        noteRight.SetActive(false);

        startCollider = startObject.GetComponent<Collider2D>();
        endCollider = endObject.GetComponent<Collider2D>();
        startOriginalPos = startObject.transform.position;

        if (damageFlashPanel != null)
        {
            damageFlashPanel.SetActive(false);
            var img = damageFlashPanel.GetComponent<Image>();
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }
    }

    public void OnItemUsed(string itemName)
    {
        if (solved) return;

        if (itemName == requiredItem)
        {
            activated = true;
            fieldObject.SetActive(true);
            startObject.SetActive(true);
            foreach (var pathObj in pathObjects) pathObj.SetActive(true);
            endObject.SetActive(true);
            startObject.transform.position = startOriginalPos;

            Debug.Log("[WirePuzzle] Activated with " + itemName);
        }
    }

    void Update()
    {
        if (solved || !activated) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            if (startCollider.OverlapPoint(mouseWorldPos))
            {
                isDragging = true;
                Debug.Log("[WirePuzzle] Start dragging");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            startObject.transform.position = Vector3.Lerp(
                startObject.transform.position,
                mouseWorldPos,
                Time.deltaTime * dragFollowSpeed
            );
        }

        Vector3 pos = startObject.transform.position;

        bool touchingRed = false;
        foreach (var pathObj in pathObjects)
        {
            if (pathObj == null) continue;
            Collider2D col = pathObj.GetComponent<Collider2D>();
            if (col == null) continue;

            bool overlap = col.OverlapPoint(pos) ||
                           (Vector2.Distance(pos, col.bounds.ClosestPoint(pos)) < checkRadius);

            if (overlap)
            {
                touchingRed = true;
                break;
            }
        }

        if (!touchingRed)
        {
            Debug.LogWarning("[WirePuzzle] ❌ Out of red — Reset!");

            if (damageFlashPanel != null)
                StartCoroutine(FlashDamagePanel());

            WallCountdownWithImages timer = FindObjectOfType<WallCountdownWithImages>();
            if (timer != null)
                timer.ReduceTime(outOfPathPenalty);

            ResetPuzzle();
            return;
        }

        if (endCollider.OverlapPoint(pos))
        {
            Debug.Log("[WirePuzzle] 🎉 Reached END");
            PuzzleSolved();
        }
    }

    private IEnumerator FlashDamagePanel()
    {
        damageFlashPanel.SetActive(true);
        Image img = damageFlashPanel.GetComponent<Image>();
        if (img == null) yield break;

        Color baseColor = img.color;
        float t = 0f;

        while (t < flashDuration * 0.3f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, flashMaxAlpha, t / (flashDuration * 0.3f));
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        t = 0f;
        while (t < flashDuration * 0.7f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(flashMaxAlpha, 0f, t / (flashDuration * 0.7f));
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        damageFlashPanel.SetActive(false);
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }

    void ResetPuzzle()
    {
        isDragging = false;
        activated = false;

        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects) pathObj.SetActive(false);
        endObject.SetActive(false);
        startObject.transform.position = startOriginalPos;

        Debug.Log("[WirePuzzle] Puzzle Reset complete");
    }

    void PuzzleSolved()
    {
        isDragging = false;
        solved = true;
        doorLight.SetGreen();

        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects)
            pathObj.SetActive(false);
        endObject.SetActive(false);

        foreach (var slot in FindObjectsOfType<InventorySlot>())
        {
            if (slot.currentItem != null && slot.currentItem.itemName == requiredItem)
            {
                slot.ClearSlot();
                Debug.Log("[WirePuzzle] Removed required item");
                break;
            }
        }

        noteRight.SetActive(true);
    }

    void OnDrawGizmosSelected()
    {
        if (startObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startObject.transform.position, checkRadius);
        }
    }
}
