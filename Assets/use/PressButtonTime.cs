using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemButtonSequence : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public Button[] buttons;
    public float timeLimit = 1f;

    private List<int> sequence;
    private int currentIndex = 0;
    private bool isPanelActive = false;
    private Coroutine timerCoroutine;

    void Start()
    {
        panel.SetActive(false);
        foreach (Button btn in buttons)
        {
            btn.gameObject.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (!isPanelActive)
        {
            OpenPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    void OpenPanel()
    {
        panel.SetActive(true);
        isPanelActive = true;
        currentIndex = 0;

        // สุ่มลำดับปุ่ม
        sequence = new List<int>();
        for (int i = 0; i < buttons.Length; i++)
            sequence.Add(i);
        Shuffle(sequence);

        ActivateNextButton();
    }

    void ClosePanel()
    {
        panel.SetActive(false);
        isPanelActive = false;
        StopTimer();

        // ปิดปุ่มทั้งหมด
        foreach (Button btn in buttons)
        {
            btn.gameObject.SetActive(false);
        }
    }

    void ActivateNextButton()
    {
        if (currentIndex >= sequence.Count)
        {
            gameObject.SetActive(false); // กดครบทุกปุ่ม → ไอเทมหาย
            ClosePanel();
            return;
        }

        int idx = sequence[currentIndex];
        Button btn = buttons[idx];
        btn.gameObject.SetActive(true);

        // กำหนดสีขาวก่อนกด (ถ้ายังไม่ได้กด)
        btn.image.color = Color.white;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnButtonPressed(btn));

        StartTimer();
    }

    void OnButtonPressed(Button btn)
    {
        StopTimer();

        // เปลี่ยนสีปุ่มทันทีเป็นเขียว
        btn.image.color = Color.green;

        currentIndex++;
        ActivateNextButton();
    }

    void StartTimer()
    {
        StopTimer();
        timerCoroutine = StartCoroutine(ButtonTimer());
    }

    void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    IEnumerator ButtonTimer()
    {
        float elapsed = 0f;
        while (elapsed < timeLimit)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ถ้าไม่ทัน → ปิด panel และเริ่มใหม่
        ClosePanel();
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}
