using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoubleTouch : MonoBehaviour, IPointerDownHandler
{
    private float interval = 0.25f;
    private float doubleClickedTime = -1.0f;
    public bool isDoubleClicked = false;
    public Button button;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        if ((Time.time - doubleClickedTime) < interval)
        {
            isDoubleClicked = true;
            doubleClickedTime = -1.0f;
            GetComponent<FloatingJoystick>().OnPointerUp(eventData);    // ¡∂¿ÃΩ∫∆Ω ≤Ù±‚
            button.onClick.Invoke();
        }
        else
        {
            isDoubleClicked = false;
            doubleClickedTime = Time.time;
        }
    }

    public void SetButton(Button button)
    {
        this.button = button;
    }
}
