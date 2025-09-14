using UnityEngine;

public class BookDrag : MonoBehaviour
{
    private BookPuzzle puzzle;
    public int index;

    private Vector3 offset;
    private bool dragging = false;
    public float swapDistance = 1f; // 👉 ระยะที่อนุญาตให้สลับ

    public void Setup(BookPuzzle p, int i)
    {
        puzzle = p;
        index = i;
    }

    void OnMouseDown()
    {
        if (puzzle == null || puzzle.solved) return;

        dragging = true;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mousePos.x, mousePos.y, 0f);
    }

    void OnMouseDrag()
    {
        if (!dragging) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, 0f) + offset;
    }

    void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        // หาเล่มที่ใกล้ที่สุด
        float minDist = Mathf.Infinity;
        BookDrag closest = null;

        foreach (var book in puzzle.books)
        {
            if (book == gameObject) continue;

            float dist = Vector3.Distance(transform.position, book.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = book.GetComponent<BookDrag>();
            }
        }

        // ถ้าใกล้พอ → สลับ
        if (closest != null && minDist < swapDistance)
        {
            puzzle.SwapBooks(this, closest);
        }
        else
        {
            // snap กลับตำแหน่ง slot ปัจจุบัน
            transform.position = puzzle.slots[index].position;
        }
    }
}
