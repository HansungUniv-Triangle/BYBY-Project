using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform PlayerPos;

    public Transform CameraPos;
    public Transform CameraFocusPos;

    public Vector3 _orignalCameraPos;
    public Vector3 _orignalCameraFocusPos;

    public float a;
    public float b;

    private void Awake()
    {
        _orignalCameraPos= CameraPos.localPosition;
        _orignalCameraFocusPos = CameraFocusPos.localPosition;
    }

    void Update()
    {
        var dir = (transform.position - PlayerPos.position).normalized;
        var ray = new Ray(PlayerPos.position, dir);

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.white);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, (int)Type.Layer.World))
        {
            CameraPos.position = hit.point;
            CameraFocusPos.position = hit.point;
        }
        else
        {
            if (CameraPos.localPosition != _orignalCameraPos)
                CameraPos.localPosition = _orignalCameraPos;

            if (CameraFocusPos.localPosition != _orignalCameraFocusPos)
                CameraFocusPos.localPosition = _orignalCameraFocusPos;
        }

        /*
        // 조준할 때의 카메라 위치도 변경 그러나 버그 존재
        a = Vector3.Distance(PlayerPos.position, CameraFocusPos.position);
        b = Vector3.Distance(PlayerPos.position, hit.point);

        if (a > b)
            CameraFocusPos.position = hit.point;
        else
            CameraFocusPos.localPosition = _orignalCameraFocusPos;
        */
    }
}
