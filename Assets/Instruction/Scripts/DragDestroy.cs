using UnityEngine;

public class DragAndDestroy : MonoBehaviour
{
    [Header("Target Object ที่จะชนแล้วหาย")]
    public string targetTag = "DestroyTarget"; // กำหนด tag ของ object ที่จะให้หายไปด้วย

    private Vector3 offset;
    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        offset = transform.position - mousePos;
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            transform.position = mousePos + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    // ตรวจจับการชน 2D
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            Destroy(collision.gameObject); // ทำลายอีกวัตถุ
            Destroy(gameObject);           // ทำลายตัวเอง
        }
    }
}
