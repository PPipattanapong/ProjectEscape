using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Header("ชื่อ Scene ที่จะเปลี่ยนไป (ถ้าไม่ใส่ จะใช้ buildIndex)")]
    public string sceneName = "";
    public int sceneIndex = -1;

    // ปุ่มออกเกม
    public void QuitGame()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();

        // ทดสอบใน Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ปุ่มเปลี่ยน Scene
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Loading scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else if (sceneIndex >= 0)
        {
            Debug.Log("Loading scene index: " + sceneIndex);
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("No sceneName or sceneIndex set!");
        }
    }
}
