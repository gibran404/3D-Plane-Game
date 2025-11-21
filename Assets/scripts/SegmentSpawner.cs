using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject segmentPrefab;        // Loopable segment prefab
    public Transform segmentsParent;        // Parent for all spawned segments
    public GameObject initialSegment;       // Special starting segment

    [Header("Settings")]
    public float segmentLength = 100f;      // Terrain length along Z
    public int startCount = 3;              // Number of loopable segments
    public float recycleZ = -50f;           // Z at which segments recycle

    // Use a fixed-size circular list to avoid allocations from Queue enumeration
    private List<GameObject> pool = new List<GameObject>();
    private int headIndex = 0; // index of the first (oldest) segment

    void Start()
    {
        // Make sure initial segment is parented
        if (initialSegment != null)
            initialSegment.transform.SetParent(segmentsParent);

        // Spawn the initial loopable segments immediately after the initial segment
        float spawnZ = initialSegment.transform.position.z + segmentLength;
        for (int i = 0; i < startCount; i++)
        {
            GameObject seg = Instantiate(segmentPrefab,
                new Vector3(0, 0, spawnZ),
                Quaternion.identity,
                segmentsParent);

            pool.Add(seg);

            spawnZ += segmentLength;
        }
    }

    void Update()
    {
        HandleLoopSegments();
    }

    void HandleLoopSegments()
    {
        if (pool.Count == 0) return;

        GameObject first = pool[headIndex];

        if (first.transform.position.z < recycleZ)
        {
            // Find last index in the circular buffer
            int lastIndex = (headIndex + pool.Count - 1) % pool.Count;
            GameObject last = pool[lastIndex];

            float newZ = last.transform.position.z + segmentLength;

            // Reposition the first segment to the end
            first.transform.position = new Vector3(0, 0, newZ);

            // Advance head index (circular)
            headIndex = (headIndex + 1) % pool.Count;
        }
    }
}
