using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.SceneView;
using static UnityEngine.GraphicsBuffer;

public class PlayerCamera : MonoBehaviour
{
    public Transform Target;
    public Transform Player;

    public Transform CameraPos;
    public Transform CameraFocusPos;

    public Vector3 _orignalCameraPos;
    public Vector3 _orignalCameraFocusPos;

    private void Awake()
    {
        _orignalCameraPos= CameraPos.localPosition;
        _orignalCameraFocusPos = CameraFocusPos.localPosition;
    }

    void Update()
    {
        var dir = (CameraPos.position - Player.position).normalized;
        var ray = new Ray(Player.position, dir);

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

        CameraMovement();
    }

    private void CameraMovement()
    {
        var cameraSpeed = 10f;

        if (Move.isCameraFocused)
        {
            transform.position = Vector3.Lerp(transform.position, CameraFocusPos.position, Time.deltaTime * cameraSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, CameraFocusPos.rotation, Time.deltaTime * cameraSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, CameraPos.position, Time.deltaTime * cameraSpeed);

            var relativePosition = Target.position - transform.position;
            var targetRotation = Quaternion.LookRotation(relativePosition);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
        }
    }
}
