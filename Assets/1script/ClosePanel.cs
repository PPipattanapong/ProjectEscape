using UnityEngine;

public class ClosePanel : MonoBehaviour
{
    public GameObject panel;

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
