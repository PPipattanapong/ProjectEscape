using UnityEngine;

public class LoadButtonAction : MonoBehaviour
{
    public void LoadGame()
    {
        if (AutoSaveManager.Instance != null)
            AutoSaveManager.Instance.LoadGame();
        else
            Debug.LogWarning("AutoSaveManager not found.");
    }

    public void DeleteSave()
    {
        if (AutoSaveManager.Instance != null)
            AutoSaveManager.Instance.DeleteSave();
        else
            Debug.LogWarning("AutoSaveManager not found.");
    }
}
