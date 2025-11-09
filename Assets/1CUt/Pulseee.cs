using UnityEngine;
using TMPro;

public class TextPulse : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float pulseSpeed = 2f;
    public Color baseColor = Color.red;
    public Color pulseColor = new Color(1, 0.4f, 0.4f);

    void Update()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        tmpText.color = Color.Lerp(baseColor, pulseColor, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2);
    }
}
