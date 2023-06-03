using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeShader : MonoBehaviour
{
    public Camera mainCam;
    public Camera lightingCam;
    public Material mat;
    public RenderTexture mainRenderTexture;

    void Start()
    {
        mainCam.targetTexture = new RenderTexture(Screen.width, Screen.height, 8);
    }

    void Update()
    {
        if (mat == null)
        {
            // Assign shader to material.
            mat = new Material(Shader.Find("Hidden/Composite"));
        }

        mainCam.targetTexture = mainRenderTexture;

        mat.SetTexture("_MainTex", mainRenderTexture);
        mat.SetTexture("_LightingTex", lightingCam.GetComponent<Test>().lightingRenderTexture);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Make a temporary texture to hold the lighting information
        var temporaryRenderTexture0 = RenderTexture.GetTemporary(Screen.width, Screen.height);
        var temporaryRenderTexture1 = RenderTexture.GetTemporary(Screen.width, Screen.height);
        // Render source to screen with shader.
        // blur x-axis
        Graphics.Blit(source, temporaryRenderTexture0, mat, 0);
        // blur y-axis
        Graphics.Blit(source, temporaryRenderTexture1, mat, 1);
        // composite main and lighting
        mat.SetTexture("_LightingTexX", temporaryRenderTexture0);
        mat.SetTexture("_LightingTexY", temporaryRenderTexture1);
        Graphics.Blit(source, destination, mat, 2);
    }
}
