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

    private Queue<GameObject> pool = new Queue<GameObject>();

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

            pool.Enqueue(seg);

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

        GameObject first = pool.Peek();

        if (first.transform.position.z < recycleZ)
        {
            pool.Dequeue();

            // Find the last segment in queue
            GameObject last = null;
            foreach (var s in pool) last = s;

            float newZ;
            if (last == null)
            {
                // Rare case: only one segment
                newZ = first.transform.position.z + segmentLength * pool.Count;
            }
            else
            {
                newZ = last.transform.position.z + segmentLength;
            }

            first.transform.position = new Vector3(0, 0, newZ);

            pool.Enqueue(first);
        }
    }
}
