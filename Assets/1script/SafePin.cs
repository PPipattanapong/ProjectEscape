using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class SafePin : MonoBehaviour
{
    [Header("Safe Settings")]
    public string correctPin = "4567";
    public TextMeshProUGUI pinDisplay;
    public GameObject keyReward;

    [Header("UI Panel")]
    public GameObject safePanel;   // drag your SafePanel here

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float zoomSize = 2f;
    public float zoomDuration = 1f;

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
        if (!solved && safePanel != null && !isZooming)
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

        isZooming = false;
        if (camController != null) camController.enabled = true;
    }

    public void PressNumber(string num)
    {
        if (solved) return;
        input += num;
        pinDisplay.text = input;
    }

    public void Submit()
    {
        if (solved) return;

        if (input == correctPin)
        {
            solved = true;

            Debug.Log("Safe opened!");

            if (safeRenderer != null)
                safeRenderer.sortingOrder = 0;

            if (centerLight != null)
                centerLight.SetGreen();

            // ปิด panel ทันที
            if (safePanel != null)
                safePanel.SetActive(false);

            // ✨ keyReward เริ่ม fade-in ทันที หลังรหัสถูก
            if (keyReward != null)
                StartCoroutine(FadeInReward(keyReward, 2f));

            // เริ่ม sequence หลัง solved
            StartCoroutine(HandleAfterSolved());

            input = "";
        }
        else
        {
            pinDisplay.text = "Wrong code!";
            input = "";
        }
    }

    private IEnumerator HandleAfterSolved()
    {
        // 1) Zoom out กลับห้อง
        yield return StartCoroutine(ZoomOutAndClose());

        // 2) Fade out safe
        yield return StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeInReward(GameObject obj, float duration)
    {
        obj.SetActive(true);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        // reset alpha = 0 ก่อน fade
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
