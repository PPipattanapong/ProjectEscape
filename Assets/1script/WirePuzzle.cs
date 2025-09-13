using UnityEngine;

public class WirePuzzle : MonoBehaviour
{
    [Header("Puzzle Parts")]
    public GameObject pathObject;    // Path gameobject (with collider)
    public GameObject endObject;     // End point (with collider)

    public GameObject noteRight;     // Reward
    public LightController doorLight;

    private bool isDragging = false;
    private bool solved = false;

    private Collider2D pathCollider;
    private Collider2D endCollider;
    public Sprite noteRightIcon;

    void Start()
    {
        // Hide puzzle parts at start
        pathObject.SetActive(false);
        endObject.SetActive(false);
        noteRight.SetActive(false);

        pathCollider = pathObject.GetComponent<Collider2D>();
        endCollider = endObject.GetComponent<Collider2D>();
    }

    void OnMouseDown()
    {
        if (!solved)
        {
            pathObject.SetActive(true);
            endObject.SetActive(true);

            // ถ้าอยากให้แน่ใจว่ามี sprite โผล่
            var pathSprite = pathObject.GetComponent<SpriteRenderer>();
            if (pathSprite != null) pathSprite.enabled = true;

            var endSprite = endObject.GetComponent<SpriteRenderer>();
            if (endSprite != null) endSprite.enabled = true;

            Debug.Log("Fusebox puzzle started!");
        }
    }


    void Update()
    {
        if (solved) return;

        if (Input.GetMouseButtonDown(0))
            isDragging = true;
        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Check fail (outside path)
            if (!pathCollider.OverlapPoint(mousePos))
            {
                Debug.Log("Out of path → reset");
                ResetPuzzle(); // hide path/end, allow replay
            }

            // Check success (reach end)
            if (endCollider.OverlapPoint(mousePos))
            {
                Debug.Log("Wire puzzle solved!");
                PuzzleSolved();
            }
        }
    }

    void ResetPuzzle()
    {
        isDragging = false;
        pathObject.SetActive(false);
        endObject.SetActive(false);
    }

    void PuzzleSolved()
    {
        isDragging = false;
        solved = true;

        noteRight.SetActive(true); // 👈 โชว์ note ในฉาก
        doorLight.SetGreen();

        pathObject.SetActive(false);
        endObject.SetActive(false);

        Debug.Log("Wire puzzle solved!");
    }

}
