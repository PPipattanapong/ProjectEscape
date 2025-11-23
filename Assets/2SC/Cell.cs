using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    [HideInInspector] public int x, y;
    [HideInInspector] public bool isEndpoint = false;
    [HideInInspector] public int colorIndex = -1;

    private Image img;
    private ColorConnectPuzzle puzzle;

    private readonly Color baseGray = new Color(0.25f, 0.25f, 0.25f);
    private readonly float endpointTint = 0.55f;

    public void Setup(int px, int py, ColorConnectPuzzle p)
    {
        x = px;
        y = py;
        puzzle = p;
        if (!img) img = GetComponent<Image>();
        img.color = baseGray;
        isEndpoint = false;
        colorIndex = -1;
    }

    public void SetEndpoint(Color color, int idx)
    {
        if (!img) img = GetComponent<Image>();
        isEndpoint = true;
        colorIndex = idx;

        // ผสมสี endpoint กับพื้นเทาให้ดูชัด
        Color c = Color.Lerp(baseGray, color, endpointTint);
        c.a = 1f;
        img.color = c;
    }

    public void SetTempColor(Color color)
    {
        if (isEndpoint) return;
        if (!img) img = GetComponent<Image>();
        color.a = 1f;
        img.color = color;
    }

    public void SetPermanentColor(Color color)
    {
        if (isEndpoint) return;
        if (!img) img = GetComponent<Image>();
        color.a = 1f;
        img.color = color;
    }

    public void ResetTempColor()
    {
        if (isEndpoint) return;
        if (!img) img = GetComponent<Image>();
        img.color = baseGray;
    }

    public void OnPointerDown(PointerEventData e)
    {
        puzzle?.OnCellDown(this);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        puzzle?.OnCellEnter(this);
    }

    public void OnPointerUp(PointerEventData e)
    {
        puzzle?.OnCellUp(this);
    }
}
