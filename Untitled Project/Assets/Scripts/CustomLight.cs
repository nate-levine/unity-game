using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CustomLight : MonoBehaviour
{
    // Helps the Light Manager keep track of what shadow mask render texture belongs to which light.
    public int lightIndex;

    public RenderTexture pointLightRenderTexture;
    public RenderTexture shadowMaskRenderTexture;
    public RenderTexture blurredShadowMaskRenderTexture;
    public RenderTexture lightShadowCompositeRenderTexture;

    private Material material;
    private Material blurMaterial;
    //
    private Camera cam;

    void OnEnable()
    {
        // Initialize material.
        material = new Material(Shader.Find("Custom/LightShadowComposite"));
        blurMaterial = new Material(Shader.Find("Custom/GaussianBlur"));
    }
    void Start()
    {
        if (LightManager.Instance != null)
        {
            LightManager.Instance.lights.Add(gameObject);
        }

        pointLightRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        shadowMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        blurredShadowMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        lightShadowCompositeRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

        if (transform.GetChild(0).GetComponent<Camera>())
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        }
    }

    public void DrawLight()
    {
        RenderTexture temporaryRenderTexture0 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(shadowMaskRenderTexture, temporaryRenderTexture0, blurMaterial, 0);
        Graphics.Blit(temporaryRenderTexture0, blurredShadowMaskRenderTexture, blurMaterial, 1);
        RenderTexture.ReleaseTemporary(temporaryRenderTexture0);

        material.SetTexture("_LightTex", pointLightRenderTexture);
        if (GetComponent<ShadowRenderer>())
        {
            if (GetComponent<ShadowRenderer>().initialized)
            {
                material.EnableKeyword("SHADOW_MASK_IS_SET");
                material.SetTexture("_ShadowTex", blurredShadowMaskRenderTexture);
            }
        }

        Graphics.Blit(null, lightShadowCompositeRenderTexture, material);

        // Draw render texture to shadow mask array at that index's unique depth.
        Graphics.SetRenderTarget(LightManager.Instance.GetComponent<LightManager>().shadowMaskArray, 0, 0, lightIndex);
        Graphics.Blit(lightShadowCompositeRenderTexture, LightManager.Instance.GetComponent<LightManager>().shadowMaskArray, 0, lightIndex);
    }
}
