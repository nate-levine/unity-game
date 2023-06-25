using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    public List<GameObject> lights;
    // World mesh.
    public GameObject shadowObject;

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
        // Initialize material with proper shader.
        material = new Material(Shader.Find("Custom/CompositeShadows"));

        shadowMaskComposite = new RenderTexture(Screen.width, Screen.height, 0);
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

    public void Lighting()
    {
        // Set composite light render texture depth as the number of lights, one layer for each light.
        shadowMaskArray = new RenderTexture(Screen.width, Screen.height, 0);
        shadowMaskArray.dimension = TextureDimension.Tex2DArray;
        shadowMaskArray.volumeDepth = lights.Count;

        foreach (GameObject light in lights)
        {
            if (light.gameObject.GetComponent<CustomLight>())
            {
                if (light.gameObject.GetComponent<ShadowRenderer>())
                {

                    light.gameObject.GetComponent<ShadowRenderer>().DrawShadow();
                }
                light.gameObject.GetComponent<PointLightRenderer>().DrawPointLight();
                light.gameObject.GetComponent<CustomLight>().DrawLight();
            }
        }
        // Composite the render texture array.
        material.SetInt("_Depth", shadowMaskArray.volumeDepth);
        Graphics.Blit(shadowMaskArray, shadowMaskComposite, material, 0, 0);

        Camera.main.GetComponent<ApplyLightingShader>().ApplyLighting();

        shadowMaskArray.Release();
    }

    public void AddLight(GameObject light)
    {
        if (light.GetComponent<CustomLight>())
        {
            light.GetComponent<CustomLight>().lightIndex = lights.Count;
            lights.Add(light);

        }
    }
}
