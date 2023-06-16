using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    // Chunk meshes.
    public GameObject shadowObject;
    private Mesh mesh;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void GenerateShadows()
    {
        /* Get mesh data to pass the compute shader.
         * The reason it is done in the manager is so that the mesh data processing doesn't need to
         * be done per light. This increases the load time drastically, causing a ton of lag.
         */
        Mesh mesh = new Mesh();
        if (shadowObject.GetComponent<ChunkMeshGenerator>())
        {
            mesh = shadowObject.GetComponent<ChunkMeshGenerator>().GetMesh();
        }

        // Get mesh from ChunkMeshGenerator.
        List<Vector3> meshVertices = new List<Vector3>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            meshVertices.Add(mesh.vertices[i]);
        }
        List<int> meshIndices = new List<int>();
        for (int i = 0; i < mesh.GetTriangles(0).Length; i++)
        {
            meshIndices.Add(mesh.GetTriangles(0)[i]);
        }
        for (int i = 0; i < mesh.GetTriangles(1).Length; i++)
        {
            meshIndices.Add(mesh.GetTriangles(1)[i]);
        }
        // Retrieve and store vertices.
        ShadowRenderer.ReadVertex[] vertices = new ShadowRenderer.ReadVertex[meshIndices.Count];
        int[] indices = new int[meshIndices.Count];
        // Retrieve, calculate, and store indices.
        for (int i = 0; i < meshIndices.Count; i++)
        {
            vertices[i] = new ShadowRenderer.ReadVertex()
            {
                position = meshVertices[meshIndices[i]]
            };
            indices[i] = i;
        }
        int numberOfIndices = indices.Length;

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<ShadowRenderer>())
            {
                child.gameObject.GetComponent<ShadowRenderer>().GenerateShadows(vertices, indices, numberOfIndices, shadowObject.transform.localToWorldMatrix);
            }
        }
    }
}
