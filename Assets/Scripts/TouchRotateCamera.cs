using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchRotateCamera : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform camPivot;
    public float rotationSpeed = 50f;

    public Vector3 beginPos;
    public Vector3 draggingPos;

    public float xAngle;
    public float yAngle;
    public float xAngleTemp;
    public float yAngleTemp;

    private Move _moveScript;

    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed += 5).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (rotationSpeed -= 5).ToString(); }
    #endregion
    
    private void Start()
    {
        _moveScript = camPivot.GetComponent<Move>();
    }

    private void OnEnable()
    {
        Quaternion rotation = camPivot.rotation;
        xAngle = 0;
        yAngle = rotation.eulerAngles.y;

        //Debug.Log(camPivot.name + ": " + xAngle + ", " + yAngle);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        beginPos = eventData.position;
        xAngleTemp = xAngle;
        yAngleTemp = yAngle;
    }

    public void OnDrag(PointerEventData eventData)
    {
        draggingPos = eventData.position;

        yAngle = yAngleTemp + (draggingPos.x - beginPos.x) * rotationSpeed * 2 / Screen.width;
        xAngle = xAngleTemp - (draggingPos.y - beginPos.y) * rotationSpeed * 2 / Screen.height;
        
        if (xAngle > 50) xAngle = 50;
        if (xAngle < -60) xAngle = -60;

        camPivot.rotation = Quaternion.Euler(xAngle, yAngle, 0.0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnBeginDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _moveScript.EndUlt();
    }
}
