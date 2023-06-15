using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Member;

public class CompositeShadowMasks : MonoBehaviour
{
    public Camera cam;
    public Material mat;
    public RenderTexture compositeShadowMask;

    private CommandBuffer commandBuffer;
    private int tempID;

    void OnEnable()
    {
        compositeShadowMask = new RenderTexture(Screen.width, Screen.height, 24);

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Camera>())
                cam = child.GetComponent<Camera>();
        }

        // Assign shader to material.
        mat = new Material(Shader.Find("Custom/CompositeShadowMasks"));
    }

    public void Composite()
    {

        commandBuffer = new CommandBuffer();
        tempID = Shader.PropertyToID("_Temp");

        commandBuffer.GetTemporaryRT(tempID, Screen.width, Screen.height, 8);
        commandBuffer.SetRenderTarget(tempID);
        commandBuffer.ClearRenderTarget(true, true, Color.black);

        /*foreach (RenderTexture shadowMaskRenderTexture in shadowMaskRenderTextures)
        {
            commandBuffer.Blit(shadowMaskRenderTexture, compositeShadowMask, mat);
        }*/

        commandBuffer.ReleaseTemporaryRT(tempID);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
}
