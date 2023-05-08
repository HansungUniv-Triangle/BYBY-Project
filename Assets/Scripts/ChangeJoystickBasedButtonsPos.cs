using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeJoystickBasedButtonsPos : MonoBehaviour, IPointerDownHandler
{
    private RectTransform buttonPanel;
    private Vector2 pos;

    private PlayerCamera playerCamera;

    private void Start()
    {
        buttonPanel = GameObject.Find("ButtonPanel").GetComponent<RectTransform>();
        playerCamera = Camera.main.GetComponent<PlayerCamera>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // screen left
        if (Screen.width * 0.5f > eventData.position.x)     
        {
            buttonPanel.anchorMin = buttonPanel.anchorMax = new Vector2(1f, 0);
            pos = new Vector2(-75, 0);

            playerCamera.ReverseCameraPos(false);
        }
        // screen right
        else
        {
            buttonPanel.anchorMin = buttonPanel.anchorMax = new Vector2(0f, 0);; 
            pos = new Vector2(75, 0);

            playerCamera.ReverseCameraPos(true);
        }

        buttonPanel.anchoredPosition = pos;
        buttonPanel.Translate(new Vector3(0, eventData.position.y, 0));
    }
}
