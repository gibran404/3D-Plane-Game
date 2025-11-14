using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float verticalClampMin = 1f;
    public float verticalClampMax = 7f;
    public float horizontalClamp = 4f;

    [Header("Tilt Settings")]
    public float maxTiltAngle = 25f;     // how much the helicopter rolls on drag
    public float tiltSmooth = 6f;

    [Header("Auto Stabilize")]
    public float stabilizeSpeed = 4f;
    public float hoverSwayAmount = 0.2f;
    public float hoverSwaySpeed = 2f;

    private Vector2 lastInputPos;
    private bool dragging = false;
    private Vector3 targetPos;
    private float currentTilt = 0f;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        HandleInput();
        MoveHelicopter();
        ApplyTilt();
    }

    void HandleInput()
    {
        bool isTouch = Input.touchCount > 0;
        bool isMouseDown = Input.GetMouseButton(0);
        Vector2 currentPos;
        Vector2 delta = Vector2.zero;

        if (isTouch)
        {
            Touch t = Input.GetTouch(0);
            currentPos = t.position;
            if (t.phase == TouchPhase.Began)
            {
                dragging = true;
                lastInputPos = currentPos;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                delta = currentPos - lastInputPos;
                lastInputPos = currentPos;
            }
            else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                dragging = false;
            }
        }
        else if (isMouseDown)
        {
            currentPos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                dragging = true;
                lastInputPos = currentPos;
            }
            else
            {
                delta = currentPos - lastInputPos;
                lastInputPos = currentPos;
            }
        }
        else
        {
            dragging = false;
        }

        if (dragging && delta != Vector2.zero)
        {
            // Map drag to movement
            float moveX = (delta.x / Screen.width) * moveSpeed;
            float moveY = (delta.y / Screen.height) * moveSpeed;

            targetPos += new Vector3(moveX, moveY, 0);

            // Clamp height (keep inside gorge)
            targetPos.y = Mathf.Clamp(targetPos.y, verticalClampMin, verticalClampMax);
            targetPos.x = Mathf.Clamp(targetPos.x, -horizontalClamp, horizontalClamp);

            // Set tilt based on horizontal drag
            currentTilt = Mathf.Lerp(currentTilt, -delta.x * (maxTiltAngle / 200f), 0.3f);
        }
    }

    void MoveHelicopter()
    {
        if (!dragging)
        {
            // Smooth auto-stabilized hover with slight sway
            float sway = Mathf.Sin(Time.time * hoverSwaySpeed) * hoverSwayAmount;
            targetPos.y = Mathf.Clamp(
                transform.position.y + sway * Time.deltaTime,
                verticalClampMin,
                verticalClampMax
            );

            // Tilt returns to 0
            currentTilt = Mathf.Lerp(currentTilt, 0, Time.deltaTime * stabilizeSpeed);
        }

        // Smooth movement to target
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 6f);
    }

    void ApplyTilt()
    {
        Quaternion targetRot = Quaternion.Euler(0, 0, currentTilt);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * tiltSmooth);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Terrain") || other.collider.CompareTag("Obstacle"))
        {
            Debug.Log("Hit terrain!");
            GameController.Instance.Crash();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") || other.CompareTag("Terrain"))
        {
            Debug.Log("Hit obstacle!");
            GameController.Instance.Crash();
        }
    }
}
