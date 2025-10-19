using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class WireCutPuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Wire Settings")]
    public List<Image> wireImages;
    [Tooltip("ลำดับสายไฟที่ต้องตัดให้ถูก เช่น [1,3,0] คือ 2→4→1 ถ้าเริ่มนับจาก 0")]
    public List<int> correctSequence = new List<int> { 1, 3, 0 }; // ✅ เส้นที่ 2, 4, 1
    public string requiredItem = "WireCutter";

    [Header("Flash Effect")]
    public GameObject damageFlashPanel;
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    [Header("Scene Settings")]
    public string failSceneName;

    [Header("Reward Object")]
    [Tooltip("GameObject ที่จะโผล่มาหลังตัดครบถูกทั้งหมด")]
    public GameObject rewardObject;

    private int currentStep = 0;
    private bool failed = false;
    private bool solved = false;
    private BombTime bombTimer;

    void Start()
    {
        if (damageFlashPanel != null)
            damageFlashPanel.SetActive(false);

        if (rewardObject != null)
            rewardObject.SetActive(false);

        bombTimer = FindObjectOfType<BombTime>();

        // เพิ่ม collider + receiver ให้ทุกเส้น
        for (int i = 0; i < wireImages.Count; i++)
        {
            int index = i;
            var img = wireImages[i];
            if (img == null) continue;

            img.raycastTarget = true;
            if (img.GetComponent<BoxCollider2D>() == null)
                img.gameObject.AddComponent<BoxCollider2D>();

            var trigger = img.gameObject.AddComponent<WireReceiver>();
            trigger.Init(this, index);
        }
    }

    public void OnWireItemUsed(string itemName, int index)
    {
        if (solved || failed) return;

        if (itemName == requiredItem)
            CutWire(index);
    }

    public void OnItemUsed(string itemName) { }

    private void CutWire(int index)
    {
        if (solved || failed) return;

        Debug.Log($"[WireCutPuzzle] Cutting wire #{index}");

        // ตรวจว่าถูกเส้นตามลำดับไหม
        if (index == correctSequence[currentStep])
        {
            wireImages[index].gameObject.SetActive(false);
            currentStep++;

            // ✅ ครบทุกเส้นตามลำดับ
            if (currentStep >= correctSequence.Count)
            {
                solved = true;
                Debug.Log("[WireCutPuzzle] ✅ All correct wires cut in order!");

                if (bombTimer != null)
                    bombTimer.FreezeTimer();

                if (rewardObject != null)
                    rewardObject.SetActive(true); // ✅ โผล่มาเลย

                return;
            }
        }
        else
        {
            // ❌ ตัดผิดลำดับ หรือเส้นไม่ถูกต้อง
            failed = true;
            Debug.Log("[WireCutPuzzle] 💥 Wrong wire or order! Triggering explosion...");

            if (bombTimer != null)
            {
                bombTimer.FreezeTimer();
                if (bombTimer.clockText != null)
                {
                    bombTimer.clockText.text = "BOOM!";
                    bombTimer.clockText.color = Color.red;
                }
            }

            StartCoroutine(FlashDamagePanel());
            StartCoroutine(LoadFailScene());
        }
    }

    private IEnumerator FlashDamagePanel()
    {
        if (damageFlashPanel == null) yield break;

        damageFlashPanel.SetActive(true);
        Image img = damageFlashPanel.GetComponent<Image>();
        if (img == null) yield break;

        Color baseColor = img.color;
        float t = 0f;

        while (t < flashDuration * 0.3f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, flashMaxAlpha, t / (flashDuration * 0.3f));
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        t = 0f;
        while (t < flashDuration * 0.7f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(flashMaxAlpha, 0f, t / (flashDuration * 0.7f));
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        damageFlashPanel.SetActive(false);
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
    }

    private IEnumerator LoadFailScene()
    {
        yield return new WaitForSeconds(1.5f);
        if (!string.IsNullOrEmpty(failSceneName))
            SceneManager.LoadScene(failSceneName);
    }
}

public class WireReceiver : MonoBehaviour, IItemReceiver
{
    private WireCutPuzzle puzzle;
    private int wireIndex;

    public void Init(WireCutPuzzle p, int index)
    {
        puzzle = p;
        wireIndex = index;
    }

    public void OnItemUsed(string itemName)
    {
        puzzle.OnWireItemUsed(itemName, wireIndex);
    }
}
