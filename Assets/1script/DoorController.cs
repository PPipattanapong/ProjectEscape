using UnityEngine;
using System.Collections;

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

    private bool keyInserted = false;
    private Collider2D doorCollider;

    void Start()
    {
        doorCollider = GetComponent<Collider2D>();

        // ปิด target ตั้งแต่แรก
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

        // ✅ ถ้าไฟซ้าย-ขวาเขียว และใส่กุญแจแล้ว → ประตูเปิด
        if (leftLight.isGreen && rightLight.isGreen && keyInserted)
        {
            Debug.Log("✅ Door opened!");

            // ปิด collider เพื่อให้เดินผ่านได้
            if (doorCollider != null)
                doorCollider.enabled = false;

            // โชว์วัตถุปลายทาง
            if (unlockTarget != null)
                unlockTarget.SetActive(true);

            // ✅ ทำ fade และลบ object ที่ตั้งไว้
            if (destroyWhenOpened != null)
                StartCoroutine(FadeAndDestroy(destroyWhenOpened, fadeDuration));
        }
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        UnityEngine.UI.Image img = target.GetComponent<UnityEngine.UI.Image>();

        // ถ้าไม่มี Renderer หรือ Image → ลบทันที
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
