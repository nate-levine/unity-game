using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Test : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    void Update()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.DepthNormals;
        }

        if (mat == null)
        {
            // Assign shader to material.
            mat = new Material(Shader.Find("Hidden/Test"));
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Render source to screen with shader.
        Graphics.Blit(source, destination, mat);
    }
}
