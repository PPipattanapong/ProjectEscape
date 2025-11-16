using UnityEngine;

public class UIWiggle : MonoBehaviour
{
    public RectTransform target;
    public float moveAmount = 5f;     // ขยับซ้าย–ขวาแค่กี่พิกเซล
    public float speed = 2f;          // ความเร็ว

    Vector2 originalPos;

    void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        originalPos = target.anchoredPosition;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * moveAmount;
        target.anchoredPosition = new Vector2(originalPos.x + offset, originalPos.y);
    }
}
