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

    // Start is called before the first frame update
    void Start()
    {
        randomSpawn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Picks a random prefab from the `prefabs` array and instantiates it as a child of this plate.
    /// If the array is empty or null, nothing is spawned.
    /// </summary>
    public void randomSpawn()
    {
        if (hasSpawned)
            return;

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: no prefabs assigned to spawn.");
            return;
        }

        int idx = Random.Range(0, prefabs.Length);
        GameObject prefab = prefabs[idx];

        if (prefab == null)
        {
            Debug.LogWarning($"ItemSpawner: selected prefab at index {idx} is null.");
            return;
        }

        // Calculate spawn position and rotation. Spawn as child so it follows the plate if it moves.
        Vector3 worldPos = transform.TransformPoint(spawnLocalOffset);
        Quaternion worldRot = transform.rotation;

        GameObject instance = Instantiate(prefab, worldPos, worldRot, this.transform);

        // Optional: reset local position/rotation if prefab has its own pivoting we want preserved relative to plate
        instance.transform.localPosition = spawnLocalOffset;
        instance.transform.localRotation = Quaternion.identity;

        hasSpawned = true;
    }
}
