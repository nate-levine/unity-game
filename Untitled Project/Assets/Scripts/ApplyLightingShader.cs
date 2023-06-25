using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ApplyLightingShader : MonoBehaviour
{
    public Material mat;

    private void Start()
    {
        if (mat == null)
        {
            // Assign shader to material.
            mat = new Material(Shader.Find("Custom/ApplyLighting"));
        }
    }

    public void ApplyLighting()
    {
        mat.SetTexture("_LightingTex", LightManager.Instance.GetComponent<LightManager>().shadowMaskComposite);
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        LightManager.Instance.GetComponent<LightManager>().Lighting();

        Graphics.Blit(source, destination, mat);
    }
}
