using UnityEngine;

public class SegmentMover : MonoBehaviour
{
    public float speed = 20f;

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }
}
