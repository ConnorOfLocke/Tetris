using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningObject : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed = 100.0f;
    
    [SerializeField]
    private Vector3 spinAxis = Vector3.forward;

    private RectTransform rectTransform = null;

    private void Start()
    {
        if (GetComponent<RectTransform>() != null)
            rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.rotation = Quaternion.AngleAxis(spinSpeed * Time.deltaTime, spinAxis) * rectTransform.rotation;
        }
        else
        {
            transform.rotation = Quaternion.AngleAxis(spinSpeed * Time.deltaTime, spinAxis) * transform.rotation;
        }
    }
}
