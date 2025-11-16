using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombLauncher : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject bombPrefab;
    public float bombSpeed = 20f;

    public void LaunchBomb()
    {
        if (bombPrefab == null) return;

        GameObject b = Instantiate(bombPrefab, transform.position, transform.rotation);
        EnsureBombComponents(b);
        SetupAndLaunch(b);
    }

    private void EnsureBombComponents(GameObject b)
    {
        var rb = b.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = b.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void SetupAndLaunch(GameObject bomb)
    {
        var rb = bomb.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(transform.forward * bombSpeed, ForceMode.Impulse);
        }
    }
}
