using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickChangeScene : MonoBehaviour
{
    [Header("ชื่อ Scene ที่จะโหลด")]
    public string sceneName = "Room2"; // กำหนดใน Inspector

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // ยิง Raycast ดูว่าชน Collider ไหนก่อน
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("คลิกโดน: " + hit.collider.gameObject.name);

                // ถ้าเจอ collider ของ object นี้ -> เปลี่ยน scene
                if (hit.collider.gameObject == gameObject)
                {
                    SceneManager.LoadScene(sceneName);
                }
            }
            else
            {
                Debug.Log("ไม่ชน Collider อะไรเลย");
            }
        }
    }
}
