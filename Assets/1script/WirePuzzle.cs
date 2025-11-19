using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class WirePuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Audio Sources")]
    public AudioSource activateAudio;   // 🔊 ตอนเปิดด้วยไขควง
    public AudioSource failAudio;       // 🔊 ตอนพลาด
    public AudioSource successAudio;    // 🔊 ตอนสำเร็จ

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
    public float outOfPathPenalty = 10f;

    [Header("Flash Effect (Penalty)")]
    public GameObject damageFlashPanel;
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    [Header("TMP (3D)")]
    public TextMeshPro tmp3D;
    [TextArea(1, 3)] public string activeMessage = "Drag along the red path.";
    [TextArea(1, 3)] public string penaltyMessage = "-10s! Stay on the path.";
    [TextArea(1, 3)] public string successMessage = "Unlocked.";
    public float textFadeDuration = 0.35f;
    public float messageHoldTime = 0.8f;

    [Header("Tooltip To Delete When Solved")]
    public List<GameObject> tooltipObjects = new List<GameObject>();

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

        if (tmp3D != null)
        {
            tmp3D.gameObject.SetActive(false);
            SetTMPAlpha(tmp3D, 0f);
            tmp3D.text = "";
        }
    }

    public void OnItemUsed(string itemName)
    {
        if (solved) return;

        if (itemName == requiredItem)
        {
            // 🔊 เสียงตอนเปิดด้วยไขควง
            if (activateAudio != null)
                activateAudio.Play();

            activated = true;
            fieldObject.SetActive(true);
            startObject.SetActive(true);
            foreach (var pathObj in pathObjects) pathObj.SetActive(true);
            endObject.SetActive(true);
            startObject.transform.position = startOriginalPos;

            if (tmp3D != null)
            {
                tmp3D.text = activeMessage;
                tmp3D.gameObject.SetActive(true);
                StartCoroutine(FadeTMP(tmp3D, 0f, 1f, textFadeDuration));
            }

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

            // 🔊 เสียงพลาด (-10 วินาที)
            if (failAudio != null)
                failAudio.Play();

            if (damageFlashPanel != null)
                StartCoroutine(FlashDamagePanel());

            WallCountdownWithImages timer = FindObjectOfType<WallCountdownWithImages>();
            if (timer != null)
                timer.ReduceTime(outOfPathPenalty);

            if (tmp3D != null && activated && !solved)
                StartCoroutine(ShowTMPBrief(tmp3D, penaltyMessage, messageHoldTime, textFadeDuration));

            ResetPuzzle();
            return;
        }

        if (endCollider.OverlapPoint(pos))
        {
            Debug.Log("[WirePuzzle] 🎉 Reached END");
            PuzzleSolved();
        }
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

        if (tmp3D != null)
        {
            tmp3D.text = "";
            tmp3D.gameObject.SetActive(false);
            SetTMPAlpha(tmp3D, 0f);
        }

        Debug.Log("[WirePuzzle] Puzzle Reset complete");
    }

    void PuzzleSolved()
    {
        isDragging = false;
        solved = true;
        doorLight.SetGreen();

        // 🔊 เสียงตอนสำเร็จ
        if (successAudio != null)
            successAudio.Play();

        fieldObject.SetActive(false);
        startObject.SetActive(false);
        foreach (var pathObj in pathObjects)
            pathObj.SetActive(false);
        endObject.SetActive(false);

        // ลบ Tooltip
        foreach (var obj in tooltipObjects)
        {
            if (obj == null) continue;

            Tooltip t = obj.GetComponent<Tooltip>();
            if (t != null)
                Destroy(t);
        }

        // ลบไอเทมไขควง
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

        if (tmp3D != null)
            StartCoroutine(ShowTMPBrief(tmp3D, successMessage, messageHoldTime, textFadeDuration, deactivateAtEnd: true));
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

    void OnDrawGizmosSelected()
    {
        if (startObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startObject.transform.position, checkRadius);
        }
    }

    // ===== TMP Helpers =====
    void SetTMPAlpha(TextMeshPro t, float a)
    {
        if (t == null) return;
        var c = t.color;
        t.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
    }

    IEnumerator FadeTMP(TextMeshPro t, float from, float to, float duration)
    {
        if (t == null) yield break;
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            float a = Mathf.Lerp(from, to, e / Mathf.Max(0.0001f, duration));
            SetTMPAlpha(t, a);
            yield return null;
        }
        SetTMPAlpha(t, to);
    }

    IEnumerator ShowTMPBrief(TextMeshPro t, string msg, float hold, float fade, bool deactivateAtEnd = false)
    {
        t.gameObject.SetActive(true);
        t.text = msg;
        yield return StartCoroutine(FadeTMP(t, t.color.a, 1f, fade));
        yield return new WaitForSeconds(hold);
        yield return StartCoroutine(FadeTMP(t, 1f, 0f, fade));
        if (deactivateAtEnd)
        {
            t.text = "";
            t.gameObject.SetActive(false);
        }
    }
}
