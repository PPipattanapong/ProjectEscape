using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class SaveData
{
    public string sceneName;
}

public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager Instance;

    [Header("Auto Save Settings")]
    public bool autoSaveOnStart = false;

    private string saveKey = "AutoSave";

    // 👉 Event สำหรับให้ UI ฟังเวลาเซฟเปลี่ยน
    public event Action OnSaveDataChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (autoSaveOnStart)
        {
            SaveGame();
        }
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[AutoSave] Game saved at scene: {data.sceneName}");
        OnSaveDataChanged?.Invoke(); // แจ้ง UI ให้รีเฟรช
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            SceneManager.LoadScene(data.sceneName);
            Debug.Log($"[AutoSave] Loaded scene: {data.sceneName}");
        }
        else
        {
            Debug.Log("[AutoSave] No saved data found.");
        }
    }

    public string GetSavedSceneName()
    {
        if (!PlayerPrefs.HasKey(saveKey)) return "No Save Data";
        string json = PlayerPrefs.GetString(saveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data.sceneName;
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        Debug.Log("[AutoSave] Save deleted.");
        OnSaveDataChanged?.Invoke(); // แจ้ง UI ให้รีเฟรชหลังลบ
    }
}
