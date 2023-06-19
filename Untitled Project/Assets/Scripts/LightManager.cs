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
    public List<GameObject> lights;

    public int shadowMaskCount;
    public RenderTexture shadowMaskArray;
    public RenderTexture shadowMaskComposite;

    private Material material;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void Start()
    {
        shadowMaskCount = 0;
        foreach (GameObject light in lights)
        {
            if (light.GetComponent<ShadowRenderer>())
            {
                light.GetComponent<ShadowRenderer>().shadowMaskIndex = shadowMaskCount;
                shadowMaskCount++;
            }
        }

        shadowMaskArray = new RenderTexture(Screen.width, Screen.height, 24);
        shadowMaskArray.dimension = TextureDimension.Tex2DArray;
        shadowMaskArray.volumeDepth = shadowMaskCount;

        // Initialize material with proper shader.
        material = new Material(Shader.Find("Custom/CompositeShadows"));

        shadowMaskComposite = new RenderTexture(Screen.width, Screen.height, 1);
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
        foreach (GameObject light in lights)
        {
            if (light.gameObject.GetComponent<ShadowRenderer>())
            {
                light.gameObject.GetComponent<ShadowRenderer>().GenerateShadows(mesh.vertices, mesh.triangles, mesh.triangles.Length, shadowObject.transform.localToWorldMatrix);
            }
        }
    }

    private void LateUpdate()
    {
        // Composite the render texture array.
        material.SetInt("_Depth", shadowMaskArray.volumeDepth);
        Graphics.Blit(shadowMaskArray, shadowMaskComposite, material, 0, 0);
        Graphics.SetRenderTarget(Camera.main.targetTexture);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(shadowMaskComposite, destination);
    }
}
