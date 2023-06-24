using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;
    void FixedUpdate()
    {
        transform.position = target.transform.position + new Vector3(0.0f, 0.0f, -16.0f);
        m_OrientCamera();
    }

    private void m_OrientCamera()
    {
        transform.LookAt(target.transform.position, target.transform.up);
    }
}
