using UnityEngine;

public class BookClick : MonoBehaviour
{
    private BookPuzzle puzzle;
    public int index;

    private SpriteRenderer sr;
    private Color originalColor;

    public void Setup(BookPuzzle p, int i)
    {
        puzzle = p;
        index = i;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    void OnMouseDown()
    {
        if (puzzle != null && !puzzle.solved)
        {
            puzzle.SelectBook(this);
        }
    }

    public void Highlight()
    {
        if (sr != null)
            sr.color = Color.white;
    }

    public void ResetColor()
    {
        if (sr != null)
            sr.color = originalColor;
    }
}
