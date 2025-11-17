using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorController : MonoBehaviour, IItemReceiver
{
    public LightController leftLight;
    public LightController rightLight;
    public LightController centerLight;

    [Header("Extra Object To Enable")]
    [Tooltip("วัตถุที่จะโผล่มาหลังจากประตูเปิด")]
    public GameObject unlockTarget;

    [Header("Extra Object To Destroy")]
    [Tooltip("วัตถุที่จะค่อยๆจางหายหลังประตูเปิด")]
    public GameObject destroyWhenOpened;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Tooltip To Delete When Door Opens")]
    public List<GameObject> tooltipObjects = new List<GameObject>();

    [Header("Audio")]
    public AudioSource doorOpenSound;   // 🔊 เสียงตอนประตูเปิด

    private bool keyInserted = false;
    private Collider2D doorCollider;

    void Start()
    {
        doorCollider = GetComponent<Collider2D>();

        if (unlockTarget != null)
            unlockTarget.SetActive(false);
    }

    public void OnItemUsed(string itemName)
    {
        Debug.Log($"Door received item: {itemName}");

        if (itemName == "Key")
        {
            keyInserted = true;
            if (centerLight != null)
                centerLight.SetGreen();

            CheckDoor();
        }
        else
        {
            Debug.Log("Door does not accept this item!");
        }
    }

    void CheckDoor()
    {
        Debug.Log($"CheckDoor → left:{leftLight.isGreen}, right:{rightLight.isGreen}, key:{keyInserted}");

        if (leftLight.isGreen && rightLight.isGreen && keyInserted)
        {
            Debug.Log("✅ Door opened!");

            // --- 🔊 เล่นเสียงตอนเปิดประตู ---
            if (doorOpenSound != null)
                doorOpenSound.Play();

            if (doorCollider != null)
                doorCollider.enabled = false;

            if (unlockTarget != null)
                unlockTarget.SetActive(true);

            // ลบ tooltip
            foreach (var obj in tooltipObjects)
            {
                if (obj == null) continue;

                Tooltip t = obj.GetComponent<Tooltip>();
                if (t != null)
                    Destroy(t);
            }

            if (destroyWhenOpened != null)
                StartCoroutine(FadeAndDestroy(destroyWhenOpened, fadeDuration));
        }
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image img = target.GetComponent<UnityEngine.UI.Image>();

        if (sr == null && img == null)
        {
            Destroy(target);
            yield break;
        }

        float t = 0f;
        Color originalColor = sr ? sr.color : img.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, t / duration);

            if (sr)
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            if (img)
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        Destroy(target);
    }
}
