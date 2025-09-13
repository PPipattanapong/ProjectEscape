using UnityEngine;
using TMPro;

public class SlotClue : MonoBehaviour
{
    public TextMeshProUGUI clueText;

    public void ShowClue()
    {
        if (clueText == null) return;

        // toggle แทนที่จะเปิดตลอด
        bool isActive = clueText.gameObject.activeSelf;
        clueText.gameObject.SetActive(!isActive);
    }
}
