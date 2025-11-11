using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SpawnClutter : MonoBehaviour
{
    [Header("Sphere Settings")]
    [Tooltip("Assign the sphere (transform) to spawn objects on.")]
    public Transform sphere;

    [Header("Spawn Settings")]
    [Tooltip("Array of prefabs to pick from randomly.")]
    public GameObject[] prefabs;
    [Tooltip("Number of objects to spawn across the surface.")]
    public int count = 100;
    [Tooltip("Random uniform scale multiplier range applied to the instantiated object.")]
    public Vector2 scaleRange = new Vector2(1f, 1f);
    [Tooltip("Slight offset along the surface normal (positive pushes outward).")]
    public float surfaceOffset = 0f;

    [Header("Randomization")]
    [Tooltip("Random additional rotation around the object's local up axis (0-360).")]
    public bool randomYaw = true;

    [Header("Editor Controls")]
    public bool generate = false;
    public bool clear = false;

    [Header("Placement")]
    [Tooltip("Align spawned object's up to the surface normal. Turn off if prefab has its own orientation logic.")]
    public bool alignToNormal = true;

    [Header("Parenting")]
    [Tooltip("Optional parent/collector transform to place spawned items under. If null, uses this GameObject's transform.")]
    public Transform spawnCollector;

    private void Update()
    {
        // don't run during play mode
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
        if (sphere == null)
        {
            Debug.LogWarning("SpawnClutter: Assign a sphere transform first.");
            return;
        }

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("SpawnClutter: Add at least one prefab to the prefabs array.");
            return;
        }

        ClearObjects();

        // Local radii: assume the sphere primitive has a diameter of 1 at scale 1 (radius 0.5)
        Vector3 localRadii = new Vector3(
            Mathf.Abs(sphere.localScale.x) * 0.5f,
            Mathf.Abs(sphere.localScale.y) * 0.5f,
            Mathf.Abs(sphere.localScale.z) * 0.5f
        );

        // Avoid very small/zero radii which would cause division by zero when computing normals
        const float kMinRadius = 1e-4f;
        if (localRadii.x < kMinRadius) localRadii.x = kMinRadius;
        if (localRadii.y < kMinRadius) localRadii.y = kMinRadius;
        if (localRadii.z < kMinRadius) localRadii.z = kMinRadius;

        for (int i = 0; i < count; i++)
        {
            // sample a random direction on the unit sphere (uniform)
            Vector3 unit = Random.onUnitSphere; // this is in local-space directions for our use

            // point on (possibly ellipsoid) surface in local space
            Vector3 localPoint = new Vector3(unit.x * localRadii.x, unit.y * localRadii.y, unit.z * localRadii.z);

            // convert to world position (handles rotation/parenting)
            Vector3 worldPos = sphere.TransformPoint(localPoint);

            // compute local-space normal for an ellipsoid: (x/a^2, y/b^2, z/c^2)
            Vector3 localNormal = new Vector3(
                localPoint.x / (localRadii.x * localRadii.x),
                localPoint.y / (localRadii.y * localRadii.y),
                localPoint.z / (localRadii.z * localRadii.z)
            );

            // convert normal to world space and normalize (fallback)
            Vector3 worldNormal = sphere.TransformDirection(localNormal).normalized;

            // If the sphere has a collider, snap the worldPos to the collider surface for accuracy
            Collider sphereCollider = sphere.GetComponent<Collider>();
            if (sphereCollider != null)
            {
                Vector3 closest = sphereCollider.ClosestPoint(worldPos);
                // If ClosestPoint moved the position significantly, use it and approximate normal from center
                if ((closest - worldPos).sqrMagnitude > 1e-6f)
                {
                    worldPos = closest;
                    // approximate normal from sphere center to surface point (works well for spheres/ellipsoids)
                    worldNormal = (worldPos - sphere.position).normalized;
                }
            }

            // apply small offset along normal if requested
            worldPos += worldNormal * surfaceOffset;

            // pick random prefab
            GameObject chosen = prefabs[Random.Range(0, prefabs.Length)];
            if (chosen == null) continue;

#if UNITY_EDITOR
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(chosen);
#else
            GameObject instance = Instantiate(chosen);
#endif

            instance.transform.position = worldPos;

            if (alignToNormal)
            {
                // align object's up to the surface normal
                instance.transform.up = worldNormal;
            }

            if (randomYaw)
            {
                // random yaw rotation around object's up axis
                instance.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);
            }

            // random uniform scale multiplier
            float s = Random.Range(scaleRange.x, scaleRange.y);
            instance.transform.localScale = instance.transform.localScale * s;

            Transform parentTarget = spawnCollector != null ? spawnCollector : transform;
            instance.transform.SetParent(parentTarget);
            instance.name = chosen.name + "_" + i;
        }

        Debug.Log($"SpawnClutter: Spawned {count} objects across sphere surface.");
    }
}
