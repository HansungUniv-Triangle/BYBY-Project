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
        StartCoroutine(IsLongTouch(eventData));
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
            StopAllCoroutines();
        }
    }

    private IEnumerator IsLongTouch(PointerEventData eventData)
    {
        yield return OneSec;
        LongTouched = true;
        RDG.Vibration.Vibrate(20, 1);

        button.OnPointerUp(eventData);
        registerButton();
        registeredVibrate();
    }

    private IEnumerator buttonEnable()
    {
        yield return PointOneSec;
        button.enabled = true;
    }

    private void registerButton()
    {
        _doubleTouch.button.transform.Find("Image").gameObject.SetActive(false);
        _doubleTouch.SetButton(button);
        button.transform.Find("Image").gameObject.SetActive(true);
    }

    private void registeredVibrate()
    {
        long[] pattern = { 0, 10, 1000, 0 };
        int[] amplitudes = { 0, 1 };

        RDG.Vibration.Vibrate(pattern, amplitudes);
    }
}
