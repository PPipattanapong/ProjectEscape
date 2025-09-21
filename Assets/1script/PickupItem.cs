using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea]
    public string clueText;

    private bool collected = false;

    void Update()
    {
        if (collected) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        collected = true;

        InventoryManager.Instance.AddItem(itemName, itemIcon, clueText);

        // ถ้าเป็นไฟฉาย → เปิดระบบไฟฉาย
        if (itemName == "UVFlashlight")
        {
            FlashlightController.Instance.EnableFlashlight();
        }

        gameObject.SetActive(false);

        Debug.Log(itemName + " collected!");
    }

}
