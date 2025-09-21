using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    public static FlashlightController Instance;

    [Header("Flashlight Settings")]
    public GameObject flashlightCircle;
    public Image flashlightIconUI;     // <- ไอคอนใน UI

    [Header("Colors")]
    public Color idleColor = Color.white;
    public Color activeColor = Color.cyan;

    private bool hasFlashlight = false;
    private bool isHolding = false;

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
        }
    }

    public void EnableFlashlight()
    {
        hasFlashlight = true;
    }
}
