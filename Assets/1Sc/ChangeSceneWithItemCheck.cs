using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneWithItemCheck : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneIfHasItem;     // ถ้ามี UnknownPhoto → ไปซีนนี้
    public string sceneIfMissingItem; // ถ้าไม่มี → ไปซีนนี้

    [Header("Item Requirement")]
    public string requiredItem = "UnknownPhoto";

    void OnMouseDown()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null || hit.collider.gameObject != gameObject)
        {
            Debug.Log("Click blocked by another object.");
            return;
        }

        // ---- เช็คไอเทมใน InventoryManager (สมมติว่ามีระบบนี้อยู่แล้ว) ----
        bool hasItem = InventoryManager.Instance != null &&
                       InventoryManager.Instance.HasItem(requiredItem);

        if (hasItem)
        {
            Debug.Log("✔ มีไอเทม UnknownPhoto → ไปซีน: " + sceneIfHasItem);
            SceneManager.LoadScene(sceneIfHasItem);
        }
        else
        {
            Debug.Log("✖ ไม่มีไอเทม → ไปซีน: " + sceneIfMissingItem);
            SceneManager.LoadScene(sceneIfMissingItem);
        }
    }
}
