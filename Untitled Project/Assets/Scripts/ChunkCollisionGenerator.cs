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
        List<Vector3> lowestDistanceChunkKeys = new List<Vector3>();
        List<int> lowestDistanceTriangleIndices = new List<int>();
        List<float> lowestDistances = new List<float>();

        foreach (Vector3 chunkKey in ChunkLoader.Instance.currentChunkKeys)
        {
            Chunk dictChunk = ChunkManager.Instance.chunkDictionary[chunkKey];
            List<float> chunkValidDistances = new List<float>();
            List<int> chunkValidTriangleIndices = new List<int>();

            for (int i = 0; i < dictChunk.vertices.Count; i += 3)
            {
                // get vertices for the given triangle.
                Vector3 v0 = dictChunk.vertices[i + 0];
                Vector3 v1 = dictChunk.vertices[i + 1];
                Vector3 v2 = dictChunk.vertices[i + 2];

                // lerp for each line of the triangle.
                float t0 = Vector3.Dot(target.transform.position - v0, v1 - v0) / Mathf.Pow(Vector3.Magnitude(v1 - v0), 2.0f);
                float t1 = Vector3.Dot(target.transform.position - v1, v2 - v1) / Mathf.Pow(Vector3.Magnitude(v2 - v1), 2.0f);
                float t2 = Vector3.Dot(target.transform.position - v2, v0 - v2) / Mathf.Pow(Vector3.Magnitude(v0 - v2), 2.0f);

                // check if the lerp falls on the line segment (between 0.0f and 1.0f).
                // if so, add to the lists of points and their corresponding distances that can be compared.
                bool addTrianglePointsToComparision = false;
                List<Vector3> validPointsOnTriangle = new List<Vector3>();
                if (t0 > 0.0f && t0 < 1.0f)
                    validPointsOnTriangle.Add(Vector3.Lerp(v0, v1, t0));
                if (t0 <= 0.0f)
                    validPointsOnTriangle.Add(v0);
                if (t0 >= 1.0f)
                    validPointsOnTriangle.Add(v1);
                if (t1 > 0.0f && t1 < 1.0f)
                    validPointsOnTriangle.Add(Vector3.Lerp(v1, v2, t1));
                if (t1 <= 0.0f)
                    validPointsOnTriangle.Add(v1);
                if (t1 >= 1.0f)
                    validPointsOnTriangle.Add(v2);
                if (t2 >= 0.0f && t2 < 1.0f)
                    validPointsOnTriangle.Add(Vector3.Lerp(v2, v0, t2));
                if (t2 <= 0.0f)
                    validPointsOnTriangle.Add(v2);
                if (t2 >= 1.0f)
                    validPointsOnTriangle.Add(v0);

                // check if any of the points of the triangle fall within 5.0f distance of the target.
                // if not, don't add it to the list. This DRASTICALLY reduces calculation times as most of the triangles in the mesh are discarded.
                foreach (Vector3 validPoint in validPointsOnTriangle)
                {
                    float distance = Vector3.Magnitude(target.transform.position - validPoint);
                    if (distance < maximumDistance)
                        addTrianglePointsToComparision = true;
                }

                if (addTrianglePointsToComparision)
                {
                    // run through all points in the triangle, swapping the lowest magnitude whenever a new "challenger" is lower that the current "champion"
                    float lowestDistance = Vector3.Magnitude(target.transform.position - validPointsOnTriangle[0]);
                    for (int j = 1; j < validPointsOnTriangle.Count; j++)
                    {
                        float pointDistance = Vector3.Magnitude(target.transform.position - validPointsOnTriangle[j]);
                        if (pointDistance < lowestDistance)
                        {
                            lowestDistance = pointDistance;
                        }
                    }
                    // put the lowest magnitude point for the triangle into a list, and its magnitude into another corresponding list.
                    chunkValidTriangleIndices.Add(i);
                    chunkValidDistances.Add(lowestDistance);
                }
            }

            for (int i = 0; i < colliderCount; i++)
            {
                if (chunkValidTriangleIndices.Count > 0)
                {
                    // put the lowest magnitude point for the chunk into a list, its magnitude into another corresponding list, and the chunk it belongs to into one final list.
                    float lowestDistanceInChunk = chunkValidDistances.Min();
                    int lowestDistanceInChunkIndex = chunkValidTriangleIndices[chunkValidDistances.IndexOf(lowestDistanceInChunk)];

                    lowestDistanceChunkKeys.Add(chunkKey);
                    lowestDistanceTriangleIndices.Add(lowestDistanceInChunkIndex);
                    lowestDistances.Add(lowestDistanceInChunk);

                    chunkValidDistances.RemoveAt(chunkValidDistances.IndexOf(lowestDistanceInChunk));
                    chunkValidTriangleIndices.RemoveAt(chunkValidTriangleIndices.IndexOf(lowestDistanceInChunkIndex));
                }
            }
        }

        List<float> championDistances = new List<float>();
        List<int> championTriangleIndices = new List<int>();
        List<Vector3> championChunkKeys = new List<Vector3>();
        for (int i = 0; i < colliderCount; i++)
        {
            if (lowestDistanceTriangleIndices.Count > 0)
            {
                // run the same process as before, but between the "champion" for every loaded chunk.
                float championDistance = lowestDistances.Min();
                championDistances.Add(championDistance);
                championTriangleIndices.Add(lowestDistanceTriangleIndices[lowestDistances.IndexOf(championDistance)]);
                championChunkKeys.Add(lowestDistanceChunkKeys[lowestDistances.IndexOf(championDistance)]);
                // redefine the path for the collider using the triangle with the closest point to the center of the target game object.
                Chunk championChunk = ChunkManager.Instance.chunkDictionary[championChunkKeys[i]];
                List<Vector2> colliderPath = new List<Vector2>();
                colliderPath.Add(championChunk.vertices[championTriangleIndices[i] + 0]);
                colliderPath.Add(championChunk.vertices[championTriangleIndices[i] + 1]);
                colliderPath.Add(championChunk.vertices[championTriangleIndices[i] + 2]);
                polygonCollider.SetPath(i, colliderPath.ToArray());

                lowestDistances.RemoveAt(lowestDistances.IndexOf(championDistance));
                lowestDistanceTriangleIndices.RemoveAt(lowestDistanceTriangleIndices.IndexOf(championTriangleIndices[i]));
                lowestDistanceChunkKeys.RemoveAt(lowestDistanceChunkKeys.IndexOf(championChunkKeys[i]));
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
