using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLight : MonoBehaviour
{
    // Render texture to store the light map.
    [SerializeField] RenderTexture pointLightRenderTexture;
    // Render texture to store the shadow mask.
    [SerializeField] RenderTexture shadowMaskRenderTexture;
    // Render texture to store the composite light map.
    [SerializeField] RenderTexture lightShadowCompositeRenderTexture;

    // Material to composite the light map and shadow mask.
    private Material compositeMaterial;
    // Render texture camera.
    private Camera cam;
    // Assists the light manager in keeping track of what shadow mask render texture belongs to which light.
    private int lightIndex;

    void OnEnable()
    {
        // Initialize material.
        compositeMaterial = new Material(Shader.Find("Custom/LightShadowComposite"));
    }
    void Start()
    {
        // If the light manager in initialized, add the light to the light manager's list.
        if (LightManager.Instance != null)
        {
            LightManager.Instance.AddLight(gameObject);
        }
        // Initialize the render textures
        pointLightRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        shadowMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        lightShadowCompositeRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

        // Set the camera and initialize its render texture.
        if (transform.GetChild(0).GetComponent<Camera>())
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        }
    }

    public void CompositeLighting()
    {
        // Set the lighting texture in the shader.
        compositeMaterial.SetTexture("_LightTex", pointLightRenderTexture);
        // If a shadow mask exists, enable and set the texture in the shader.
        if (GetComponent<ShadowRenderer>())
        {
            if (GetComponent<ShadowRenderer>().initialized)
            {
                compositeMaterial.EnableKeyword("SHADOW_MASK_IS_SET");
                compositeMaterial.SetTexture("_ShadowTex", shadowMaskRenderTexture);
            }
        }
        // Composite the light map and the shadow mask.
        Graphics.Blit(null, lightShadowCompositeRenderTexture, compositeMaterial);

        // Set the render target to the texture 2d array's depth at the light's light index.
        Graphics.SetRenderTarget(LightManager.Instance.GetComponent<LightManager>().GetLightingMaskTextureArray(), 0, 0, lightIndex);
        // Blit the lighting map to that render target.
        Graphics.Blit(lightShadowCompositeRenderTexture, LightManager.Instance.GetComponent<LightManager>().GetLightingMaskTextureArray(), 0, lightIndex);
    }

    // Getters and setters.
    public int GetLightIndex()
    {
        return lightIndex;
    }
    public void SetLightIndex(int index)
    {
        lightIndex = index;
    }

    public RenderTexture GetPointLightRenderTexture()
    {
        return pointLightRenderTexture;
    }
    public RenderTexture GetShadowMaskRenderTexture()
    {
        return shadowMaskRenderTexture;
    }
}
