using UnityEngine;

[ExecuteInEditMode]
public class SpawnOnLongitude : MonoBehaviour
{
    [Header("Sphere Settings")]
    public Transform sphere; // assign your sphere here
    public float sphereRadius = 50f;

    [Header("Spawn Settings")]
    public GameObject prefab; // assign cube prefab here
    public int count = 20;
    public float xVariance = 0f;

    [Header("Editor Controls")]
    public bool generate = false;
    public bool clear = false;

    [Header("Parenting")]
    [Tooltip("Optional parent/collector transform to place spawned items under. If null, uses this GameObject's transform.")]
    public Transform spawnCollector;

    private void Update()
    {
        if (Application.isPlaying) return;

        if (generate)
        {
            generate = false;
            GenerateObjects();
        }

        if (clear)
        {
            clear = false;
            ClearObjects();
        }
    }

    private void ClearObjects()
    {
    Transform parentTarget = spawnCollector != null ? spawnCollector : transform;
    for (int i = parentTarget.childCount - 1; i >= 0; i--)
    {
#if UNITY_EDITOR
        DestroyImmediate(parentTarget.GetChild(i).gameObject);
#else
        Destroy(parentTarget.GetChild(i).gameObject);
#endif
    }
    }

    private void GenerateObjects()
    {
        if (sphere == null || prefab == null)
        {
            Debug.LogWarning("Assign both Sphere and Prefab first.");
            return;
        }

        ClearObjects();

        // Auto-detect radius if not set
        if (sphereRadius <= 0f)
            sphereRadius = sphere.localScale.x * 0.5f;

        // Evenly distribute objects around the full 360° circle
        float spacingDegrees = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * spacingDegrees;
            float radians = angle * Mathf.Deg2Rad;

            // Longitudinal placement (vertical ring around sphere) with x variance
            Vector3 basePos = new Vector3(0, Mathf.Sin(radians), Mathf.Cos(radians));
            Vector3 offset = new Vector3(Random.Range(-xVariance, xVariance), 0, 0);
            Vector3 combined = basePos + offset;
            Vector3 localPos = combined.normalized * sphereRadius;
            Vector3 worldPos = sphere.position + localPos;

#if UNITY_EDITOR
            GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
#else
            GameObject obj = Instantiate(prefab);
#endif

            obj.transform.position = worldPos;
            obj.transform.up = (obj.transform.position - sphere.position).normalized;
            Transform parentTarget = spawnCollector != null ? spawnCollector : transform;
            obj.transform.SetParent(parentTarget);
            obj.name = $"Cube_{i}";
        }

        Debug.Log($"Spawned {count} evenly spaced objects around sphere longitude loop.");
    }
}
