using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotateBasedOnCamera : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Transform _camera;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        _rectTransform.rotation = Quaternion.Euler(0, 0, -_camera.eulerAngles.z);
    }
}
