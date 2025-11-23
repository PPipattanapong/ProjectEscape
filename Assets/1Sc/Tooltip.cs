using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour
{
    public string message;

    private void OnMouseEnter()
    {
        if (IsTopObject())
        {
            Tooltipmanager._instance.SetAndShowToolTip(message);
        }
    }

    private void OnMouseExit()
    {
        Tooltipmanager._instance.HideToolTip();
    }

    // 🔥 สำคัญมาก — เมื่อ object ถูกปิดหรือหาย ให้ปิด tooltip ทันที
    private void OnDisable()
    {
        if (Tooltipmanager._instance != null)
            Tooltipmanager._instance.HideToolTip();
    }

    private void OnDestroy()
    {
        if (Tooltipmanager._instance != null)
            Tooltipmanager._instance.HideToolTip();
    }

    bool IsTopObject()
    {
        // ถ้าเมาส์อยู่บน UI → ถือว่าไม่โดนอันนี้
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        // ยิง Raycast2D เหมือนกล้องเช็คจริง
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(pos, Vector2.zero);

        if (hits.Length == 0)
            return false;

        // หาอันที่อยู่ข้างหน้าสุด (z ใหญ่สุด)
        float bestZ = float.NegativeInfinity;
        GameObject top = null;

        foreach (var h in hits)
        {
            float z = h.collider.transform.position.z;

            if (z > bestZ)
            {
                bestZ = z;
                top = h.collider.gameObject;
            }
        }

        // ถ้า object นี้คือ "ตัวบนสุดจริง" → return true
        return top == this.gameObject;
    }
}
