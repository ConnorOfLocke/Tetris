using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPropertyController_Float : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer meshRenderer = null;

    public string customShaderPropertyName;
    public float customShaderPropertyValue;

    private float lastCustomShaderPropertyValue;

    private void Awake()
    {
        meshRenderer.material.SetFloat(customShaderPropertyName, customShaderPropertyValue);
        lastCustomShaderPropertyValue = customShaderPropertyValue;
    }

    void Update()
    {
        if (customShaderPropertyValue != lastCustomShaderPropertyValue)
        {
            meshRenderer.material.SetFloat(customShaderPropertyName, customShaderPropertyValue);
        }

    }
}
