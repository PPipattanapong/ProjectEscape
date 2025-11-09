using UnityEngine;
using TMPro;
using System.Collections;

public class TextGlitchEffect : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float glitchChance = 0.1f;
    public float glitchDuration = 0.05f;

    private string originalText;

    void Start()
    {
        if (tmpText == null) tmpText = GetComponent<TextMeshProUGUI>();
        originalText = tmpText.text;
        StartCoroutine(GlitchLoop());
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            if (Random.value < glitchChance)
            {
                tmpText.text = Scramble(originalText);
                yield return new WaitForSeconds(glitchDuration);
                tmpText.text = originalText;
            }
            yield return new WaitForSeconds(0.05f);
        }
    }

    string Scramble(string text)
    {
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
            if (Random.value < 0.3f)
                chars[i] = (char)Random.Range(33, 126); // ตัวอักษรสุ่ม
        return new string(chars);
    }
}
