using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneOnClick : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName; // ตั้งชื่อ scene ที่จะโหลดใน Inspector

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // ยิง Raycast2D เช็ค object ที่กดโดนจริง
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("Loading scene: " + sceneName);
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.Log("Click blocked by another object.");
            }
        }
    }
}
