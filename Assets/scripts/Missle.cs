using System.Collections;
using UnityEngine;

public class Missle : MonoBehaviour
{
    [Header("Ascent")]
    [Tooltip("How far above the spawn Y the missile will climb before homing")]
    [SerializeField] private float riseHeight = 6f;
    [Tooltip("Vertical speed while rising (units/sec)")]
    [SerializeField] private float riseSpeed = 8f;

    [Header("Homing")]
    [Tooltip("Turn speed in degrees/sec when homing toward target")]
    [SerializeField] private float turnSpeed = 90f;
    [Tooltip("Forward flight speed while homing (units/sec)")]
    [SerializeField] private float flySpeed = 20f;

    [Header("Detonation")]
    [Tooltip("World Z coordinate at which the missile will detonate (use negative value, e.g. -10)")]
    [SerializeField] private float detonationZ = -10f;

    private Transform target = null;
    private float startY = 0f;
    private bool ascending = true;

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        startY = transform.position.y;
        ascending = true;
    }

    private void Start()
    {
        // If Initialize wasn't called, capture startY so ascent still works
        startY = transform.position.y;
    }

    private void Update()
    {
        if (ascending)
        {
            float targetY = startY + riseHeight;
            Vector3 pos = transform.position;
            float newY = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
            transform.position = new Vector3(pos.x, newY, pos.z);

            if (Mathf.Approximately(transform.position.y, targetY) || transform.position.y >= targetY)
            {
                ascending = false;
            }
            return;
        }

        // Homing phase
        Vector3 forwardDir;
        if (target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            // Smoothly rotate the missile's "up" vector toward the target direction.
            // Missile prefab should be modeled with its long axis aligned to local up (Y).
            Vector3 newUp = Vector3.RotateTowards(transform.up, dirToTarget, Mathf.Deg2Rad * turnSpeed * Time.deltaTime, 0f);
            transform.up = newUp;
            forwardDir = transform.up;
        }
        else
        {
            // No target: keep current orientation
            forwardDir = transform.up;
        }

        transform.position += forwardDir * flySpeed * Time.deltaTime;

        // Detonation check: if the missile crosses or reaches the configured negative Z value
        if (transform.position.z <= detonationZ)
        {
            Detonate();
        }
    }

    private void Detonate()
    {
        // TODO: play VFX, deal damage, etc. For now just destroy
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 a = new Vector3(0f, 0f, detonationZ);
        Vector3 b = new Vector3(0f, 0f, -detonationZ);
        Gizmos.DrawLine(a + Vector3.left * 50f, a + Vector3.right * 50f);
        Gizmos.DrawLine(b + Vector3.left * 50f, b + Vector3.right * 50f);
    }
}
