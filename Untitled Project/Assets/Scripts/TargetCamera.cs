using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
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
        cam.orthographic = target.orthographic;
        cam.orthographicSize = target.orthographicSize;
        cam.fieldOfView = target.fieldOfView;
        cam.transform.position = target.transform.position;
        cam.transform .rotation = target.transform.rotation;
    }
}
