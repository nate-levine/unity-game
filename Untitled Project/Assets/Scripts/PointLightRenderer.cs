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

    public void DrawPointLight()
    {
        Debug.Log("PointLightRenderer: " + Time.time);
        material.SetVector("_LightPos", transform.position);
        material.SetFloat("_LightInnerRadius", lightInnerRadius);
        material.SetFloat("_LightOuterRadius", lightOuterRadius);
        material.SetVector("_LightInnerColor", new Vector3(lightInnerColor.r, lightInnerColor.g, lightInnerColor.b));
        material.SetVector("_LightOuterColor", new Vector3(lightOuterColor.r, lightOuterColor.g, lightOuterColor.b));

        // Camera corners in camera viewport space.
        Vector3 cameraBottomLeftVP = new Vector3(0, 0, Camera.main.nearClipPlane);
        Vector3 cameraTopRightVP = new Vector3(1, 1, Camera.main.nearClipPlane);
        // Camera corners from viewport space to world space.
        Vector3 cameraBottomLeftWS = Camera.main.ViewportToWorldPoint(cameraBottomLeftVP);
        Vector3 cameraTopRightWS = Camera.main.ViewportToWorldPoint(cameraTopRightVP);
        // Camera corners from world space to local space with transforms.
        Vector3 cameraBottomLeftLS = Camera.main.transform.InverseTransformPoint(cameraBottomLeftWS);
        Vector3 cameraTopRightLS = Camera.main.transform.InverseTransformPoint(cameraTopRightWS);
        // Camera corners from local space to world space but only with translation.
        Vector3 cameraBottomLeft = Camera.main.transform.position + cameraBottomLeftLS;
        Vector3 cameraTopRight = Camera.main.transform.position + cameraTopRightLS;

        material.SetVector("_BottomLeft", cameraBottomLeft);
        material.SetVector("_TopRight", cameraTopRight);

        Graphics.Blit(null, GetComponent<Light>().pointLightRenderTexture, material);
    }
}
