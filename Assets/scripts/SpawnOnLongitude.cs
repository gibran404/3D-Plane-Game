using UnityEngine;

[ExecuteInEditMode]
public class SpawnOnLongitude : MonoBehaviour
{
    [Header("Sphere Settings")]
    public Transform sphere; // assign your sphere here
    [Tooltip("Use individual axis radii for ellipsoid support. If all zero, will auto-detect from sphere scale.")]
    public Vector3 ellipsoidRadii = Vector3.zero; // X, Y, Z radii
    [Tooltip("Legacy single radius - only used if ellipsoidRadii is zero")]
    public float sphereRadius = 50f;

    [Header("Spawn Settings")]
    public GameObject prefab; // assign cube prefab here
    public int count = 20;
    public float xVariance = 0f;

    [Header("Center Strip Exclusion")]
    [Tooltip("If enabled, leaves a blank strip at the center (x=0 plane)")]
    public bool excludeCenterStrip = false;
    [Tooltip("Width of the center strip to exclude from spawning")]
    public float centerStripWidth = 5f;

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

        // Auto-detect radii if not set
        Vector3 radii = ellipsoidRadii;
        if (radii == Vector3.zero)
        {
            // Try to use the sphere's scale to determine radii
            radii = sphere.localScale * 0.5f;
            
            // Fallback to legacy sphereRadius if scale is not useful
            if (radii.magnitude <= 0f && sphereRadius > 0f)
            {
                radii = Vector3.one * sphereRadius;
            }
        }

        // Evenly distribute objects around the full 360° circle
        float spacingDegrees = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * spacingDegrees;
            float radians = angle * Mathf.Deg2Rad;

            // Longitudinal placement (vertical ring around sphere) with x variance
            Vector3 basePos = new Vector3(0, Mathf.Sin(radians), Mathf.Cos(radians));
            
            // Apply x variance, but avoid center strip if enabled
            float xOffset;
            if (excludeCenterStrip && xVariance > 0f)
            {
                // Generate random value outside the center strip
                float halfStrip = centerStripWidth * 0.5f;
                float availableRange = xVariance - halfStrip;
                
                if (availableRange > 0f)
                {
                    // Randomly choose left or right side
                    float randomVal = Random.Range(0f, availableRange);
                    xOffset = Random.value > 0.5f ? (halfStrip + randomVal) : -(halfStrip + randomVal);
                }
                else
                {
                    // If strip is wider than variance, just use the edges
                    xOffset = Random.value > 0.5f ? halfStrip : -halfStrip;
                }
            }
            else
            {
                xOffset = Random.Range(-xVariance, xVariance);
            }
            
            Vector3 offset = new Vector3(xOffset, 0, 0);
            Vector3 combined = basePos + offset;
            
            // Normalize and then scale by ellipsoid radii instead of uniform sphere radius
            Vector3 normalized = combined.normalized;
            Vector3 localPos = new Vector3(
                normalized.x * radii.x,
                normalized.y * radii.y,
                normalized.z * radii.z
            );
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
