using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    public Transform leftPos;
    public Transform middlePos;
    public Transform rightPos;
    public float speed = 5f;

    private Transform target;

    void Start()
    {
        target = middlePos;
    }

    void Update()
    {
        if (target != null)
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }

    public void GoLeft()
    {
        if (target == middlePos) target = leftPos;
        else if (target == rightPos) target = middlePos;
    }

    public void GoRight()
    {
        if (target == middlePos) target = rightPos;
        else if (target == leftPos) target = middlePos;
    }
}