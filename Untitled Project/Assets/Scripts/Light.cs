using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Light : MonoBehaviour
{
    public Camera cam;
    public RenderTexture shadowMask;
    void Start()
    {
        cam = transform.GetChild(0).GetComponent<Camera>();
        shadowMask = new RenderTexture(Screen.width, Screen.height, 24);
    }
}
