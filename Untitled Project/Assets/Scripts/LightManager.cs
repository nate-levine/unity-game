using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void GenerateShadows()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<ShadowRenderer>())
            {
                child.gameObject.GetComponent<ShadowRenderer>().GenerateShadows();
            }
        }
    }
}
