using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    // Chunk meshes.
    public GameObject shadowObject;

    public List<RenderTexture> shadowMasks;
    public RenderTexture compositeMask;
    public RenderTexture finalCompositeMask;

    private Material material;

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
                RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 1);
                shadowMasks.Add(rt);
            }
        }

        compositeMask = new RenderTexture(Screen.width, Screen.height, 24);
        compositeMask.dimension = TextureDimension.Tex2DArray;
        compositeMask.volumeDepth = shadowMasks.Count;

        // Initialize material with proper shader.
        material = new Material(Shader.Find("Custom/CompositeShadows"));


        finalCompositeMask = new RenderTexture(Screen.width, Screen.height, 1);
    }

    public void GenerateShadowMasks()
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

    private void LateUpdate()
    {
        material.SetInt("_Depth", compositeMask.volumeDepth);
        Graphics.Blit(compositeMask, finalCompositeMask, material, 0, 0);
    }
}
