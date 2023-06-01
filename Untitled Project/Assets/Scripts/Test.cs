using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class Test : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    public RenderTexture lightingRenderTexture;

    void Start()
    {
        lightingRenderTexture = new RenderTexture(Screen.width, Screen.height, 8);
    }
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

        cam.targetTexture = lightingRenderTexture;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Render source to screen with shader.
        Graphics.Blit(source, destination, mat);
    }
}
