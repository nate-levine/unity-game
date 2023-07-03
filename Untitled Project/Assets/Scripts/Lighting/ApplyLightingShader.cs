using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ApplyLightingShader : MonoBehaviour
{
    // Light application material.
    private Material applyLightingMaterial;

    private void Start()
    {
        // Initialize material
        if (applyLightingMaterial == null)
        {
            // Assign shader to material.
            applyLightingMaterial = new Material(Shader.Find("Custom/ApplyLighting"));
        }
    }

    // Set the composite light mask as the render texture to composite the main camera target texture with.
    public void ApplyLighting()
    {
        applyLightingMaterial.SetTexture("_LightingTex", LightManager.Instance.GetComponent<LightManager>().GetLightingMaskComposite());
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Run lighting calculations the frame before they are applied.
        LightManager.Instance.GetComponent<LightManager>().DoLighting();

        // Blit the composite to the main camera.
        Graphics.Blit(source, destination, applyLightingMaterial);
    }
}
