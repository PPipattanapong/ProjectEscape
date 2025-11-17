using UnityEngine;
using TMPro;
using System.Collections;

public class AutoSaveTrigger : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI saveText;   // ลาก TextMeshPro มาใส่ใน Inspector
    public float showDuration = 1.5f;  // เวลาที่โชว์ข้อความ

    void Start()
    {
        if (AutoSaveManager.Instance != null)
        {
            AutoSaveManager.Instance.SaveGame();
            ShowSavedText();
        }
    }

    void ShowSavedText()
    {
        if (saveText != null)
            StartCoroutine(ShowTextRoutine());
    }

    IEnumerator ShowTextRoutine()
    {
        saveText.text = "Saved";
        saveText.gameObject.SetActive(true);

        yield return new WaitForSeconds(showDuration);

        saveText.gameObject.SetActive(false);
    }
}
