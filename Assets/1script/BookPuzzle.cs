using UnityEngine;
using System.Collections;

public class BookPuzzle : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public GameObject[] books;        // หนังสือทั้งหมด
    public Transform[] slots;         // ตำแหน่งบนชั้นวาง
    public string[] correctOrder;     // ใช้ "ชื่อหนังสือ" แทน index

    [Header("Secret Door")]
    public GameObject secretDoor;     // ประตูลับที่จะเลื่อน
    public float moveDuration = 2f;   // เวลาในการเลื่อน
    public float moveOffsetX = 2f;    // ✅ ขยับเพิ่มจากตำแหน่งเดิม

    [Header("Extra Riddle Panel")]
    public GameObject riddlePanel;    // Panel ปริศนา
    public GameObject specialBook;    // หนังสือเล่มที่จะคลิกขวา

    [HideInInspector] public bool solved = false;

    void Start()
    {
        // วางหนังสือตาม slot เริ่มต้น
        for (int i = 0; i < books.Length; i++)
        {
            books[i].transform.position = slots[i].position;

            var drag = books[i].AddComponent<BookDrag>();
            drag.Setup(this, i);
        }

        // ปิด panel ตั้งแต่แรก
        if (riddlePanel != null)
            riddlePanel.SetActive(false);
    }

    void Update()
    {
        // ตรวจสอบการคลิกขวา
        if (Input.GetMouseButtonDown(1)) // 1 = right click
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == specialBook)
            {
                Debug.Log("Right-clicked special book: " + specialBook.name);

                if (riddlePanel != null)
                {
                    riddlePanel.SetActive(true);

                    // เรียก MemoryPuzzle ให้โชว์เนื้อเรื่องและ input
                    MemoryPuzzle mp = riddlePanel.GetComponent<MemoryPuzzle>();
                    if (mp != null)
                        mp.OpenPuzzle();
                }
            }
        }
    }
    public void SwapBooks(BookDrag a, BookDrag b)
    {
        if (solved) return;

        int indexA = a.index;
        int indexB = b.index;

        // สลับ reference ใน array
        GameObject temp = books[indexA];
        books[indexA] = books[indexB];
        books[indexB] = temp;

        // อัปเดต index
        a.index = indexB;
        b.index = indexA;

        // Snap ทุกเล่มกลับตำแหน่ง slot ของมัน
        for (int i = 0; i < books.Length; i++)
        {
            books[i].transform.position = slots[i].position;
        }

        // Debug ลำดับปัจจุบัน
        string order = "";
        for (int i = 0; i < books.Length; i++)
        {
            order += books[i].name + (i < books.Length - 1 ? " | " : "");
        }
        Debug.Log("Current order: " + order);

        // ตรวจสอบว่าถูกหรือยัง
        CheckSolved();
    }


    void CheckSolved()
    {
        for (int i = 0; i < books.Length; i++)
        {
            if (books[i].name != correctOrder[i])
                return;
        }

        solved = true; // 👈 แค่ flag นี้พอ
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
