using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIHolder;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchRotateCamera : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform camPivot;
    public float rotationSpeed = 100f;

    public Vector3 beginPos;
    public Vector3 draggingPos;

    public float xAngle;
    public float yAngle;
    public float xAngleTemp;
    public float yAngleTemp;

    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed += 5).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed -= 5).ToString(); }
    #endregion

    public bool isReady = false;

    private void Update()
    {
        FindCamPivot();
    }

    private void OnEnable()
    {
        if (!isReady) return;

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
                isReady = true;
                OnEnable();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isReady) return;
        
        beginPos = eventData.position;
        beginPos = PlayerCamera.GetRotatedCoordinates(beginPos.x, beginPos.y);

        xAngleTemp = xAngle;
        yAngleTemp = yAngle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isReady) return;
        
        draggingPos = eventData.position;
        draggingPos = PlayerCamera.GetRotatedCoordinates(draggingPos.x, draggingPos.y);

        yAngle = yAngleTemp + (draggingPos.x - beginPos.x) * rotationSpeed * 2 / Screen.width;
        xAngle = xAngleTemp - (draggingPos.y - beginPos.y) * rotationSpeed * 2 / Screen.height;
        
        if (xAngle > 50) xAngle = 50;
        if (xAngle < -60) xAngle = -60;

        camPivot.rotation = Quaternion.Euler(xAngle, yAngle, 0.0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isReady) return;
        
        OnBeginDrag(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.NetworkManager.PlayerCharacter.EndUlt();
    }
}
