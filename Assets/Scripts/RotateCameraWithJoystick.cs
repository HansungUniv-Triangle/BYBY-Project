using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateCameraWithJoystick : MonoBehaviour, IPointerUpHandler
{
    public Joystick joystick;

    public Transform joystickImage;
    public Transform camPivot;
    private Transform _camera;

    public float rotationSpeed = 0.5f;
    public float xAngle;
    public float yAngle;

    public Vector3 prevJoystickPos;

    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed += 5).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed -= 5).ToString(); }
    #endregion

    public bool isReady = false;

    private void Update()
    {
        RotateCam();
    }

    private void RotateCam()
    {
        if (isReady)
        {
            if (joystickImage.position == prevJoystickPos)
                return;

            var oh = joystick.Horizontal;
            var ov = joystick.Vertical;

            var camAngle = _camera.eulerAngles.z * Mathf.Deg2Rad;

            var h = oh * Mathf.Cos(camAngle) - ov * Mathf.Sin(camAngle);
            var v = oh * Mathf.Sin(camAngle) + ov * Mathf.Cos(camAngle);

            xAngle += Mathf.Clamp(-v * rotationSpeed, -60, 50);
            yAngle += h * rotationSpeed;

            camPivot.rotation = Quaternion.Euler(xAngle, yAngle, 0.0f);
            prevJoystickPos = joystickImage.position;
        }
    }

    private void OnEnable()
    {
        FindCamPivot();
        if (!isReady)
            return;

        Quaternion rotation = camPivot.rotation;
        xAngle = 0;
        yAngle = rotation.eulerAngles.y;
    }

    private void FindCamPivot()
    {
        if (camPivot == null)
        {
            if (GameManager.Instance.NetworkManager.PlayerCharacter)
            {
                camPivot = GameManager.Instance.NetworkManager.PlayerCharacter.transform;
                joystickImage = joystick.transform.GetChild(0);
                _camera = Camera.main.transform;
                isReady = true;
                OnEnable();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.NetworkManager.PlayerCharacter.EndUlt();
    }
}
