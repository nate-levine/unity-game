using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public Vector3 position;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int>[] triangles = new List<int>[2];
    public List<Vector2> UVs = new List<Vector2>();

    public Chunk(Vector3 position)
    {
        this.position = position;
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = new List<int>();
        }
    }
}