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
    public int correctWireIndex = 0;
    public string requiredItem = "WireCutter";

    [Header("Flash Effect")]
    public GameObject damageFlashPanel;
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    [Header("Scene Settings")]
    public string failSceneName;

    private bool solved = false;
    private BombTime bombTimer;

    void Start()
    {
        if (damageFlashPanel != null)
            damageFlashPanel.SetActive(false);

        bombTimer = FindObjectOfType<BombTime>();

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
        if (solved) return;

        if (itemName == requiredItem)
            CutWire(index);
    }

    public void OnItemUsed(string itemName) { }

    private void CutWire(int index)
    {
        if (solved) return;

        if (index == correctWireIndex)
        {
            solved = true;
            wireImages[index].gameObject.SetActive(false);

            if (bombTimer != null)
                bombTimer.FreezeTimer(); // ✅ แค่หยุดเวลาเฉย ๆ

            Debug.Log("[WireCutPuzzle] ✅ Correct wire cut!");
        }
        else
        {
            Debug.Log("[WireCutPuzzle] 💥 Wrong wire! Triggering explosion...");

            if (bombTimer != null)
            {
                bombTimer.FreezeTimer(); // ✅ หยุดเวลา
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
