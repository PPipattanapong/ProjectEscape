using UnityEngine;

public class BookClick : MonoBehaviour
{
    private BookPuzzle puzzle;
    public int index;

    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    public void Setup(BookPuzzle p, int i)
    {
        puzzle = p;
        index = i;

        // ตั้งสีเริ่มต้นให้ตรงกับค่า normalColor ใน Inspector
        if (sr != null)
        {
            originalColor = p.normalColor;
            sr.color = p.normalColor;
        }
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
        if (sr != null && puzzle != null)
            sr.color = puzzle.selectedColor; // ✅ ใช้สีจาก Inspector
    }

    public void ResetColor()
    {
        if (sr != null && puzzle != null)
            sr.color = puzzle.normalColor; // ✅ คืนเป็นสีเดิมที่ตั้งไว้
    }
}
