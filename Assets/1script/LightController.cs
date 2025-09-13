using UnityEngine;

public class LightController : MonoBehaviour
{
    public SpriteRenderer lightRenderer;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public bool isGreen = false;

    void Start() { SetRed(); }

    public void SetRed()
    {
        lightRenderer.color = redColor;
        isGreen = false;
    }

    public void SetGreen()
    {
        lightRenderer.color = greenColor;
        isGreen = true;
    }
}