using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenDepthNormal : MonoBehaviour
{
    public Camera cam;
    public Material mat;

    void Start()
    {
        
    }

    void Update()
    {
        if (cam == null)
        {
            cam = this.GetComponent<Camera>();
            cam.depthTextureMode = DepthTextureMode.DepthNormals;
        }
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/ScreenDepthNormal"));
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Render source to screen with shader.
        Graphics.Blit(source, destination, mat);
    }
}
