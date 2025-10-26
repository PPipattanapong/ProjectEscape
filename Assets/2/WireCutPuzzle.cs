using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WireCutPuzzle : MonoBehaviour, IItemReceiver
{
    [Header("Wire Settings")]
    public List<Image> wireImages;
    [Tooltip("ลำดับสายไฟที่ต้องตัดให้ถูก เช่น [1,3,0] คือ 2→4→1 ถ้าเริ่มนับจาก 0")]
    public List<int> correctSequence = new List<int> { 1, 3, 0 };
    public string requiredItem = "WireCutter";

    [Header("Wire Colors (Fixed 4 Colors)")]
    [Tooltip("ลำดับสี 0=แดง, 1=เขียว, 2=เหลือง, 3=ขาว")]
    public List<Color> wireColors = new List<Color> { Color.red, Color.green, Color.yellow, Color.white };

    [Header("Color Display Objects")]
    public GameObject coloredStar;
    public GameObject coloredPodium;
    public GameObject coloredSq;

    [Header("Flash Effect")]
    public GameObject damageFlashPanel;
    public float flashDuration = 0.3f;
    public float flashMaxAlpha = 0.6f;

    [Header("Scene Settings")]
    public string failSceneName;


    private int currentStep = 0;
    private bool failed = false;
    private bool solved = false;
    private bool[] colorRevealed = new bool[3];
    private BombTime bombTimer;

    void Start()
    {
        ShuffleSequence();

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

    // 🎲 สุ่มลำดับ 3 ค่า จาก 0–3 โดยไม่ซ้ำ
    private void ShuffleSequence()
    {
        List<int> pool = new List<int> { 0, 1, 2, 3 };
        correctSequence.Clear();

        for (int i = 0; i < 3; i++)
        {
            int r = Random.Range(0, pool.Count);
            correctSequence.Add(pool[r]);
            pool.RemoveAt(r);
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif

        Debug.Log("[WireCutPuzzle] Randomized 3-wire sequence: " + string.Join(", ", correctSequence));
    }

    // 🎨 ฟังก์ชันภายในใช้ย้อมสี
    private void ApplyColor(GameObject target, int sequenceIndex)
    {
        if (target == null || sequenceIndex < 0 || sequenceIndex >= correctSequence.Count)
            return;

        int colorIndex = correctSequence[sequenceIndex];
        Color c = (colorIndex >= 0 && colorIndex < wireColors.Count) ? wireColors[colorIndex] : Color.gray;

        // รองรับทั้ง SpriteRenderer และ Image
        var sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = c;
        else
        {
            var img = target.GetComponent<Image>();
            if (img != null)
                img.color = c;
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    // 🟡 ย้อมสีทีละช่อง
    public void ApplyStarColor()
    {
        if (!colorRevealed[0])
        {
            ApplyColor(coloredStar, 0);
            colorRevealed[0] = true;
            Debug.Log("[WireCutPuzzle] ⭐ Star color revealed!");
        }
    }

    public void ApplyPodiumColor()
    {
        if (!colorRevealed[1])
        {
            ApplyColor(coloredPodium, 1);
            colorRevealed[1] = true;
            Debug.Log("[WireCutPuzzle] 🏆 Podium color revealed!");
        }
    }

    public void ApplySqColor()
    {
        if (!colorRevealed[2])
        {
            ApplyColor(coloredSq, 2);
            colorRevealed[2] = true;
            Debug.Log("[WireCutPuzzle] 🔳 Square color revealed!");
        }
    }

    // ---------------------------------------------------------------

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

        if (index == correctSequence[currentStep])
        {
            wireImages[index].gameObject.SetActive(false);
            currentStep++;

            if (currentStep >= correctSequence.Count)
            {
                solved = true;
                Debug.Log("[WireCutPuzzle] ✅ All correct wires cut in order!");

                if (bombTimer != null)
                    bombTimer.FreezeTimer();

                var light = FindObjectOfType<SafeProgressLight>();
                if (light != null)
                    light.MarkPuzzleComplete();


                // ตอนสำเร็จ คุณจะเลือกเองว่าจะโชว์สีไหน
                // เช่น เรียก ApplyStarColor(); หรือทั้งหมดก็ได้
                return;
            }
        }
        else
        {
            failed = true;
            Debug.Log("[WireCutPuzzle] 💥 Wrong wire or order!");

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
