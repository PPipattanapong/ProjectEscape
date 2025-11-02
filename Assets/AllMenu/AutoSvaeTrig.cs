using UnityEngine;

public class AutoSaveTrigger : MonoBehaviour
{
    void Start()
    {
        if (AutoSaveManager.Instance != null)
        {
            AutoSaveManager.Instance.SaveGame();
        }
    }
}
