using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongTouchGyro : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool LongTouched;
    private WaitForSeconds OneSec;
    private Button button;

    private void Awake()
    {
        LongTouched = false;
        OneSec = new WaitForSeconds(1f);

        button = GetComponent<Button>();
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

        button.OnPointerUp(eventData);
        Action();
        Vibrate();
    }

    public void Action()
    {
        Camera.main.GetComponent<PlayerCamera>().ToggleGyro();
        ChangeUI(GameManager.Instance.IsGyroOn);
    }

    public void ChangeUI(bool isOn)
    {
        if (isOn)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(false);
            button.interactable = true;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
            button.interactable = false;
        }
    }

    private void Vibrate()
    {
        long[] pattern = { 0, 10, 1000, 0 };
        int[] amplitudes = { 0, 1 };

        RDG.Vibration.Vibrate(pattern, amplitudes);
    }
}
