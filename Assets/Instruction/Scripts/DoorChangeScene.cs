using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickChangeScene : MonoBehaviour
{
    [Header("ชื่อ Scene ที่จะโหลด")]
    public string sceneName = "Room2"; // กำหนดใน Inspector

    private void OnMouseDown()
    {
        // เช็คว่ากดคลิกซ้าย
        if (Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
