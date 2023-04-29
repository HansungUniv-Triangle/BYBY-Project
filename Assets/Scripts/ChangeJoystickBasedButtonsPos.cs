using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeJoystickBasedButtonsPos : MonoBehaviour, IPointerDownHandler
{
    private RectTransform buttonPanel;
    private Vector2 pos;

    private void Start()
    {
        buttonPanel = GameObject.Find("ButtonPanel").GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // screen left
        if (Screen.width * 0.5f > eventData.position.x)     
        {
            buttonPanel.anchorMin = buttonPanel.anchorMax = new Vector2(1f, 0);
            pos = new Vector2(-100, 0);
        }
        // screen right
        else
        {

            buttonPanel.anchorMin = buttonPanel.anchorMax = new Vector2(0f, 0);; 
            pos = new Vector2(100, 0);
        }

        buttonPanel.anchoredPosition = pos;
        buttonPanel.Translate(new Vector3(0, eventData.position.y, 0));
    }
}
