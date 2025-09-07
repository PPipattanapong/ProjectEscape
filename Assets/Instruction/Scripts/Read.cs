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

    // เรียกจาก DialogueManager เวลาโดนคลิก
    public void OpenPanelFromOutside()
    {
        panel.SetActive(true);
        isActive = true;

        // Position panel near mouse when opening
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
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    bool IsPointerOverItem()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        if (hits.Length > 0)
        {
            GameObject topObject = null;
            int topSortingOrder = int.MinValue;

            foreach (var hit in hits)
            {
                SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sr.sortingOrder > topSortingOrder)
                    {
                        topSortingOrder = sr.sortingOrder;
                        topObject = hit.collider.gameObject;
                    }
                }
            }

            return topObject == this.gameObject;
        }

        return false;
    }

}
