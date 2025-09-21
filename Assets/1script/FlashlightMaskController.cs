using UnityEngine;

public class FlashlightMaskController : MonoBehaviour
{
    public GameObject flashlightCircle; // ใส่ FlashlightMask SpriteMask เข้าไปใน Inspector

    private bool isHolding = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            isHolding = true;
        if (Input.GetMouseButtonUp(0))
            isHolding = false;

        flashlightCircle.SetActive(isHolding);

        if (isHolding)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            flashlightCircle.transform.position = mouseWorld;
        }
    }
}
