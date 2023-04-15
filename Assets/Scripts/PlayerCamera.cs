using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCamera : MonoBehaviour
{
    private bool _isReady = false;
    private Transform _target;
    private Transform _player;
    private Transform _cameraPos;
    private Transform _cameraFocusPos;
    private Vector3 _originalCameraPos;
    private Vector3 _originalCameraFocusPos;

    public void AddPlayer(Transform player)
    {
        _player = player;
        _cameraPos = player.transform.Find("CameraPos");
        _cameraFocusPos = player.transform.Find("CameraFocusPos");
        _originalCameraPos = _cameraPos.localPosition;
        _originalCameraFocusPos = _cameraFocusPos.localPosition;
        _isReady = (_target != null);
    }
    
    public void AddEnemy(Transform enemy)
    {
        _target = enemy;
        _isReady = (_player != null);
    }

    private void FixedUpdate()
    {
        if(!_isReady) return;

        var position = _player.position;
        var dir = (_cameraPos.position - position).normalized;
        var ray = new Ray(position, dir);

        //Debug.DrawRay(ray.origin, ray.direction * 10, Color.white);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, (int)Types.Layer.World))
        {
            _cameraPos.position = hit.point;
            _cameraFocusPos.position = hit.point;
        }
        else
        {
            if (_cameraPos.localPosition != _originalCameraPos)
                _cameraPos.localPosition = _originalCameraPos;

            if (_cameraFocusPos.localPosition != _originalCameraFocusPos)
                _cameraFocusPos.localPosition = _originalCameraFocusPos;
        }

        CameraMovement();
    }

    private void CameraMovement()
    {
        var cameraSpeed = 10f;

        if (Move.isCameraFocused)
        {
            transform.position = Vector3.Lerp(transform.position, _cameraFocusPos.position, Time.deltaTime * cameraSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _cameraFocusPos.rotation, Time.deltaTime * cameraSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _cameraPos.position, Time.deltaTime * cameraSpeed);
            var relativePosition = _target.position - transform.position;
            var targetRotation = Quaternion.LookRotation(relativePosition);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
        }
    }
}
