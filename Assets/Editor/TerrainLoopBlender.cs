// Place in Editor folder.
using UnityEditor;
using UnityEngine;

public class TerrainLoopBlender : EditorWindow
{
    Terrain targetTerrain;
    public int blendWidth = 8; // number of rows to blend
    public bool blendHeights = true;
    public bool blendAlphamaps = true;

    [MenuItem("Tools/Terrain Loop Blender")]
    static void Open() => GetWindow<TerrainLoopBlender>("Terrain Loop Blender");

    void OnGUI()
    {
        targetTerrain = EditorGUILayout.ObjectField("Terrain", targetTerrain, typeof(Terrain), true) as Terrain;
        blendWidth = EditorGUILayout.IntField("Blend Width (rows)", blendWidth);
        blendHeights = EditorGUILayout.Toggle("Blend Heights", blendHeights);
        blendAlphamaps = EditorGUILayout.Toggle("Blend Alphamaps", blendAlphamaps);

        if (GUILayout.Button("Blend Z-axis (start <-> end)"))
            BlendZ();

        if (GUILayout.Button("Blend X-axis (left <-> right)"))
            BlendX();
    }

    void BlendZ()
    {
        if (!targetTerrain) { Debug.LogWarning("Assign a terrain"); return; }
        TerrainData d = targetTerrain.terrainData;
        int res = d.heightmapResolution;
        float[,] h = d.GetHeights(0,0,res,res);

        int bw = Mathf.Clamp(blendWidth, 1, res/2 - 1);

        for (int row = 0; row < bw; row++)
        {
            float t = (float)row / (bw - 1); // 0 .. 1
            int src = row;
            int dst = res - bw + row;
            for (int x = 0; x < res; x++)
            {
                float a = h[src, x];
                float b = h[dst, x];
                float blended = Mathf.Lerp(a, b, 0.5f);       // midpoint
                // interpolate so the outer rows move closer
                h[src, x] = Mathf.Lerp(a, blended, t);
                h[dst, x] = Mathf.Lerp(b, blended, t);
            }
        }
        if (blendHeights) d.SetHeights(0,0,h);

        if (blendAlphamaps)
        {
            int aw = d.alphamapWidth;
            int ah = d.alphamapHeight;
            int layers = d.alphamapLayers;
            float[,,] al = d.GetAlphamaps(0,0,aw,ah);
            int bwA = Mathf.Clamp(blendWidth, 1, ah/2 - 1);
            for (int row = 0; row < bwA; row++)
            {
                float t = (float)row / (bwA - 1);
                int src = row;
                int dst = ah - bwA + row;
                for (int x = 0; x < aw; x++)
                {
                    for (int l = 0; l < layers; l++)
                    {
                        float va = al[src, x, l];
                        float vb = al[dst, x, l];
                        float mid = (va + vb) * 0.5f;
                        al[src, x, l] = Mathf.Lerp(va, mid, t);
                        al[dst, x, l] = Mathf.Lerp(vb, mid, t);
                    }
                }
                // Normalize per cell to ensure alpha layers sum to 1.
                for (int x = 0; x < aw; x++)
                {
                    float sumA = 0, sumB = 0;
                    for (int l = 0; l < layers; l++) { sumA += al[src, x, l]; sumB += al[dst, x, l]; }
                    if (sumA > 0) for (int l = 0; l < layers; l++) al[src, x, l] /= sumA;
                    if (sumB > 0) for (int l = 0; l < layers; l++) al[dst, x, l] /= sumB;
                }
            }
            d.SetAlphamaps(0,0,al);
        }

        Debug.Log("Blended terrain Z-axis edges.");
    }

    void BlendX()
    {
        // Similar logic but for X axis. For brevity, implement same pattern as BlendZ
        Debug.Log("BlendX not implemented in this sample. Use BlendZ then rotate terrain or replicate logic.");
    }
}
