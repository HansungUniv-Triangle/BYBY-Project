using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool LongTouched;
    private WaitForSeconds OneSec;

    private void Awake()
    {
        LongTouched = false;
        OneSec = new WaitForSeconds(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(IsLongTouch());
    }

    private IEnumerator IsLongTouch()
    {
        yield return OneSec;
        LongTouched = true;
        RDG.Vibration.Vibrate(20, 1);

        //Debug.Log("LongTouched");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        LongTouched = false;
    }
}
