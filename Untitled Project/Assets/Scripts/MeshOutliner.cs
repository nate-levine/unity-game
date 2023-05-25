using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshOutliner : MonoBehaviour
{
    public static MeshOutliner Instance { get; private set; }

    public Mesh mesh;
    List<Vector3> meshVertices = new List<Vector3>();
    List<int> meshIndices = new List<int>();
    List<Color> meshColors = new List<Color>();

    // A struct to store edge data
    struct Edge
    {
        public Vector3 vertex0;
        public Vector3 vertex1;

        // not rendered but used for calculations
        public Vector3 vertex2;
    }
    struct Line
    {
        public List<Vector3> verts;
        public List<int> ints;
        public List<Color> colors;

        public Line(Vector3 v0, Vector3 v1, Vector3 v2, float thickness, int offset)
        {
            verts = new List<Vector3>();
            ints = new List<int>();
            colors = new List<Color>();

            Vector3 tangent = (v1 - v0).normalized;
            Vector3 normal = Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), tangent).normalized;

            verts.Add((v1 + (( tangent - normal) * thickness)));
            verts.Add((v0 + ((-tangent - normal) * thickness)));
            verts.Add((v0 + ((-tangent + normal) * thickness)));
            verts.Add((v1 + (( tangent + normal) * thickness)));

            ints.AddRange(new List<int>(){ 0 + offset, 1 + offset, 2 + offset, 0 + offset, 2 + offset, 3 + offset });

            colors.Add(new Color(0.0f, 0.0f, 0.0f));
            colors.Add(new Color(0.0f, 0.0f, 0.0f));
            colors.Add(new Color(0.0f, 0.0f, 0.0f));
            colors.Add(new Color(0.0f, 0.0f, 0.0f));
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

        /* This dictionary contains <edge data, # of similar edges> in <Edge, int> form.
         * The first vertex of the edge contains the 2D position of the vertex of the edge with the lowest x-position.
         * This decision is arbitrary, and is only one of many methods to consistantly organize edges into similar groups,
         * without needing to know which vertex of the edge to start with.
         */
        Dictionary<Edge, int> edgeDictionary = new Dictionary<Edge, int>();

        // iterate through each edge of each triangle in the mesh.
        for (int subMeshIndex = 0; subMeshIndex < worldMesh.subMeshCount; subMeshIndex++)
        {
            for (int i = 0; i < triangles[subMeshIndex].Count; i += 3)
            {
                // retireive triangle vertex data.
                int tri0 = triangles[subMeshIndex][i + 0];
                int tri1 = triangles[subMeshIndex][i + 1];
                int tri2 = triangles[subMeshIndex][i + 2];
                Vector3 vert0 = vertices[tri0];
                Vector3 vert1 = vertices[tri1];
                Vector3 vert2 = vertices[tri2];

                // initialize edges
                Edge edge0 = new Edge();
                Edge edge1 = new Edge();
                Edge edge2 = new Edge();
                // input edge data
                if (vert0.x < vert1.x)
                {
                    edge0.vertex0 = vert0;
                    edge0.vertex1 = vert1;
                    edge0.vertex2 = vert2;
                }
                else
                {
                    edge0.vertex0 = vert1;
                    edge0.vertex1 = vert0;
                    edge0.vertex2 = vert2;
                }
                if (vert1.x < vert2.x)
                {
                    edge1.vertex0 = vert1;
                    edge1.vertex1 = vert2;
                    edge1.vertex2 = vert0;
                }
                else
                {
                    edge1.vertex0 = vert2;
                    edge1.vertex1 = vert1;
                    edge1.vertex2 = vert0;
                }
                if (vert2.x < vert0.x)
                {
                    edge2.vertex0 = vert2;
                    edge2.vertex1 = vert0;
                    edge2.vertex2 = vert1;
                }
                else
                {
                    edge2.vertex0 = vert0;
                    edge2.vertex1 = vert2;
                    edge2.vertex2 = vert1;
                }

                // iterate through each edge in the triangle
                Edge[] edges = new Edge[3] { edge0, edge1, edge2 };
                foreach (Edge edge in edges)
                {
                    // if the dictionary already contains a similar edge, increment the count by 1.
                    if (edgeDictionary.ContainsKey(edge))
                    {
                        edgeDictionary[edge]++;
                    }
                    // if no similar edge is in the dictionary, add the edge with a count of 1.
                    else
                    {
                        edgeDictionary.Add(edge, 1);
                    }
                }
            }
        }

        // iterate through every dictionary key, value pair
        foreach (KeyValuePair<Edge, int> edgeEntry in edgeDictionary)
        {
            // if the entry is unique (value is 1), then the key (edge information) is not shared, and therefore is on the outline of the mesh.
            if (edgeEntry.Value == 1)
            {
                // Create new line.
                int offset = meshVertices.Count;
                Line line = new Line(edgeEntry.Key.vertex0, edgeEntry.Key.vertex1, edgeEntry.Key.vertex2, 0.05f, offset);

                foreach (Vector3 vertex in line.verts)
                {
                    meshVertices.Add(vertex);
                }
                foreach (int index in line.ints)
                {
                    meshIndices.Add(index);
                }
                foreach (Color color in line.colors)
                {
                    meshColors.Add(color);
                }
            }
        }

        // Render the line.
        mesh.Clear();
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshIndices.ToArray();
        mesh.colors = meshColors.ToArray();
    }
}
