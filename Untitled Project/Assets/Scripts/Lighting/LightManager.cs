using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightManager : MonoBehaviour
{
    // Singleton to reference.
    public static LightManager Instance { get; private set; }
    // What object will cast shadows.
    public GameObject targetObject;

    // List of lights.
    [SerializeField] List<GameObject> lights;
    // Texture 2d array, which stores a seperate light mask on each layer.
    [SerializeField] RenderTexture lightingMaskTextureArray;
    // A composite of all the layers in the lighting mask texture 2d array.
    [SerializeField] RenderTexture lightingMaskComposite;
    // Material holds the shader to composite all the texture 2d array layers together.
    private Material compositeMaterial;

    void Awake()
    {
        // Initialize singleton.
        if (Instance == null)
            Instance = this;
    }

    public void Start()
    {
        // Initialize material with proper shader.
        compositeMaterial = new Material(Shader.Find("Custom/CompositeLightMasks"));
        // Initialize composite lighting render texture.
        lightingMaskComposite = new RenderTexture(Screen.width, Screen.height, 0);
    }

    public void GenerateShadowMasks()
    {
        /* Get mesh data to pass the compute shader.
         * The reason it is done in the manager is so that the mesh data processing doesn't need to
         * be done per light.
         */
        Mesh mesh = new Mesh();
        if (targetObject.GetComponent<ChunkMeshGenerator>())
        {
            mesh = targetObject.GetComponent<ChunkMeshGenerator>().GetMesh();
        }
        // Pass mesh data into each light with a shadow caster, and generate their shadow meshes.
        foreach (GameObject light in lights)
        {
            if (light.gameObject.GetComponent<ShadowRenderer>())
            {
                light.gameObject.GetComponent<ShadowRenderer>().GenerateShadowMask(mesh.vertices, mesh.triangles, mesh.triangles.Length, targetObject.transform.localToWorldMatrix);
            }
        }
    }

    // Run custom lighting pipeline
    public void DoLighting()
    {
        // Initialize lighting texture 2d array each frame so its depth can be adjusted per frame if needed.
        lightingMaskTextureArray = new RenderTexture(Screen.width, Screen.height, 0);
        lightingMaskTextureArray.dimension = TextureDimension.Tex2DArray;
        // Set composite light render texture depth as the number of lights, one layer for each light map.
        lightingMaskTextureArray.volumeDepth = lights.Count;

        // Run lighting pipeline per each light.
        foreach (GameObject light in lights)
        {
            if (light.gameObject.GetComponent<CustomLight>())
            {
                // If the light has a shadow caster, create a shadow mask.
                if (light.gameObject.GetComponent<ShadowRenderer>())
                {

                    light.gameObject.GetComponent<ShadowRenderer>().DrawShadow();
                }
                // Create a light map.
                light.gameObject.GetComponent<PointLightRenderer>().DrawPointLight();
                // Composite the light map and shadow mask.
                light.gameObject.GetComponent<CustomLight>().CompositeLighting();
            }
        }

        // Set the depth of the texture 2d array in the shader.
        compositeMaterial.SetInt("_Depth", lightingMaskTextureArray.volumeDepth);
        // Composite the texture 2d array into a composite render texture via blitting.
        Graphics.Blit(lightingMaskTextureArray, lightingMaskComposite, compositeMaterial, 0, 0);
        // Composite the lighting mask composite with the main camera render texture.
        Camera.main.GetComponent<ApplyLightingShader>().ApplyLighting();

        // Release the lighting mask texture 2d array.
        lightingMaskTextureArray.Release();
    }

    // Adds light to the light manager by index.
    public void AddLight(GameObject light)
    {
        if (light.GetComponent<CustomLight>())
        {
            light.GetComponent<CustomLight>().SetLightIndex(lights.Count);
            lights.Add(light);

        }
    }

    // Getters and setters.
    public RenderTexture GetLightingMaskTextureArray()
    {
        return lightingMaskTextureArray;
    }
    public RenderTexture GetLightingMaskComposite()
    {
        return lightingMaskComposite;
    }
}
