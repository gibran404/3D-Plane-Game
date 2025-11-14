// Place this file inside an "Editor" folder in your project.
using UnityEditor;
using UnityEngine;

public class TerrainLoopFixer : EditorWindow
{
    Terrain targetTerrain;
    int copyRows = 1;          // how many rows to copy
    bool copyHeights = true;
    bool copyAlphamaps = true;
    bool copyDetails = false;

    [MenuItem("Tools/Terrain Loop Fixer")]
    static void OpenWindow() => GetWindow<TerrainLoopFixer>("Terrain Loop Fixer");

    void OnGUI()
    {
        GUILayout.Label("Make Terrain Loopable (Copy edges)", EditorStyles.boldLabel);
        targetTerrain = EditorGUILayout.ObjectField("Terrain", targetTerrain, typeof(Terrain), true) as Terrain;
        copyRows = EditorGUILayout.IntField("Rows / Columns to copy", copyRows);

        copyHeights = EditorGUILayout.Toggle("Copy Heights", copyHeights);
        copyAlphamaps = EditorGUILayout.Toggle("Copy Alphamaps (textures)", copyAlphamaps);
        copyDetails = EditorGUILayout.Toggle("Copy Details (grass)", copyDetails);

        if (GUILayout.Button("Make Loopable Z-axis (start -> end)"))
            MakeLoopableZ();

        if (GUILayout.Button("Make Loopable X-axis (left -> right)"))
            MakeLoopableX();
    }

    void MakeLoopableZ()
    {
        if (targetTerrain == null) { Debug.LogWarning("Assign a Terrain."); return; }
        TerrainData data = targetTerrain.terrainData;

        if (copyHeights)
        {
            int w = data.heightmapResolution;
            int h = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, w, h);

            // copy first 'copyRows' rows (at z=0) to last 'copyRows' rows (at z=h - copyRows)
            for (int row = 0; row < copyRows; row++)
            {
                int srcZ = row;
                int dstZ = h - copyRows + row;
                for (int x = 0; x < w; x++)
                    heights[dstZ, x] = heights[srcZ, x];
            }
            data.SetHeights(0, 0, heights);
        }

        if (copyAlphamaps)
        {
            int alphW = data.alphamapWidth;
            int alphH = data.alphamapHeight;
            int layers = data.alphamapLayers;
            float[,,] alphas = data.GetAlphamaps(0, 0, alphW, alphH);

            for (int row = 0; row < copyRows; row++)
            {
                int srcZ = row;
                int dstZ = alphH - copyRows + row;
                for (int x = 0; x < alphW; x++)
                    for (int l = 0; l < layers; l++)
                        alphas[dstZ, x, l] = alphas[srcZ, x, l];
            }
            data.SetAlphamaps(0, 0, alphas);
        }

        if (copyDetails)
        {
            int detailW = data.detailWidth;
            int detailH = data.detailHeight;
            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                int[,] layer = data.GetDetailLayer(0, 0, detailW, detailH, i);
                for (int row = 0; row < copyRows; row++)
                {
                    int srcZ = row;
                    int dstZ = detailH - copyRows + row;
                    for (int x = 0; x < detailW; x++)
                        layer[dstZ, x] = layer[srcZ, x];
                }
                data.SetDetailLayer(0, 0, i, layer);
            }
        }

        Debug.Log("Terrain Z-axis loop fix applied.");
    }

    void MakeLoopableX()
    {
        if (targetTerrain == null) { Debug.LogWarning("Assign a Terrain."); return; }
        TerrainData data = targetTerrain.terrainData;

        if (copyHeights)
        {
            int w = data.heightmapResolution;
            int h = data.heightmapResolution;
            float[,] heights = data.GetHeights(0, 0, w, h);

            // copy first 'copyRows' columns (x=0) to last columns (x = w - copyRows)
            for (int col = 0; col < copyRows; col++)
            {
                int srcX = col;
                int dstX = w - copyRows + col;
                for (int z = 0; z < h; z++)
                    heights[z, dstX] = heights[z, srcX];
            }
            data.SetHeights(0, 0, heights);
        }

        if (copyAlphamaps)
        {
            int alphW = data.alphamapWidth;
            int alphH = data.alphamapHeight;
            int layers = data.alphamapLayers;
            float[,,] alphas = data.GetAlphamaps(0, 0, alphW, alphH);

            for (int col = 0; col < copyRows; col++)
            {
                int srcX = col;
                int dstX = alphW - copyRows + col;
                for (int z = 0; z < alphH; z++)
                    for (int l = 0; l < layers; l++)
                        alphas[z, dstX, l] = alphas[z, srcX, l];
            }
            data.SetAlphamaps(0, 0, alphas);
        }

        if (copyDetails)
        {
            int detailW = data.detailWidth;
            int detailH = data.detailHeight;
            for (int i = 0; i < data.detailPrototypes.Length; i++)
            {
                int[,] layer = data.GetDetailLayer(0, 0, detailW, detailH, i);
                for (int col = 0; col < copyRows; col++)
                {
                    int srcX = col;
                    int dstX = detailW - copyRows + col;
                    for (int z = 0; z < detailH; z++)
                        layer[z, dstX] = layer[z, srcX];
                }
                data.SetDetailLayer(0, 0, i, layer);
            }
        }

        Debug.Log("Terrain X-axis loop fix applied.");
    }
}
