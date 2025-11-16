using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class MissleLauncher : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject misslePrefab = null;
    [Tooltip("Seconds between missiles while player is inside trigger")] [SerializeField] private float spawnInterval = 1f;
    [Tooltip("Vertical offset above the player where missiles are spawned")] [SerializeField] private float spawnHeightOffset = 6f;
    [Tooltip("Maximum number of active missiles spawned by this launcher (0 = unlimited)")]
    [SerializeField] private int maxMissles = 5;

    private bool playerInRange = false;
    private Transform playerTransform = null;
    private Coroutine spawnRoutine = null;
    private System.Collections.Generic.List<GameObject> activeMissles = new System.Collections.Generic.List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerTransform = other.transform;
        playerInRange = true;
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.transform == playerTransform)
        {
            playerInRange = false;
            playerTransform = null;
            activeMissles.Clear();
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (playerInRange && playerTransform != null)
        {
            // if max missles count is reached, stop the spawn loop
            if (maxMissles > 0 && activeMissles.Count >= maxMissles)
            {
                break;
            }

            // prune dead entries
            activeMissles.RemoveAll(x => x == null);

            if (maxMissles > 0 && activeMissles.Count >= maxMissles)
            {
                // reached limit, wait and try again
                yield return new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));
                continue;
            }

            SpawnMissleAbovePlayer();


            yield return new WaitForSeconds(Mathf.Max(0.01f, spawnInterval));
        }
        spawnRoutine = null;
    }

    private void SpawnMissleAbovePlayer()
    {
        if (misslePrefab == null) return;

        // Spawn above the launcher itself (not above the player)
        Vector3 spawnPos = transform.position + Vector3.up * spawnHeightOffset;
        GameObject go = Instantiate(misslePrefab, spawnPos, Quaternion.identity);

        // Track active missiles so we can enforce a cap
        activeMissles.Add(go);

        Missle m = go.GetComponent<Missle>();
        if (m != null)
        {
            m.Initialize(playerTransform);
        }
    }
}
