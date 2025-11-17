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

    [Header("Extra Objects To Destroy (Optional)")]
    public List<GameObject> destroyWhenOpened = new List<GameObject>();

    [Header("3D TextMeshPro List (World Space)")]
    public List<TextMeshPro> worldTexts = new List<TextMeshPro>();

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    void Start()
    {
        mainCam = Camera.main;

        if (targetObject != null)
            targetObject.SetActive(false);

        foreach (var obj in destroyWhenOpened)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // ปิด TextMeshPro ทั้งหมดก่อน
        foreach (var t in worldTexts)
        {
            if (t != null)
                t.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
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

        // ถ้าเพิ่งเปิด → เช็ก WireCutter
        if (!isActive)
        {
            CheckWireCutter();

            foreach (var obj in destroyWhenOpened)
            {
                if (obj != null)
                    StartCoroutine(FadeAndDestroy(obj, fadeDuration));
            }
        }
    }

    void CheckWireCutter()
    {
        Transform found = targetObject.transform.Find("WireCutter");
        bool hasWireCutter = found != null;

        // เปิดหรือปิด TMP 3D ทั้งหมด
        foreach (var t in worldTexts)
        {
            if (t != null)
                t.gameObject.SetActive(hasWireCutter);
        }
    }

    bool IsPointerOverTarget()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider == null) return false;

        return hit.collider.gameObject == targetObject ||
               hit.collider.transform.IsChildOf(targetObject.transform);
    }

    IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        if (target == null) yield break;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Destroy(target);
            yield break;
        }

        float t = 0f;
        Color originalColor = sr.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, t / duration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        Destroy(target);
    }
}
