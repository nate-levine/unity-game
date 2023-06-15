using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class ShadowRenderTexture : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    public RenderTexture shadowMaskRenderTexture;

    private CommandBuffer commandBuffer;

    void Start()
    {
        // Create a new render texture for the camera to output too.
        shadowMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 8);
    }

    void Update()
    {
        if (cam == null)
        {
            foreach (Transform child in transform)
            {
                cam = child.gameObject.GetComponent<Camera>();
            }
        }

        if (mat == null)
        {
            // Assign shader to material.
            mat = new Material(Shader.Find("Hidden/DepthNormals"));
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<ShadowMeshGenerator>())
            {
                //cam.targetTexture = child.gameObject.GetComponent<ShadowMeshGenerator>().GetRenderTexture();
            }
        }
/*
        // Set shadow mesh as the main render texture.
        mat.SetTexture("_MainTex", cam.targetTexture);
        // Shader processes the camera render texture to output a new depth-normal render texture.
        Graphics.SetRenderTarget(depthNormalsRenderTexture);
        Graphics.Blit(cam.targetTexture, depthNormalsRenderTexture, mat);
        Graphics.SetRenderTarget(null);*/
    }
}
