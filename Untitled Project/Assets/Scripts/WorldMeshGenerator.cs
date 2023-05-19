using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMeshGenerator : MonoBehaviour
{
    public static WorldMeshGenerator Instance { get; private set; }

    public float innerRadius;
    public float outerRadius;
    public float noiseAmplitude;
    public float noiseFrequency;
    public float scalarUV;
    public List<Vector3> vertices = new List<Vector3>();
    public List<int>[] triangles = new List<int>[2];
    public List<Vector2> UVs = new List<Vector2>();
    public List<Color> colors = new List<Color>();

    private int m_verticesIndex = 0;
    private float[] m_perlinNoise = new float[361];

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = new List<int>();
        }
        for (int i = 0; i < 360; i++)
            GeneratePerlinNoise(i * Mathf.PI / 720.0f, i);
        for (int i = 0; i < 360; i++)
            CreateTile(i * Mathf.PI / 180.0f, i);
    }

    public void GeneratePerlinNoise(float theta, int index)
    {
        // sample a 1d circle or perlin noise from a 2D plane. rather than sampling a straigh line from a 2D plane, sampling from a circle ensures that the noise loops.
        float noiseValue = Mathf.PerlinNoise(Mathf.Cos(theta) * noiseFrequency, Mathf.Sin(theta) * noiseFrequency);
        m_perlinNoise[index] = noiseValue * noiseAmplitude;
        if (index == 0)
            m_perlinNoise[360] = noiseValue * noiseAmplitude;
    }

    public void CreateTile(float theta, int index)
    {
        List<Vector3> initialVertices = new List<Vector3> {
            new Vector3(Mathf.Cos(theta - (Mathf.PI/180.0f)), Mathf.Sin(theta - (Mathf.PI / 180.0f)), 0.0f) * innerRadius,
            new Vector3(Mathf.Cos(theta               ), Mathf.Sin(theta                 ), 0.0f) * innerRadius,
            new Vector3(Mathf.Cos(theta               ), Mathf.Sin(theta                 ), 0.0f) * (outerRadius + m_perlinNoise[index + 1]),
            new Vector3(Mathf.Cos(theta - (Mathf.PI/180.0f)), Mathf.Sin(theta - (Mathf.PI / 180.0f)), 0.0f) * (outerRadius + m_perlinNoise[index]),
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(Mathf.Cos(theta               ), Mathf.Sin(theta                 ), 0.0f) * innerRadius,
            new Vector3(Mathf.Cos(theta - (Mathf.PI/180.0f)), Mathf.Sin(theta - (Mathf.PI / 180.0f)), 0.0f) * innerRadius,
        };
        foreach (Vector3 vertex in initialVertices)
        {
            vertices.Add(vertex);
        }
        triangles[0].Add(m_verticesIndex + 0);
        triangles[0].Add(m_verticesIndex + 1);
        triangles[0].Add(m_verticesIndex + 2);
        triangles[0].Add(m_verticesIndex + 0);
        triangles[0].Add(m_verticesIndex + 2);
        triangles[0].Add(m_verticesIndex + 3);
        m_verticesIndex += 4;
        for (int i = 0; i < 3; i++)
        {
            triangles[1].Add(m_verticesIndex);
            m_verticesIndex++;
        }
        List<Vector2> initialUVs = new List<Vector2> {
            new Vector2(0.5f + (Mathf.Cos(theta - Mathf.PI/180.0f) * scalarUV), 0.5f + (Mathf.Sin(theta - Mathf.PI/180.0f) * scalarUV)),
            new Vector2(0.5f + (Mathf.Cos(theta) * scalarUV), 0.5f + (Mathf.Sin(theta) * scalarUV)),
            new Vector2(0.5f + (Mathf.Cos(theta) * (scalarUV + 1.0f)), 0.5f + (Mathf.Sin(theta) * (scalarUV + 1.0f))),
            new Vector2(0.5f + (Mathf.Cos(theta - Mathf.PI/180.0f) * (scalarUV + 1.0f)), 0.5f + (Mathf.Sin(theta - Mathf.PI/180.0f) * (scalarUV + 1.0f))),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f + (Mathf.Cos(theta) * scalarUV), 0.5f + (Mathf.Sin(theta) * scalarUV)),
            new Vector2(0.5f + (Mathf.Cos(theta - Mathf.PI/180.0f) * scalarUV), 0.5f + (Mathf.Sin(theta - Mathf.PI/180.0f) * scalarUV)),
        };
        foreach (Vector2 UV in initialUVs)
        {
            UVs.Add(UV);
        }
        List<Color> initialColors = new List<Color> {
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
            new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f),
        };
        foreach (Color color in initialColors)
        {
            colors.Add(color);
        }
    }
}
