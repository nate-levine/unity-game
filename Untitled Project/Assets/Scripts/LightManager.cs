using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    // Chunk meshes.
    public GameObject shadowObject;

    public List<RenderTexture> shadowMasks;

    private Mesh mesh;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<ShadowRenderer>())
            {
                child.GetComponent<ShadowRenderer>().shadowMaskIndex = shadowMasks.Count;
                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
                shadowMasks.Add(rt);
            }
        }
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
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<ShadowRenderer>())
            {
                child.gameObject.GetComponent<ShadowRenderer>().GenerateShadows(mesh.vertices, mesh.triangles, mesh.triangles.Length, shadowObject.transform.localToWorldMatrix);
            }
        }
    }
}
