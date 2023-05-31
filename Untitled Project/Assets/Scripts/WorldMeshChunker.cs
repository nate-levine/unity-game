using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMeshChunker : MonoBehaviour
{
    // this variable is a placeholder variable to make sure the chunking happens at the right time in relation to other events. It may need refactoring in the future.
    bool run = true;

    void Update()
    {
        if (run)
        {
            List<Vector3> vertices_V = new List<Vector3>(WorldMeshGenerator.Instance.vertices);
            List<int>[] triangles_V = new List<int>[2];
            for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
            {
                triangles_V[subMeshIndex] = new List<int>(WorldMeshGenerator.Instance.triangles[subMeshIndex]);
            }
            List<Vector3> normals_V = new List<Vector3>(WorldMeshGenerator.Instance.normals);
            List<Vector2> UVs_V = new List<Vector2>(WorldMeshGenerator.Instance.UVs);
            List<Vector2> UV2s_V = new List<Vector2>(WorldMeshGenerator.Instance.UV2s);
            List<Color> colors_V = new List<Color>(WorldMeshGenerator.Instance.colors);

            for (int i = -64; i <= 64; i++)
            {
                Vector3 planePosition_V = new Vector3((i * ChunkManager.Instance.chunkSize) + (ChunkManager.Instance.chunkSize / 2), 0.0f, 0.0f);
                var (positiveVertices_V, positiveTriangles_V, positiveNormals_V, positiveUVs_V, positiveUV2s_V, positiveColors_V, negativeVertices_V, negativeTriangles_V, negativeNormals_V, negativeUVs_V, negativeUV2s_V, negativeColors_V) =
                    this.GetComponent<Sliceable>().LineSliceMesh(vertices_V, triangles_V, normals_V, UVs_V, UV2s_V, colors_V, new Vector3(1.0f, 0.0f, 0.0f), planePosition_V, -1000000.0f, 1000000.0f);

                vertices_V = new List<Vector3>(positiveVertices_V);
                for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
                {
                    triangles_V[subMeshIndex] = new List<int>(positiveTriangles_V[subMeshIndex]);
                }
                normals_V = new List<Vector3>(positiveNormals_V);
                UVs_V = new List<Vector2>(positiveUVs_V);
                UV2s_V = new List<Vector2>(positiveUV2s_V);
                colors_V = new List<Color>(positiveColors_V);

                List<Vector3> vertices_H = new List<Vector3>(negativeVertices_V);
                List<int>[] triangles_H = new List<int>[2];
                for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
                {
                    triangles_H[subMeshIndex] = new List<int>(negativeTriangles_V[subMeshIndex]);
                }
                List<Vector3> normals_H = new List<Vector3>(negativeNormals_V);
                List<Vector2> UVs_H = new List<Vector2>(negativeUVs_V);
                List<Vector2> UV2s_H = new List<Vector2>(negativeUV2s_V);
                List<Color> colors_H = new List<Color>(negativeColors_V);

                for (int j = -64; j <= 64; j++)
                {
                    Vector3 planePosition_H = new Vector3(0.0f, (j * ChunkManager.Instance.chunkSize) + (ChunkManager.Instance.chunkSize / 2), 0.0f);
                    var (positiveVertices_H, positiveTriangles_H, positiveNormals_H, positiveUVs_H, positiveUV2s_H, positiveColors_H, negativeVertices_H, negativeTriangles_H, negativeNormals_H, negativeUVs_H, negativeUV2s_H, negativeColors_H) =
                        this.GetComponent<Sliceable>().LineSliceMesh(vertices_H, triangles_H, normals_H, UVs_H, UV2s_H, colors_H, new Vector3(0.0f, 1.0f, 0.0f), planePosition_H, -1000000.0f, 1000000.0f);

                    vertices_H = new List<Vector3>(positiveVertices_H);
                    for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
                    {
                        triangles_H[subMeshIndex] = new List<int>(positiveTriangles_H[subMeshIndex]);
                    }
                    normals_H = new List<Vector3>(positiveNormals_H);
                    UVs_H = new List<Vector2>(positiveUVs_H);
                    UV2s_H = new List<Vector2>(positiveUV2s_H);
                    colors_H = new List<Color>(positiveColors_H);

                    Vector3 chunkKey = new Vector3(i, j, 0);
                    if (ChunkManager.Instance.chunkDictionary.ContainsKey(chunkKey))
                    {
                        ChunkManager.Instance.chunkDictionary[chunkKey].vertices.AddRange(new List<Vector3>(negativeVertices_H));
                        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
                        {
                            ChunkManager.Instance.chunkDictionary[chunkKey].triangles[subMeshIndex].AddRange(new List<int>(negativeTriangles_H[subMeshIndex]));
                        }
                        ChunkManager.Instance.chunkDictionary[chunkKey].normals.AddRange(new List<Vector3>(negativeNormals_H));
                        ChunkManager.Instance.chunkDictionary[chunkKey].UVs.AddRange(new List<Vector2>(negativeUVs_H));
                        ChunkManager.Instance.chunkDictionary[chunkKey].UV2s.AddRange(new List<Vector2>(negativeUV2s_H));
                        ChunkManager.Instance.chunkDictionary[chunkKey].colors.AddRange(new List<Color>(negativeColors_H));
                    }
                }
            }

            run = false;
        }
    }
}
