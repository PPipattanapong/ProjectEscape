using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [Header("ปุ่มปกติ — Scene ที่จะเปิดเมื่อกดปุ่มบน UI")]
    public string sceneName = "";
    public int sceneIndex = -1;

    [Header("ปุ่มโกง — Scene ที่จะเปิดเมื่อกด T")]
    public string cheatSceneName = "";
    public int cheatSceneIndex = -1;

    // ปุ่มออกเกม
    public void QuitGame()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ปุ่มเปลี่ยน Scene (onClick)
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[CHEAT] T pressed!");

            if (!string.IsNullOrEmpty(cheatSceneName))
            {
                SceneManager.LoadScene(cheatSceneName);
            }
            else if (cheatSceneIndex >= 0)
            {
                SceneManager.LoadScene(cheatSceneIndex);
            }
            else
            {
                Debug.LogWarning("[CHEAT] Cheat scene not set!");
            }
        }
    }
}
