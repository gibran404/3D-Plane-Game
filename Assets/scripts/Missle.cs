using System.Collections;
using UnityEngine;

public class Missle : MonoBehaviour
{
    [Header("Ascent")]
    [Tooltip("How far above the spawn Y the missile will climb before homing")]
    [SerializeField]
    private float riseHeight = 1f;

    [Tooltip("Vertical speed while rising (units/sec)")]
    [SerializeField]
    private float riseSpeed = 5f;

    [Header("Homing")]
    [Tooltip("Turn speed in degrees/sec when homing toward target")]
    [SerializeField]
    private float turnSpeed = 20f;

    [Tooltip("Forward flight speed while homing (units/sec)")]
    [SerializeField]
    private float flySpeed = 40f;

    [Header("Detonation")]
    [Tooltip(
        "World Z coordinate at which the missile will detonate (use negative value, e.g. -10)"
    )]
    [SerializeField]
    private float detonationZ = -10f;

    private Transform target = null;
    private float startY = 0f;

    private enum Phase
    {
        Rising,
        Turning,
        Homing,
    }

    private Phase phase = Phase.Rising;
    private bool ascending = true;
    private float turningTimer = 0f;

    [Header("Turning Phase")]
    [Tooltip(
        "Turn speed in degrees/sec while performing the initial turn toward the player/world - used during Turning phase"
    )]
    [SerializeField]
    private float turningTurnSpeed = 60f;

    [Tooltip("Forward flight speed while in the Turning phase (units/sec)")]
    [SerializeField]
    private float turningFlySpeed = 10f;

    [Tooltip(
        "If the missile's angle to the desired turn direction falls below this (degrees), switch to Homing"
    )]
    [SerializeField]
    private float turningAngleThreshold = 5f;

    [Tooltip("Maximum time (seconds) to spend in the Turning phase before forcing Homing")]
    [SerializeField]
    private float turningMaxTime = 2f;
    public GameObject destroy_effect;

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        startY = transform.position.y;
        ascending = true;
        phase = Phase.Rising;
        turningTimer = 0f;
    }

    private void Start()
    {
        // If Initialize wasn't called, capture startY so ascent still works
        startY = transform.position.y;
    }

    private void Update()
    {
        // Phase handling: Rising -> Turning -> Homing
        if (phase == Phase.Rising)
        {
            float targetY = startY + riseHeight;
            Vector3 pos = transform.position;
            float newY = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
            transform.position = new Vector3(pos.x, newY, pos.z);

            if (
                Mathf.Approximately(transform.position.y, targetY)
                || transform.position.y >= targetY
            )
            {
                // Move into turning phase
                phase = Phase.Turning;
                turningTimer = 0f;
            }
            return;
        }

        if (phase == Phase.Turning)
        {
            // Determine desired direction for the initial turn.
            // Prefer an actual target if supplied; otherwise face world -Z.
            Vector3 desiredDir;
            if (target != null)
            {
                desiredDir = (target.position - transform.position).normalized;
            }
            else
            {
                desiredDir = Vector3.back; // world negative Z
            }

            // Smoothly rotate missile's up toward desiredDir using turning-phase turn speed.
            Vector3 newUp = Vector3.RotateTowards(
                transform.up,
                desiredDir,
                Mathf.Deg2Rad * turningTurnSpeed * Time.deltaTime,
                0f
            );
            transform.up = newUp;

            // Move forward slowly during the turning phase
            transform.position += transform.up * turningFlySpeed * Time.deltaTime;

            turningTimer += Time.deltaTime;

            float angleToTarget = Vector3.Angle(transform.up, desiredDir);
            if (angleToTarget <= turningAngleThreshold || turningTimer >= turningMaxTime)
            {
                // Enter homing phase
                phase = Phase.Homing;
            }
            return;
        }

        // Homing phase
        Vector3 forwardDir;
        if (target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            // Use homing turn speed (existing `turnSpeed`) to smooth rotate toward the target
            Vector3 newUp = Vector3.RotateTowards(
                transform.up,
                dirToTarget,
                Mathf.Deg2Rad * turnSpeed * Time.deltaTime,
                0f
            );
            transform.up = newUp;
            forwardDir = transform.up;
        }
        else
        {
            // No target: keep current orientation and fly forward
            forwardDir = transform.up;
        }

        transform.position += forwardDir * flySpeed * Time.deltaTime;

        // Detonation check: if the missile crosses or reaches the configured negative Z value
        if (transform.position.z <= detonationZ)
        {
            Destroy(gameObject);
        }
    }

    private void Detonate()
    {
        if (destroy_effect != null)
        {
            Instantiate(destroy_effect, transform.position, transform.rotation);
        }
        // TODO: play VFX, deal damage, etc. For now just destroy
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Detonate();
    }

    private void OnTriggerEnter(Collider other)
    {
        Detonate();
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
