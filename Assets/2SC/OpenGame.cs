using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ObjectClickOpener : MonoBehaviour
{
    [Header("Object to Open")]
    public GameObject targetObject;
    private Camera mainCam;

    [Header("WireCutter Reference")]
    public GameObject wireCutter;  // ⭐ โยงคีมตัวนี้ใน Inspector

    [Header("3D TextMeshPro List (World Space)")]
    public List<TextMeshPro> worldTexts = new List<TextMeshPro>();

    [Header("Extra Objects To Destroy (Optional)")]
    public List<GameObject> destroyWhenOpened = new List<GameObject>();

    [Header("Audio")]
    public AudioSource clickSound;

    private bool textShown = false;   // ⭐ กันโชว์ซ้ำ

    void Start()
    {
        mainCam = Camera.main;

        if (targetObject != null)
            targetObject.SetActive(false);

        foreach (var obj in destroyWhenOpened)
            if (obj != null) obj.SetActive(true);

        foreach (var t in worldTexts)
            if (t != null) t.gameObject.SetActive(false);
    }

    void Update()
    {
        // ===========================
        // ⭐ เช็คว่าคีมถูกเก็บแล้วไหม
        // ===========================
        if (!textShown && wireCutter != null && !wireCutter.activeSelf)
        {
            Debug.Log("<color=green>[DEBUG] Cutter detected collected → Show text</color>");

            foreach (var t in worldTexts)
            {
                Debug.Log(" - Activate world text: " + t);
                if (t != null) t.gameObject.SetActive(true);
            }

            textShown = true;
        }

        // ===========================
        // ⭐ ส่วนคลิกเปิด/ปิด object
        // ===========================
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                bool willOpen = (targetObject != null && !targetObject.activeSelf);

                if (willOpen && clickSound != null)
                    clickSound.Play();

                ToggleTarget();
            }
            else
            {
                if (targetObject != null && targetObject.activeSelf)
                {
                    if (!IsPointerOverTarget())
                        targetObject.SetActive(false);
                }
            }
        }
    }

    void ToggleTarget()
    {
        if (targetObject == null) return;

        bool isActive = targetObject.activeSelf;
        targetObject.SetActive(!isActive);

        if (!isActive)
            StartCoroutine(ProcessAfterOpen());
    }

    IEnumerator ProcessAfterOpen()
    {
        foreach (var obj in destroyWhenOpened)
            if (obj != null) Destroy(obj);

        yield break;
    }

    bool IsPointerOverTarget()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        return hit.collider != null &&
            (hit.collider.gameObject == targetObject ||
             hit.collider.transform.IsChildOf(targetObject.transform));
    }
}
