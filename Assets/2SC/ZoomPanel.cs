using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ZoomPanel : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject targetPanel;

    [Header("Camera Settings")]
    public Camera mainCamera;
    public float zoomSize = 2f;
    public float zoomDuration = 1f;

    private bool isZooming = false;
    private bool isOpen = false;
    private Vector3 originalCamPos;
    private float originalCamSize;

    private SpriteRenderer objectRenderer;
    private RoomCameraController camController;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        originalCamPos = mainCamera.transform.position;
        originalCamSize = mainCamera.orthographicSize;

        if (targetPanel != null)
            targetPanel.SetActive(false);

        objectRenderer = GetComponent<SpriteRenderer>();
        camController = mainCamera.GetComponent<RoomCameraController>();
    }

    void Update()
    {
        // คลิกข้างนอก panel เพื่อปิด
        if (isOpen && targetPanel != null && !isZooming)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    ClosePanel();
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (!isZooming && !isOpen)
            StartCoroutine(ZoomInAndOpen());
    }

    private IEnumerator ZoomInAndOpen()
    {
        isZooming = true;
        if (camController != null) camController.enabled = false;

        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 targetPos = objectRenderer.bounds.center;
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

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);

            // ✅ เรียกให้ WireCutPuzzle ทำงานเมื่อซูมเข้าเสร็จ
            WireCutPuzzle puzzle = targetPanel.GetComponent<WireCutPuzzle>();
        }

        isZooming = false;
        isOpen = true;
    }



    public void ClosePanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);

        if (!isZooming && isOpen)
            StartCoroutine(ZoomOutAndClose());
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

        if (camController != null)
            camController.enabled = true;

        isZooming = false;
        isOpen = false;
    }
}
