using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAt : MonoBehaviour
{
    private Camera _camera;
    
    private void Start()
    {
        _camera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.LookAt(2 * transform.position - _camera.transform.position);
    }
}
