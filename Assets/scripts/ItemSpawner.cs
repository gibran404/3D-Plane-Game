using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs to spawn (pick one at random)")]
    public GameObject[] prefabs;

    [Header("Spawn settings")]
    [Tooltip("Local offset from the plate's transform where the item will be placed (y is useful to prevent clipping)")]
    public Vector3 spawnLocalOffset = Vector3.up * 0.5f;

    // Ensure we only spawn once unless explicitly allowed
    private bool hasSpawned = false;
    // Keep reference to the last spawned instance so it can be destroyed when respawning
    private GameObject lastSpawned = null;

    // Start is called before the first frame update
    void Start()
    {
        randomSpawn(false);
        // No automatic spawn on Start — spawning happens when entering a collider tagged "spawner".
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Picks a random prefab from the `prefabs` array and instantiates it as a child of this plate.
    /// If the array is empty or null, nothing is spawned.
    /// If `force` is true, any previously spawned instance is destroyed and a new one is created.
    /// Returns the spawned GameObject or null if nothing was spawned.
    /// </summary>
    public GameObject randomSpawn(bool force = false)
    {
        if (hasSpawned && !force && lastSpawned != null)
            return lastSpawned;

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: no prefabs assigned to spawn.");
            return null;
        }

        int idx = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[idx];

        if (prefab == null)
        {
            Debug.LogWarning($"ItemSpawner: selected prefab at index {idx} is null.");
            return null;
        }

        // Calculate spawn position and rotation. Spawn as child so it follows the plate if it moves.
        Vector3 worldPos = transform.TransformPoint(spawnLocalOffset);
        Quaternion worldRot = transform.rotation;

        // If forcing a respawn, destroy previous instance first
        if (force && lastSpawned != null)
        {
            Destroy(lastSpawned);
            lastSpawned = null;
            hasSpawned = false;
        }

        GameObject instance = Instantiate(prefab, worldPos, worldRot, this.transform);

        // Reset local position/rotation so the item sits correctly relative to the plate
        instance.transform.localPosition = spawnLocalOffset;
        instance.transform.localRotation = Quaternion.identity;

        lastSpawned = instance;
        hasSpawned = true;

        return instance;
    }

    // When this GameObject enters a trigger collider tagged "spawner", spawn (or respawn) the item.
    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (!other.CompareTag("spawner"))
            return;

        randomSpawn(true);
    }

    // Also handle non-trigger colliders (in case the spawner uses a regular collider)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null || collision.collider == null)
            return;

        if (!collision.collider.CompareTag("spawner"))
            return;

        randomSpawn(true);
    }
}
