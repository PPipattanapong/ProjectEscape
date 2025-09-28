using UnityEngine;
using System.Collections;

public class BookPuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public GameObject[] books;
    public Transform[] slots;
    public string[] correctOrder;

    [Header("Secret Door")]
    public GameObject secretDoor;
    public float moveDuration = 2f;
    public float moveOffsetX = 2f;

    [Header("Extra Riddle Panel")]
    public GameObject riddlePanel;
    public GameObject specialBook;

    [HideInInspector] public bool solved = false;
    private CanvasGroup riddleCanvasGroup;

    // 📌 ใช้สำหรับเลือกเล่มแรก
    [HideInInspector] public BookClick selectedBook;

    void Start()
    {
        // วางหนังสือตาม slot เริ่มต้น
        for (int i = 0; i < books.Length; i++)
        {
            books[i].transform.position = slots[i].position;

            var click = books[i].AddComponent<BookClick>();
            click.Setup(this, i);
        }

        // เตรียม panel
        if (riddlePanel != null)
        {
            riddleCanvasGroup = riddlePanel.GetComponent<CanvasGroup>();
            if (riddleCanvasGroup == null)
                riddleCanvasGroup = riddlePanel.AddComponent<CanvasGroup>();

            riddlePanel.SetActive(false);
            riddleCanvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        // 👉 ถ้ามีเล่มถูกเลือกไว้ แล้วคลิกซ้ายที่ "ที่ว่าง" → ยกเลิก selection
        if (Input.GetMouseButtonDown(0) && selectedBook != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider == null || hit.collider.GetComponent<BookClick>() == null)
            {
                // คลิกนอกหนังสือ → reset highlight
                selectedBook.ResetColor();
                selectedBook = null;
            }
        }

        if (Input.GetMouseButtonDown(1)) // right click specialBook
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == specialBook)
            {
                Debug.Log("Right-clicked special book: " + specialBook.name);

                if (riddlePanel != null)
                {
                    StartCoroutine(FadeInPanel());

                    MemoryPuzzle mp = riddlePanel.GetComponent<MemoryPuzzle>();
                    if (mp != null)
                        mp.OpenPuzzle();
                }
            }
        }
    }

    IEnumerator FadeInPanel()
    {
        riddlePanel.SetActive(true);

        float t = 0f;
        float duration = 1f;
        riddleCanvasGroup.alpha = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            riddleCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        riddleCanvasGroup.alpha = 1f;
    }

    public void SelectBook(BookClick book)
    {
        if (solved) return;

        if (selectedBook == null)
        {
            // ยังไม่ได้เลือก → เก็บเป็นเล่มแรก
            selectedBook = book;
            selectedBook.Highlight();
            Debug.Log("Selected: " + book.name);
        }
        else
        {
            // มีเล่มแรกแล้ว → swap กับเล่มใหม่
            if (book != selectedBook)
            {
                SwapBooks(selectedBook, book);
            }
            else
            {
                // กดเล่มเดิมซ้ำ → ยกเลิก
                selectedBook.ResetColor();
            }

            // reset selection
            selectedBook = null;
        }
    }

    public void SwapBooks(BookClick a, BookClick b)
    {
        if (solved) return;

        int indexA = a.index;
        int indexB = b.index;

        // swap array
        GameObject temp = books[indexA];
        books[indexA] = books[indexB];
        books[indexB] = temp;

        // update index
        a.index = indexB;
        b.index = indexA;

        // reset highlight ทั้งคู่
        a.ResetColor();
        b.ResetColor();

        // move all into place (smooth)
        for (int i = 0; i < books.Length; i++)
        {
            StartCoroutine(MoveBookToSlot(books[i], slots[i]));
        }

        // debug order
        string order = "";
        for (int i = 0; i < books.Length; i++)
            order += books[i].name + (i < books.Length - 1 ? " | " : "");
        Debug.Log("Current order: " + order);

        CheckSolved();
    }

    IEnumerator MoveBookToSlot(GameObject book, Transform slot)
    {
        Vector3 start = book.transform.position;
        Vector3 end = slot.position;
        float t = 0f;
        float duration = 0.2f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            book.transform.position = Vector3.Lerp(start, end, p);
            yield return null;
        }

        book.transform.position = end;
    }

    void CheckSolved()
    {
        for (int i = 0; i < books.Length; i++)
        {
            if (books[i].name != correctOrder[i])
                return;
        }

        solved = true;
        Debug.Log("Book puzzle solved!");

        if (riddlePanel != null)
            riddlePanel.SetActive(false);

        if (secretDoor != null)
            StartCoroutine(MoveSecretDoor());
    }

    IEnumerator MoveSecretDoor()
    {
        Vector3 startPos = secretDoor.transform.position;
        Vector3 endPos = new Vector3(startPos.x + moveOffsetX, startPos.y, startPos.z);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float progress = t / moveDuration;
            secretDoor.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }

        secretDoor.transform.position = endPos;
    }
}
