using UnityEngine;

public class BookPuzzle : MonoBehaviour
{
    public int correctBookIndex = 1;
    public GameObject[] books;
    public GameObject memoryPuzzle;

    private bool solved = false;

    void Start()
    {
        memoryPuzzle.SetActive(false);
        for (int i = 0; i < books.Length; i++)
        {
            int idx = i;
            books[i].AddComponent<BoxCollider2D>();
            books[i].AddComponent<BookClick>().Setup(this, idx);
        }
    }

    public void OnBookClicked(int index)
    {
        if (solved) return;

        if (index == correctBookIndex)
        {
            // เรียกเปิด MemoryPuzzlePanel ผ่านฟังก์ชัน OpenPuzzle
            memoryPuzzle.GetComponent<MemoryPuzzle>().OpenPuzzle();
            Debug.Log("Book puzzle triggered!");
        }
        else
        {
            Debug.Log("Wrong book, try again.");
        }
    }

}

public class BookClick : MonoBehaviour
{
    private BookPuzzle parent;
    private int index;

    public void Setup(BookPuzzle p, int i)
    {
        parent = p; index = i;
    }

    void OnMouseDown()
    {
        parent.OnBookClicked(index);
    }
}