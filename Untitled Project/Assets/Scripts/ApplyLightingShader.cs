using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ApplyLightingShader : MonoBehaviour
{
    public Camera mainCam;
    public Material mat;

    void Update()
    {
        if (mat == null)
        {
            // Assign shader to material.
            mat = new Material(Shader.Find("Custom/ApplyLighting"));
        }

        mat.SetTexture("_MainTex", GetComponent<Camera>().targetTexture);
        mat.SetTexture("_LightingTex", LightManager.Instance.GetComponent<LightManager>().shadowMaskComposite);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
