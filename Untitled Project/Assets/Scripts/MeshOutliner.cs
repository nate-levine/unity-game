using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshOutliner : MonoBehaviour
{
    public static MeshOutliner Instance { get; private set; }

    public Mesh mesh;
    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshIndices = new List<int>();

    public float lineThickness;

    // initialize instance of singleton
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        // Instantiate mesh.
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // A function to generate the mesh outline
    public void GenerateOutline(Mesh worldMesh)
    {
        meshVertices.Clear();
        meshIndices.Clear();

        // Initialize lists
        List<Vector3> vertices = new List<Vector3>();
        List<int>[] triangles = new List<int>[2];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = new List<int>();
        }
        List<Vector2> UV2s = new List<Vector2>();

        // Fill mesh data into lists
        vertices.AddRange(worldMesh.vertices);
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i].AddRange(worldMesh.GetTriangles(i));
        }
        UV2s.AddRange(worldMesh.uv2);

        // iterate through each vertex in the mesh.
        List<int> edgeIndices = new List<int>();
        for (int i = 0; i < vertices.Count; i++)
        {
            // if the vertex has imbedded edge data, add it to the list of edge vertices.
            if (UV2s[i].x == 1.0f)
                edgeIndices.Add(i);
        }

        for (int i = 0; i < edgeIndices.Count; i++)
        {
            Vector3 vertex = vertices[edgeIndices[i]];

            meshVertices.Add(vertex + new Vector3( lineThickness, -lineThickness, 0.0f));
            meshVertices.Add(vertex + new Vector3(-lineThickness, -lineThickness, 0.0f));
            meshVertices.Add(vertex + new Vector3(-lineThickness,  lineThickness, 0.0f));
            meshVertices.Add(vertex + new Vector3( lineThickness,  lineThickness, 0.0f));

            meshIndices.Add((i * 4) + 0);
            meshIndices.Add((i * 4) + 1);
            meshIndices.Add((i * 4) + 2);
            meshIndices.Add((i * 4) + 0);
            meshIndices.Add((i * 4) + 2);
            meshIndices.Add((i * 4) + 3);
        }


        // Render the line.
        mesh.Clear();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshIndices.ToArray();
    }
}
