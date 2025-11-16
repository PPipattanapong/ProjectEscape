using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance;

    [Header("Flashlight Settings")]
    public GameObject flashlightCircle;
    public Image flashlightIconUI;

    [Header("Colors")]
    public Color idleColor = Color.white;
    public Color activeColor = Color.cyan;

    [Header("Reveal Targets")]
    [Tooltip("วัตถุที่เมื่อโดนไฟฉายแล้วจะลบ Tooltip ออก")]
    public List<GameObject> revealObjects = new List<GameObject>();

    private bool hasFlashlight = false;
    private bool isHolding = false;
    private float flashlightRadius = 0.7f;    // รัศมีตรวจ (ตาม sprite mask ทั่วไป)

    void Awake()
    {
        Instance = this;
        flashlightCircle.SetActive(false);

        if (flashlightIconUI != null)
            flashlightIconUI.color = idleColor;
    }

    void Update()
    {
        if (!hasFlashlight) return;

        if (Input.GetMouseButtonDown(0)) isHolding = true;
        if (Input.GetMouseButtonUp(0)) isHolding = false;

        flashlightCircle.SetActive(isHolding);

        if (flashlightIconUI != null)
            flashlightIconUI.color = isHolding ? activeColor : idleColor;

        if (isHolding)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            flashlightCircle.transform.position = mouseWorld;

            CheckReveal(mouseWorld);
        }
    }

    public void EnableFlashlight()
    {
        hasFlashlight = true;
    }

    // ⭐ ถ้าไฟฉายส่องโดนวัตถุ → ลบ Tooltip
    private void CheckReveal(Vector3 center)
    {
        foreach (GameObject obj in revealObjects)
        {
            if (obj == null) continue;

            float dist = Vector2.Distance(center, obj.transform.position);

            if (dist <= flashlightRadius)
            {
                Tooltip tooltip = obj.GetComponent<Tooltip>();
                if (tooltip != null)
                {
                    Destroy(tooltip);  // 🔥 ลบ Tooltip เลยแบบถาวร
                    Debug.Log($"Flashlight revealed → Tooltip removed on: {obj.name}");
                }
            }
        }
    }
}
