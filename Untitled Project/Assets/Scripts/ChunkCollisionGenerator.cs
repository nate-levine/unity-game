using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ChunkCollisionGenerator : MonoBehaviour
{
    public GameObject target;
    public int colliderCount;
    public float maximumDistance;

    PolygonCollider2D polygonCollider;

    void Start()
    {
        // initialize polygon collider
        polygonCollider = GetComponent<PolygonCollider2D>();
        polygonCollider.pathCount = colliderCount;

        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            List<Vector2> empty = new List<Vector2>() { };
            polygonCollider.SetPath(i, empty);
        }
    }
    void Update()
    {
        LoadCollider();
    }

    private void LoadCollider()
    {
        List<Vector3> chunkKeys = new List<Vector3>();
        List<int> subMeshIndices = new List<int>();
        List<int> triangles = new List<int>();
        List<float> distances = new List<float>();

        foreach (Vector3 chunkKey in ChunkLoader.Instance.currentChunkKeys)
        {
            Chunk dictChunk = ChunkManager.Instance.chunkDictionary[chunkKey];

            for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
            {
                for (int i = 0; i < dictChunk.triangles[subMeshIndex].Count; i += 3)
                {
                    int tri0 = dictChunk.triangles[subMeshIndex][i + 0];
                    int tri1 = dictChunk.triangles[subMeshIndex][i + 1];
                    int tri2 = dictChunk.triangles[subMeshIndex][i + 2];
                    // get vertices for the given triangle.
                    Vector3 v0 = dictChunk.vertices[tri0];
                    Vector3 v1 = dictChunk.vertices[tri1];
                    Vector3 v2 = dictChunk.vertices[tri2];

                    // lerp for each line of the triangle.
                    float t0 = Vector3.Dot(target.transform.position - v0, v1 - v0) / Mathf.Pow(Vector3.Magnitude(v1 - v0), 2.0f);
                    float t1 = Vector3.Dot(target.transform.position - v1, v2 - v1) / Mathf.Pow(Vector3.Magnitude(v2 - v1), 2.0f);
                    float t2 = Vector3.Dot(target.transform.position - v2, v0 - v2) / Mathf.Pow(Vector3.Magnitude(v0 - v2), 2.0f);

                    // check if the lerp falls on the line segment (between 0.0f and 1.0f).
                    // if so, add to the lists of points and their corresponding distances that can be compared.
                    bool addTrianglePointsToComparision = false;
                    List<Vector3> closestPointsOnEachLineSegment = new List<Vector3>();
                    // line segment 0
                    if (t0 > 0.0f && t0 < 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(Vector3.Lerp(v0, v1, t0));
                    }
                    else if (t0 <= 0.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v0);
                    }
                    else if (t0 >= 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v1);
                    }
                    // line segment 1
                    if (t1 > 0.0f && t1 < 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(Vector3.Lerp(v1, v2, t1));
                    }
                    else if (t1 <= 0.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v1);
                    }
                    else if (t1 >= 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v2);
                    }
                    // line segment 2
                    if (t2 > 0.0f && t2 < 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(Vector3.Lerp(v2, v0, t2));
                    }
                    else if (t2 <= 0.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v2);
                    }
                    else if (t2 >= 1.0f)
                    {
                        closestPointsOnEachLineSegment.Add(v0);
                    }

                    // check if any of the points of the triangle fall within 5.0f distance of the target.
                    // if not, don't add it to the list. This DRASTICALLY reduces calculation times as most of the triangles in the mesh are discarded.
                    foreach (Vector3 validPoint in closestPointsOnEachLineSegment)
                    {
                        float pointDistance = Vector3.Magnitude(target.transform.position - validPoint);
                        if (pointDistance < maximumDistance)
                            addTrianglePointsToComparision = true;
                    }
                    if (addTrianglePointsToComparision)
                    {
                        // run through all points in the triangle, swapping the lowest magnitude whenever a new "challenger" is lower that the current "champion"
                        float lowestDistance = Vector3.Magnitude(target.transform.position - closestPointsOnEachLineSegment[0]);
                        for (int j = 1; j < closestPointsOnEachLineSegment.Count; j++)
                        {
                            float pointDistance = Vector3.Magnitude(target.transform.position - closestPointsOnEachLineSegment[j]);
                            if (pointDistance < lowestDistance)
                            {
                                lowestDistance = pointDistance;
                            }
                        }
                        // store the vertex information in these lists.
                        chunkKeys.Add(chunkKey);
                        subMeshIndices.Add(subMeshIndex);
                        triangles.Add(i);
                        distances.Add(lowestDistance);
                    }
                }
            }
        }
        
        for (int i = 0; i < colliderCount; i++)
        {
            if (triangles.Count > 0)
            {
                // retrieve the list index for the lowest distance.
                float lowestDistance = distances.Min();
                int listIndex = distances.IndexOf(lowestDistance);
                // find chunk
                Vector3 chunkKey = chunkKeys[listIndex];
                Chunk chunk = ChunkManager.Instance.chunkDictionary[chunkKey];
                // find submesh
                int subMeshIndex = subMeshIndices[listIndex];
                // load vertices into collider path
                List<Vector2> colliderPath = new List<Vector2>();
                colliderPath.Add(chunk.vertices[chunk.triangles[subMeshIndex][triangles[listIndex] + 0]]);
                colliderPath.Add(chunk.vertices[chunk.triangles[subMeshIndex][triangles[listIndex] + 1]]);
                colliderPath.Add(chunk.vertices[chunk.triangles[subMeshIndex][triangles[listIndex] + 2]]);
                polygonCollider.SetPath(i, colliderPath.ToArray());

                chunkKeys.RemoveAt(listIndex);
                subMeshIndices.RemoveAt(listIndex);
                triangles.RemoveAt(listIndex);
                distances.RemoveAt(listIndex);
            }
            // sometimes there arent any edges within the specified radius. In this case, set the collider paths to zero for the meantime
            else
            {
                List<Vector2> colliderPath = new List<Vector2>();
                colliderPath.Add(new Vector2(0.0f, 0.0f));
                colliderPath.Add(new Vector2(0.0f, 0.0f));
                colliderPath.Add(new Vector2(0.0f, 0.0f));
                polygonCollider.SetPath(i, colliderPath.ToArray());
            }
        }
    }
}
