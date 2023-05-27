using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3 position;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int>[] triangles = new List<int>[2];
    public List<Vector2> UVs = new List<Vector2>();
    /* UV2s holds the mesh edge data.
     * If the "x" component of the Vector2 is 0.0f, the vertex is not an edge.
     * If the "x" component of the Vector2 is 1.0f, the vertex is an edge.
     * The "y" component of the Vector2 is unused right now, and is set to a default of 0.0f.
     */
    public List<Vector2> UV2s = new List<Vector2>();
    public List<Color> colors = new List<Color>();

    public Chunk(Vector3 position)
    {
        this.position = position;
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = new List<int>();
        }
    }
}