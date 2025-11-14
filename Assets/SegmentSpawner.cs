using UnityEngine;
using System.Collections.Generic;

public class SegmentSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject segmentPrefab;
    public float segmentLength = 100f;
    public int startCount = 3;

    [Header("Recycle Trigger")]
    public float recycleZ = -150f;   

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Start()
    {
        float spawnZ = 0f;

        // Spawn initial connected segments
        for (int i = 0; i < startCount; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, new Vector3(0, 0, spawnZ), Quaternion.identity);
            pool.Enqueue(seg);

            spawnZ += segmentLength;   // next segment goes right after
        }
    }

    void Update()
    {
        GameObject first = pool.Peek();

        if (first.transform.position.z < recycleZ)
        {
            pool.Dequeue();

            // Get the last segment in the queue
            GameObject last = null;
            foreach (var seg in pool)
                last = seg;

            // Position this segment directly after the last one
            float newZ = last.transform.position.z + segmentLength;
            first.transform.position = new Vector3(0, 0, newZ);

            // Add back to pool
            pool.Enqueue(first);
        }
    }
}
