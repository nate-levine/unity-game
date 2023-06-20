using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointLightRenderer : MonoBehaviour
{
    public Color lightColor;
    public float lightRadius;
    public RenderTexture renderTexture;

    private Material material;

    private void OnEnable()
    {
        // Initialize render texture.
        renderTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.ARGB32);

        // Initialize material.
        material = new Material(Shader.Find("Custom/PointLight"));
    }

    private void LateUpdate()
    {
        material.SetVector("_LightPos", transform.position);
        material.SetFloat("_LightRadius", lightRadius);
        material.SetVector("_LightColor", new Vector3(lightColor.r, lightColor.g, lightColor.b));

        material.SetVector("_TopRight", Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane)));
        material.SetVector("_BottomLeft", Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)));

        Graphics.Blit(null, renderTexture, material);
    }
}
