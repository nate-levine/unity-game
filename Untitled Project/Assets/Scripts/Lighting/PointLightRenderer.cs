using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointLightRenderer : MonoBehaviour
{
    // Light properties adjustable in the Unity inspector.
    public Color lightInnerColor;
    public Color lightOuterColor;
    public float lightInnerRadius;
    public float lightOuterRadius;

    // Point light material.
    private Material pointLightMaterial;

    private void OnEnable()
    {
        // Initialize material and apply the point light shader.
        pointLightMaterial = new Material(Shader.Find("Custom/PointLight"));
    }

    public void DrawPointLight()
    {
        // Set the point light properties.
        // Light position.
        pointLightMaterial.SetVector("_LightPos", transform.position);
        // Light inner radius.
        pointLightMaterial.SetFloat("_LightInnerRadius", lightInnerRadius);
        // Light outer radius.
        pointLightMaterial.SetFloat("_LightOuterRadius", lightOuterRadius);
        // light inner color.
        pointLightMaterial.SetVector("_LightInnerColor", new Vector3(lightInnerColor.r, lightInnerColor.g, lightInnerColor.b));
        // light outer color.
        pointLightMaterial.SetVector("_LightOuterColor", new Vector3(lightOuterColor.r, lightOuterColor.g, lightOuterColor.b));

        // A series of transforms to find where the render texture is in world space.
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

        // Set the camera corner positions in world space in the shader.
        pointLightMaterial.SetVector("_BottomLeft", cameraBottomLeft);
        pointLightMaterial.SetVector("_TopRight", cameraTopRight);

        // Draw the light in the shader and store it in a render texture.
        Graphics.Blit(null, GetComponent<CustomLight>().GetPointLightRenderTexture(), pointLightMaterial);
    }
}
