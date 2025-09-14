using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; // สำหรับเช็ก UI
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    public float typeSpeed = 0.03f;

    [Header("Clickable Objects")]
    public GameObject[] clickableObjects;

    private bool isTyping = false;
    private bool skipTyping = false;

    // เก็บ index ของไอเทมที่เพิ่งคลิกล่าสุด
    private int lastClickedIndex = -1;

    void Start()
    {
        // เพิ่ม Collider ให้ทุก clickable object ถ้าไม่มี
        foreach (var obj in clickableObjects)
        {
            if (obj != null && obj.GetComponent<Collider2D>() == null)
            {
                obj.AddComponent<BoxCollider2D>();
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckClick();
        }
    }

    void CheckClick()
    {
        // ถ้า cursor อยู่บน UI → return ทันที
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        if (hits.Length > 0)
        {
            GameObject topObject = null;
            int topSortingOrder = int.MinValue;

            // หาตัว sprite ที่อยู่บนสุด
            foreach (var hit in hits)
            {
                SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sortingOrder > topSortingOrder)
                {
                    topSortingOrder = sr.sortingOrder;
                    topObject = hit.collider.gameObject;
                }
            }

            if (topObject != null)
            {
                for (int i = 0; i < clickableObjects.Length; i++)
                {
                    if (topObject == clickableObjects[i])
                    {
                        // ถ้าเป็นไอเทมเดิมที่เพิ่งคลิก → ไม่ทำอะไร
                        if (lastClickedIndex == i)
                            return;

                        lastClickedIndex = i; // อัปเดตไอเทมล่าสุด
                        ShowDialogue(i);

                        FollowMousePanel panel = topObject.GetComponent<FollowMousePanel>();
                        if (panel != null)
                        {
                            panel.OpenPanelFromOutside();
                        }

                        return;
                    }
                }
            }
            else
            {
                // คลิกโดนวัตถุอื่น (ไม่ใช่ clickableObjects)
                dialogueText.text = "";
                lastClickedIndex = -1;
            }
        }
        else
        {
            // คลิกพื้นที่ว่าง
            dialogueText.text = "";
            lastClickedIndex = -1;
        }
    }

    void ShowDialogue(int index)
    {
        if (index < 0 || index >= dialogueLines.Length) return;

        if (isTyping)
        {
            skipTyping = true;
        }
        else
        {
            StartCoroutine(TypeLine(dialogueLines[index]));
        }
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        skipTyping = false;
        dialogueText.text = "";

        foreach (char c in line)
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }
}
