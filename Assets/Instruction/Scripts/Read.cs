using UnityEngine;
using UnityEngine.EventSystems;

public class FollowMousePanel : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel; // The panel to show/hide

    private bool isActive = false;

    void Start()
    {
        // Start with panel inactive
        panel.SetActive(false);
    }

    void Update()
    {
        // Click anywhere outside the item/UI → close panel
        if (Input.GetMouseButtonDown(0))
        {
            if (isActive)
            {
                if (!IsPointerOverUI() && !IsPointerOverItem())
                {
                    ClosePanel();
                }
            }
        }
    }

    void OnMouseDown()
    {
        OpenPanel();
    }

    private void OpenPanel()
    {
        panel.SetActive(true);
        isActive = true;

        // Optionally, position panel near mouse when opening
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panel.transform.parent as RectTransform,
            Input.mousePosition,
            null,
            out mousePos
        );

        panel.GetComponent<RectTransform>().localPosition = mousePos + new Vector2(50, 50); // offset if needed
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
        isActive = false;
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    bool IsPointerOverItem()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);
        return hit != null && hit.gameObject == this.gameObject;
    }
}
