using UnityEngine;

public class BookClick : MonoBehaviour
{
    private BookPuzzle puzzle;
    public int index;

    private SpriteRenderer sr;
    private Color originalColor;

    [Header("Sound")]
    public AudioSource audioSource;   // 🔊 มีแค่นี้พอ

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        // บังคับว่าต้องมี AudioSource
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // === DEBUG ===
        if (audioSource == null)
            Debug.LogError("[BookClick] ❌ ไม่มี AudioSource บน object: " + gameObject.name);
        else
            Debug.Log("[BookClick] ✔ พบ AudioSource บน: " + gameObject.name +
                      " | Clip = " + (audioSource.clip ? audioSource.clip.name : "NULL") +
                      " | Volume = " + audioSource.volume +
                      " | SpatialBlend = " + audioSource.spatialBlend);
    }

    public void Setup(BookPuzzle p, int i)
    {
        puzzle = p;
        index = i;

        if (sr != null)
        {
            originalColor = p.normalColor;
            sr.color = p.normalColor;
        }

        // === DEBUG ===
        Debug.Log("[BookClick] Setup completed for " + gameObject.name);
    }

    void OnMouseDown()
    {
        Debug.Log("[BookClick] Clicked on: " + gameObject.name);

        if (puzzle != null && !puzzle.solved)
        {
            if (audioSource == null)
            {
                Debug.LogError("[BookClick] ❌ audioSource = NULL! เสียงเล่นไม่ได้");
            }
            else
            {
                Debug.Log("[BookClick] 🔊 Play() called | Clip = " +
                          (audioSource.clip ? audioSource.clip.name : "NULL"));
                audioSource.Play();
            }

            puzzle.SelectBook(this);
        }
        else
        {
            Debug.Log("[BookClick] Puzzle is NULL or solved already.");
        }
    }

    public void Highlight()
    {
        if (sr != null && puzzle != null)
        {
            sr.color = puzzle.selectedColor;
        }
    }

    public void ResetColor()
    {
        if (sr != null && puzzle != null)
        {
            sr.color = puzzle.normalColor;
        }
    }
}
