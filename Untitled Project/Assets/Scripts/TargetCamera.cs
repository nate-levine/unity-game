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
        cam.transform.position = target.transform.position;
        cam.transform .rotation = target.transform.rotation;
    }
}
