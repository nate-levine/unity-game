using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCamera : MonoBehaviour
{
    public Camera target;
    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        cam.fieldOfView = target.fieldOfView;
        cam.orthographic = target.orthographic;
        if (cam.orthographic)
            cam.orthographicSize = target.orthographicSize;
        cam.transform.position = target.transform.position;
        cam.transform .rotation = target.transform.rotation;
    }
}
