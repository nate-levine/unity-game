using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Light : MonoBehaviour
{
    // Add light to light manager.
    void Start()
    {
        LightManager.Instance.lights.Add(gameObject);
    }
}
