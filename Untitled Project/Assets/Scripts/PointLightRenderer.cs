using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PointLightRenderer : MonoBehaviour
{
    public Color lightInnerColor;
    public Color lightOuterColor;
    public float lightInnerRadius;
    public float lightOuterRadius;

    private Material material;

    private void OnEnable()
    {
        // Initialize material.
        material = new Material(Shader.Find("Custom/PointLight"));
    }

    private void LateUpdate()
    {
        material.SetVector("_LightPos", transform.position);
        material.SetFloat("_LightInnerRadius", lightInnerRadius);
        material.SetFloat("_LightOuterRadius", lightOuterRadius);
        material.SetVector("_LightInnerColor", new Vector3(lightInnerColor.r, lightInnerColor.g, lightInnerColor.b));
        material.SetVector("_LightOuterColor", new Vector3(lightOuterColor.r, lightOuterColor.g, lightOuterColor.b));

        material.SetVector("_TopRight", Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane)));
        material.SetVector("_BottomLeft", Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)));

        Graphics.Blit(null, GetComponent<Light>().pointLightRenderTexture, material);
    }
}
