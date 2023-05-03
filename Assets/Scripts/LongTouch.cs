using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool LongTouched;
    private WaitForSeconds OneSec;
    private WaitForSeconds PointOneSec;
    private Button button;

    private DoubleTouch _doubleTouch;

    private void Awake()
    {
        button = GetComponent<Button>();
        LongTouched = false;
        OneSec = new WaitForSeconds(1f);
        PointOneSec = new WaitForSeconds(.1f);
        
        _doubleTouch = GameObject.Find("Floating Joystick").GetComponent<DoubleTouch>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(IsLongTouch());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (LongTouched)
        {
            LongTouched = false;
            button.enabled = false;
            StartCoroutine(buttonEnable());
        }
        else
        {
            StopCoroutine(IsLongTouch());
        }
    }

    private IEnumerator IsLongTouch()
    {
        yield return OneSec;
        LongTouched = true;
        RDG.Vibration.Vibrate(20, 1);

        registerButton();
        //Debug.Log("LongTouched");
    }

    private IEnumerator buttonEnable()
    {
        yield return PointOneSec;
        button.enabled = true;
    }

    private void registerButton()
    {
        _doubleTouch.SetButton(button);
    }
}
