using UnityEngine;
using TMPro;
using System.Collections;

public class LoadPanelUI : MonoBehaviour
{
    public TextMeshProUGUI saveInfoText;

    void OnEnable()
    {
        // ฟัง event เพื่ออัปเดตเมื่อมีการเซฟหรือลบ
        if (AutoSaveManager.Instance != null)
        {
            AutoSaveManager.Instance.OnSaveDataChanged += RefreshUI;
            RefreshUI();
        }
        else
        {
            // ถ้ายังไม่มี instance (ตอนเปิดเกมใหม่) → รอจนกว่าจะสร้างเสร็จ
            StartCoroutine(WaitForManagerAndRefresh());
        }
    }

    void OnDisable()
    {
        if (AutoSaveManager.Instance != null)
            AutoSaveManager.Instance.OnSaveDataChanged -= RefreshUI;
    }

    private IEnumerator WaitForManagerAndRefresh()
    {
        // รอจนกว่าจะมี instance (AutoSaveManager ถูกสร้าง)
        while (AutoSaveManager.Instance == null)
            yield return null;

        AutoSaveManager.Instance.OnSaveDataChanged += RefreshUI;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (AutoSaveManager.Instance == null)
        {
            saveInfoText.text = "<b>Autosave:</b> <color=#999999>Empty</color>";
            return;
        }

        string sceneName = AutoSaveManager.Instance.GetSavedSceneName();

        if (sceneName == "No Save Data")
            saveInfoText.text = "<b>Autosave:</b> <color=#999999>Empty</color>";
        else
            saveInfoText.text = "<b>Autosave:</b> <color=#90EE90>" + sceneName + "</color>";
    }
}
