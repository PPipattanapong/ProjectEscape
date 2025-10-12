using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SafePin : MonoBehaviour
{
    [Header("Safe Settings")]
    public string correctPin = "4567";
    public TextMeshProUGUI pinDisplay;
    public GameObject keyReward;

    [Header("Requirement")]
    [Tooltip("ชื่อไอเท็มที่ต้องมีใน Inventory ก่อนถึงจะใช้เซฟได้")]
    public string requiredItemName;

    [Header("UI Panel")]
    public GameObject safePanel;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float zoomSize = 2f;
    public float zoomDuration = 1f;

    [Header("Penalty Settings")]
    [Tooltip("จำนวนวินาทีที่จะลดเมื่อกรอกรหัสผิด")]
    public float wrongCodePenalty = 10f;

    [Header("Flash Effect")]
    [Tooltip("Panel สีแดงที่จะใช้ flash ตอนกรอกรหัสผิด")]
    public GameObject damageFlashPanel; // ใช้ panel สีแดงเหมือน WirePuzzle
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    private string input = "";
    private bool solved = false;
    private SpriteRenderer safeRenderer;
    public LightController centerLight;

    private bool isZooming = false;
    private Vector3 originalCamPos;
    private float originalCamSize;
    private RoomCameraController camController;

    void Start()
    {
        if (keyReward != null) keyReward.SetActive(false);
        if (pinDisplay != null) pinDisplay.text = "";
        if (safePanel != null) safePanel.SetActive(false);

        safeRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        originalCamPos = mainCamera.transform.position;
        originalCamSize = mainCamera.orthographicSize;

        camController = mainCamera.GetComponent<RoomCameraController>();

        if (damageFlashPanel != null)
            damageFlashPanel.SetActive(false);
    }

    void Update()
    {
        if (safePanel != null && safePanel.activeSelf && !isZooming)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    ClosePanel(true);
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!solved && !isZooming && safePanel != null)
            StartCoroutine(ZoomInAndOpen());
    }

    private IEnumerator ZoomInAndOpen()
    {
        isZooming = true;
        if (camController != null) camController.enabled = false;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 targetPos = safeRenderer.bounds.center;
        targetPos.z = mainCamera.transform.position.z;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, zoomSize, t);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.orthographicSize = zoomSize;

        safePanel.SetActive(true);
        isZooming = false;
    }

    private IEnumerator ZoomOutAndClose()
    {
        isZooming = true;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            mainCamera.transform.position = Vector3.Lerp(startPos, originalCamPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, originalCamSize, t);
            yield return null;
        }

        mainCamera.transform.position = originalCamPos;
        mainCamera.orthographicSize = originalCamSize;

        yield return new WaitForSeconds(0.15f);
        if (camController != null) camController.enabled = true;

        isZooming = false;
    }

    public void PressNumber(string num)
    {
        if (solved) return;

        if (input.Length < correctPin.Length)
        {
            input += num;
            pinDisplay.text = input;
        }
    }

    public void PressBackspace()
    {
        if (solved || string.IsNullOrEmpty(input)) return;

        input = input.Substring(0, input.Length - 1);
        pinDisplay.text = input;
    }

    public void Submit()
    {
        if (solved) return;

        if (input == correctPin)
        {
            if (!HasRequiredItem())
            {
                Debug.Log("⚠️ Requirement not passed!");
                StartCoroutine(ShowTemporaryMessage("Requirement not met!", 1.5f));
                input = "";
                return;
            }

            solved = true;
            Debug.Log("✅ Safe opened!");

            if (safeRenderer != null)
                safeRenderer.sortingOrder = 0;

            if (centerLight != null)
                centerLight.SetGreen();

            if (safePanel != null)
                safePanel.SetActive(false);

            if (keyReward != null)
                StartCoroutine(FadeInReward(keyReward, 2f));

            StartCoroutine(HandleAfterSolved());
            input = "";
        }
        else
        {
            Debug.Log("❌ Wrong code!");
            StartCoroutine(ShowTemporaryMessage("Wrong code!", 1.5f, Color.red));

            // 🔻 Flash Effect ตอนรหัสผิด
            if (damageFlashPanel != null)
                StartCoroutine(FlashDamagePanel());

            WallCountdownWithImages timer = FindObjectOfType<WallCountdownWithImages>();
            if (timer != null)
            {
                timer.ReduceTime(wrongCodePenalty);
                Debug.Log($"Reduced {wrongCodePenalty} seconds.");
            }

            input = "";
        }
    }

    private bool HasRequiredItem()
    {
        if (string.IsNullOrEmpty(requiredItemName)) return true;

        InventoryManager inv = FindObjectOfType<InventoryManager>();
        if (inv == null) return false;

        return inv.HasItem(requiredItemName);
    }

    private IEnumerator ShowTemporaryMessage(string message, float duration, Color? colorOverride = null)
    {
        if (pinDisplay == null) yield break;

        pinDisplay.text = message;
        Color originalColor = pinDisplay.color;
        if (colorOverride != null)
            pinDisplay.color = colorOverride.Value;

        yield return new WaitForSeconds(duration);

        float fade = 0.5f;
        float t = 0f;
        while (t < fade)
        {
            t += Time.deltaTime;
            pinDisplay.color = new Color(pinDisplay.color.r, pinDisplay.color.g, pinDisplay.color.b, Mathf.Lerp(1f, 0f, t / fade));
            yield return null;
        }

        pinDisplay.color = originalColor;
        pinDisplay.text = "";
    }

    private IEnumerator FlashDamagePanel()
    {
        damageFlashPanel.SetActive(true);
        Image img = damageFlashPanel.GetComponent<Image>();
        if (img == null) yield break;

        Color baseColor = img.color;
        float t = 0f;

        // Fade In (เร็ว)
        while (t < flashDuration * 0.3f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, flashMaxAlpha, t / (flashDuration * 0.3f));
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        // Fade Out (ช้า)
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

    private IEnumerator HandleAfterSolved()
    {
        yield return StartCoroutine(ZoomOutAndClose());
        yield return StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeInReward(GameObject obj, float duration)
    {
        obj.SetActive(true);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 0f);

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

    private IEnumerator FadeOutAndDisable()
    {
        if (safeRenderer == null) yield break;

        float duration = 2f;
        float t = 0f;
        Color originalColor = safeRenderer.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);
            safeRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        safeRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        gameObject.SetActive(false);
    }

    public void ClosePanel(bool instantPanelHide = false)
    {
        if (safePanel != null && !isZooming)
        {
            if (instantPanelHide)
                safePanel.SetActive(false);
            StartCoroutine(ZoomOutAndClose());
        }
    }
}
