using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Light : MonoBehaviour
{
    // Helps the Light Manager keep track of what shadow mask render texture belongs to which light.
    public int lightIndex;

    public RenderTexture pointLightRenderTexture;
    public RenderTexture shadowMaskRenderTexture;
    public RenderTexture lightShadowCompositeRenderTexture;

    private Material material;
    //
    private Camera cam;

    void OnEnable()
    {
        // Initialize material.
        material = new Material(Shader.Find("Custom/LightShadowComposite"));
    }
    void Start()
    {
        LightManager.Instance.lights.Add(gameObject);

        pointLightRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        shadowMaskRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        lightShadowCompositeRenderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

        if (transform.GetChild(0).GetComponent<Camera>())
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.ARGB32);
        }
    }

    private void LateUpdate()
    {
        material.SetTexture("_LightTex", pointLightRenderTexture);
        material.SetTexture("_ShadowTex", shadowMaskRenderTexture);

        Graphics.Blit(null, lightShadowCompositeRenderTexture, material);

        // Draw render texture to shadow mask array at that index's unique depth.
        Graphics.SetRenderTarget(LightManager.Instance.GetComponent<LightManager>().shadowMaskArray, 0, 0, lightIndex);
        Graphics.Blit(lightShadowCompositeRenderTexture, LightManager.Instance.GetComponent<LightManager>().shadowMaskArray, 0, lightIndex);
    }
}
