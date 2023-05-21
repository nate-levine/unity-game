using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
public class ChunkMeshGenerator : MonoBehaviour
{
    public static ChunkMeshGenerator Instance { get; private set; }

    Mesh mesh;
    public LineController lineController;

    Vector3 cutoutPosition;

    List<Vector3> meshVertices = new List<Vector3>();
    List<int>[] meshTriangles = new List<int>[2];
    List<Vector2> meshUVs = new List<Vector2>();
    List<Color> meshColors = new List<Color>();

    Vector3 screenPosition;
    Vector3 worldPosition;

    public float tangentScalar;
    public float normalScalar;
    Vector3 worldPositionInitial;
    Vector3 worldPositionFinal;

    bool leftMouseDown = false;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        lineController = GameObject.Find("Line Renderer").GetComponent<LineController>();
        for (int i = 0; i < meshTriangles.Length; i++)
        {
            meshTriangles[i] = new List<int>();
        }
    }

    public void ClearMeshData()
    {
        meshVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            meshTriangles[subMeshIndex].Clear();
        }
        meshUVs.Clear();
        meshColors.Clear();
        mesh.Clear();
    }

    public void GenerateMesh()
    {
        int verticesIndex = 0;
        foreach (Chunk chunk in ChunkLoader.Instance.loadedChunks)
        {
            meshVertices.AddRange(chunk.vertices);
            // offset each chunk's triangle indices by the amount of vertices in the chunk.
            for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
            {
                for (int i = 0; i < chunk.triangles[subMeshIndex].Count; i++)
                {
                    meshTriangles[subMeshIndex].Add(chunk.triangles[subMeshIndex][i] + verticesIndex);
                }
            }
            verticesIndex += chunk.vertices.Count;

            meshUVs.AddRange(chunk.UVs);
            meshColors.AddRange(chunk.colors);
        }

        // load the mesh data into the mesh filter.
        mesh.subMeshCount = 2;
        mesh.vertices = meshVertices.ToArray();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            mesh.SetTriangles(meshTriangles[subMeshIndex].ToArray(), subMeshIndex);
        }
        mesh.uv = meshUVs.ToArray();
        mesh.colors = meshColors.ToArray();
    }


    void Update()
    {
        screenPosition = Input.mousePosition;
        screenPosition.z = Camera.main.nearClipPlane;
        worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // since the plane is not at z = 0, the normal vector points more towards the z axis as it tends towards the origin. To prevent this, only the (2D) component parallel to the plane is taken.
        Vector3 normal = Vector3.Normalize(new Vector3(worldPosition.x, worldPosition.y, 0.0f));
        Vector3 tangent = Vector3.Normalize(Vector3.Cross(new Vector3(0.0f, 0.0f, 1.0f), worldPosition));

        List<Vector3> linePoints = new List<Vector3>();
        if (Input.GetMouseButtonDown(0))
        {
            leftMouseDown = true;
            worldPositionInitial = worldPosition;

            linePoints = new List<Vector3>() { new Vector3((worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition - (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition - (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition - (tangent * tangentScalar) + (normal * normalScalar)).x, (worldPosition - (tangent * tangentScalar) + (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition + (tangent * tangentScalar) + (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) + (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),};
        }
        else if (Input.GetMouseButton(0) && leftMouseDown)
        {
            worldPositionFinal = worldPosition;
            Vector3 worldPositionAverage = (worldPositionInitial + worldPositionFinal) / 2.0f;
            float cutTangentScalar = (Vector3.Magnitude(worldPositionFinal - worldPositionInitial) / 2.0f) + tangentScalar;
            Vector3 cutTangent = Vector3.Normalize(worldPositionFinal - worldPositionInitial);
            if (Vector3.Magnitude(cutTangent) == 0.0f)
                cutTangent = tangent;
            Vector3 cutNormal = Vector3.Normalize(Vector3.Cross((worldPositionFinal - worldPositionInitial), new Vector3(0.0f, 0.0f, 1.0f)));
            if (Vector3.Magnitude(cutNormal) == 0.0f)
                cutNormal = normal;

            linePoints = new List<Vector3>() { new Vector3((worldPositionAverage + (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).x, (worldPositionAverage + (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPositionAverage - (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).x, (worldPositionAverage - (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPositionAverage - (cutTangent * cutTangentScalar) + (cutNormal * normalScalar)).x, (worldPositionAverage - (cutTangent * cutTangentScalar) + (cutNormal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPositionAverage + (cutTangent * cutTangentScalar) + (cutNormal * normalScalar)).x, (worldPositionAverage + (cutTangent * cutTangentScalar) + (cutNormal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPositionAverage + (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).x, (worldPositionAverage + (cutTangent * cutTangentScalar) - (cutNormal * normalScalar)).y, 0.0f),};
        }
        else if (Input.GetMouseButtonUp(0) && leftMouseDown)
        {
            leftMouseDown = false;

            Vector3 worldPositionAverage = (worldPositionInitial + worldPositionFinal) / 2.0f;
            float cutTangentScalar = (Vector3.Magnitude(worldPositionFinal - worldPositionInitial) / 2.0f) + tangentScalar;
            Vector3 cutTangent = Vector3.Normalize(worldPositionFinal - worldPositionInitial);
            if (Vector3.Magnitude(cutTangent) == 0.0f)
                cutTangent = tangent;
            Vector3 cutNormal = Vector3.Normalize(Vector3.Cross((worldPositionFinal - worldPositionInitial), new Vector3(0.0f, 0.0f, 1.0f)));
            if (Vector3.Magnitude(cutNormal) == 0.0f)
                cutNormal = normal;
            CutMesh(worldPositionAverage, cutTangent, cutTangentScalar, cutNormal, normalScalar);
            ChunkLoader.Instance.LoadChunks();
        }
        else
        {
            linePoints = new List<Vector3>() { new Vector3((worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition - (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition - (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition - (tangent * tangentScalar) + (normal * normalScalar)).x, (worldPosition - (tangent * tangentScalar) + (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition + (tangent * tangentScalar) + (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) + (normal * normalScalar)).y, 0.0f),
                                               new Vector3((worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).x, (worldPosition + (tangent * tangentScalar) - (normal * normalScalar)).y, 0.0f),};
        }

        /*if (Input.GetMouseButtonDown(1))
        {
            rightMouseDown = true;
            worldPositionInitial = worldPosition;
        }
        else if (Input.GetMouseButtonUp(1) && rightMouseDown)
        {
            rightMouseDown = false;
            PlaceMesh(worldPosition + new Vector3(0.0f, 0.0f, 1.0f), tangent, tangentScalar, normal, normalScalar);
            ChunkLoader.Instance.LoadChunks();
        }*/

        lineController.RenderLine(linePoints);
    }

    void CutMesh(Vector3 worldPosition, Vector3 tangent, float tangentScalar, Vector3 normal, float normalScalar)
    {
        cutoutPosition = new Vector3(worldPosition.x, worldPosition.y, 0.0f);
        Vector3 position = cutoutPosition - transform.position;

        foreach (Vector3 chunkKey in ChunkLoader.Instance.currentChunkKeys)
        {
            Chunk dictChunk = ChunkManager.Instance.chunkDictionary[chunkKey];

            List<Vector3> planePositions = new List<Vector3>(){ position + (tangent * tangentScalar),
                                                                  position - (normal * normalScalar),
                                                                position - (tangent * tangentScalar),
                                                                  position + (normal * normalScalar), };
            List<Vector3> planeNormals = new List<Vector3>(){ -tangent,
                                                                normal,
                                                               tangent,
                                                               -normal, };
            List<float[]> planeBounds = new List<float[]>(){ new float[2] {-normalScalar , normalScalar },
                                                             new float[2] {-tangentScalar, tangentScalar},
                                                             new float[2] {-normalScalar , normalScalar },
                                                             new float[2] {-tangentScalar, tangentScalar}, };

            var (vertices, triangles, UVs, colors) = (new List<Vector3>(), new List<int>[2], new List<Vector2>(), new List<Color>());
            for (int i = 0; i <= 3; i++)
            {
                (vertices, triangles, UVs, colors) = this.GetComponent<Sliceable>().LineSegmentSliceMesh(dictChunk.vertices, dictChunk.triangles, dictChunk.UVs, dictChunk.colors, planeNormals[i], planePositions[i], planeBounds[i][0], planeBounds[i][1]);
                
                // new List<T>() syntax makes the lists a value type instead of a reference type. This way, the dictionary chunks are not directly referencing the values, but recieving a copy of the values.
                dictChunk.vertices = new List<Vector3>(vertices);
                for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
                {
                    dictChunk.triangles[subMeshIndex] = new List<int>(triangles[subMeshIndex]);
                }
                dictChunk.UVs = new List<Vector2>(UVs);
                dictChunk.colors = new List<Color>(colors);
            }
            
            // Delete vertices inside cutout
            (vertices, triangles, UVs, colors) = this.GetComponent<Sliceable>().DeleteMesh(dictChunk.vertices, dictChunk.triangles, dictChunk.UVs, dictChunk.colors, planeNormals, planePositions);
            // new List<T>() syntax makes the lists a value type instead of a reference type. This way, the dictionary chunks are not directly referencing the values, but recieving a copy of the values.
            dictChunk.vertices = new List<Vector3>(vertices);
            for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
            {
                dictChunk.triangles[subMeshIndex] = new List<int>(triangles[subMeshIndex]);
            }
            dictChunk.UVs = new List<Vector2>(UVs);
            dictChunk.colors = new List<Color>(colors);
        }
    }

    void PlaceMesh(Vector3 worldPosition, Vector3 tangent, float tangentScalar, Vector3 normal, float normalScalar)
    {
        cutoutPosition = new Vector3(worldPosition.x, worldPosition.y, 0.0f);
        Vector3 position = cutoutPosition - transform.position;

        Vector3 chunkKey = new Vector3((int)(position.x / 8.0f), (int)(position.y / 8.0f), 0);
        Chunk dictChunk = ChunkManager.Instance.chunkDictionary[chunkKey];

        List<Vector3> vertices = new List<Vector3>() { position + (tangent * tangentScalar) - (normal * normalScalar),
                                                       position - (tangent * tangentScalar) - (normal * normalScalar),
                                                       position - (tangent * tangentScalar) + (normal * normalScalar),
                                                       position + (tangent * tangentScalar) - (normal * normalScalar),
                                                       position - (tangent * tangentScalar) + (normal * normalScalar),
                                                       position + (tangent * tangentScalar) + (normal * normalScalar), };
        List<int> triangles = new List<int>() { dictChunk.vertices.Count + 0,
                                                dictChunk.vertices.Count + 1,
                                                dictChunk.vertices.Count + 2,
                                                dictChunk.vertices.Count + 3,
                                                dictChunk.vertices.Count + 4,
                                                dictChunk.vertices.Count + 5, };
        List<Vector2> UVs = new List<Vector2>() { new Vector2(1.0f, 0.0f),
                                                  new Vector2(0.0f, 0.0f),
                                                  new Vector2(0.0f, 1.0f),
                                                  new Vector2(1.0f, 0.0f),
                                                  new Vector2(0.0f, 1.0f),
                                                  new Vector2(1.0f, 1.0f), };
        dictChunk.vertices.AddRange(vertices);
        dictChunk.triangles[0].AddRange(triangles);
        dictChunk.UVs.AddRange(UVs);
    }
}