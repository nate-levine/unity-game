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
    List<Color> meshColors = new List<Color>();

    public float lineThickness;

    struct Edge
    {
        public int tri0;
        public int tri1;

        public Edge(int t0, int t1)
        {
            tri0 = t0;
            tri1 = t1;
        }
    }

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
        meshColors.Clear();

        // Initialize lists
        List<Vector3> vertices = new List<Vector3>();
        List<int>[] triangles = new List<int>[2];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = new List<int>();
        }
        List<Vector2> UVs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        // Fill mesh data into lists
        vertices.AddRange(worldMesh.vertices);
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i].AddRange(worldMesh.GetTriangles(i));
        }
        UVs.AddRange(worldMesh.uv);
        colors.AddRange(worldMesh.colors);

        List<Edge>[] vertEdges = new List<Edge>[vertices.Count];
        for (int i = 0; i < vertEdges.Length; i++)
        {
            vertEdges[i] = new List<Edge>();
        }
        int[] vertTrianglesCount = new int[vertices.Count];
        int[] vertEdgesCount = new int[vertices.Count];
        List<int> borderVertIndices = new List<int>();

        // iterate through each edge of each triangle in the mesh.
        for (int subMeshIndex = 0; subMeshIndex < worldMesh.subMeshCount; subMeshIndex++)
        {
            for (int i = 0; i < triangles[subMeshIndex].Count; i += 3)
            {
                // retireive triangle vertex data.
                int tri0 = triangles[subMeshIndex][i + 0];
                int tri1 = triangles[subMeshIndex][i + 1];
                int tri2 = triangles[subMeshIndex][i + 2];

                vertTrianglesCount[tri0]++;
                vertTrianglesCount[tri1]++;
                vertTrianglesCount[tri2]++;

                vertEdges[tri0].Add(new Edge(tri0, tri1));
                vertEdges[tri0].Add(new Edge(tri0, tri2));

                vertEdges[tri1].Add(new Edge(tri1, tri0));
                vertEdges[tri1].Add(new Edge(tri1, tri2));

                vertEdges[tri2].Add(new Edge(tri2, tri0));
                vertEdges[tri2].Add(new Edge(tri2, tri1));
            }
        }

        for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
        {
            List<Edge> newVertEdges = new List<Edge>();
            vertEdgesCount[vertIndex] = 0;

            for (int i = 0; i < vertEdges[vertIndex].Count; i++)
            {
                bool duplicate = false;
                Edge edgeA = vertEdges[vertIndex][i];

                for (int j = 0; j < newVertEdges.Count; j++)
                {
                    Edge edgeB = newVertEdges[j];

                    if ((i != j) && (edgeA.tri0 == edgeB.tri0) && (edgeA.tri1 == edgeB.tri1))
                    {
                        duplicate = true;
                    }
                }

                if (!duplicate)
                {
                    vertEdgesCount[vertIndex]++;
                    newVertEdges.Add(edgeA);
                }
            }
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertTrianglesCount[i] < vertEdgesCount[i])
            {
                borderVertIndices.Add(i);
            }
        }

        List<int> newBorderVertIndices = new List<int>();
        for (int i = 0; i < borderVertIndices.Count; i++)
        {
            bool duplicate = false;

            int indexA = borderVertIndices[i];
            Vector3 vertexA = vertices[indexA];

            for (int j = 0; j < newBorderVertIndices.Count; j++)
            {
                int indexB = newBorderVertIndices[j];
                Vector3 vertexB = vertices[indexB];

                if (vertexA == vertexB)
                {
                    vertTrianglesCount[indexB] += vertTrianglesCount[indexA];

                    //
                    List<Edge> combinedEdges = new List<Edge>();
                    combinedEdges.AddRange(vertEdges[indexA]);
                    combinedEdges.AddRange(vertEdges[indexB]);

                    List<Edge> finalVertEdges = new List<Edge>();
                    for (int k = 0; k < combinedEdges.Count; k++)
                    {
                        bool edgeDuplicate = false;
                        Edge edgeA = combinedEdges[k];

                        for (int m = 0; m < finalVertEdges.Count; m++)
                        {
                            Edge edgeB = finalVertEdges[m];

                            if ((vertices[edgeA.tri0] == vertices[edgeB.tri0]) && (vertices[edgeA.tri1] == vertices[edgeB.tri1]))
                            {
                                edgeDuplicate = true;
                            }
                        }
                        if (!edgeDuplicate)
                        {
                            finalVertEdges.Add(edgeA);
                        }
                    }
                    vertEdgesCount[indexB] = finalVertEdges.Count;

                    duplicate = true;
                }
            }
            if (!duplicate)
            {
                newBorderVertIndices.Add(indexA);
            }
        }

        List<int> finalBorderVertIndices = new List<int>();
        for (int i = 0; i < newBorderVertIndices.Count; i++)
        {
            int index = newBorderVertIndices[i];

            if (vertTrianglesCount[index] < vertEdgesCount[index])
            {
                finalBorderVertIndices.Add(index);
            }
        }

        for (int i = 0; i < finalBorderVertIndices.Count; i++)
        {
            Vector3 vertex = vertices[finalBorderVertIndices[i]];

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
        mesh.colors = meshColors.ToArray();
    }
}
