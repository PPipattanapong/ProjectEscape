using UnityEngine;

public class PanelToggleUI : MonoBehaviour
{
    [Header("Target Panel to Show/Hide")]
    public GameObject targetPanel;

    // Show the panel (for Load Game button)
    public void ShowPanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    // Hide the panel (for Back or Close button)
    public void HidePanel()
    {
        if (targetPanel != null)
            targetPanel.SetActive(false);
    }
}
