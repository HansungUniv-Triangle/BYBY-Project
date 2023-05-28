using Network;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeJoystickBasedButtonsPos : MonoBehaviour, IPointerDownHandler
{
    private RectTransform buttonPanel;
    private Vector2 pos;

    private PlayerCamera playerCamera;
    private Network.NetworkPlayer networkPlayer;

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
            pos = new Vector2(-90, 0);

            playerCamera.ReverseCameraPos(false);
            ChangePlayerGun(true);
        }
        // screen right
        else
        {
            buttonPanel.anchorMin = buttonPanel.anchorMax = new Vector2(0f, 0);; 
            pos = new Vector2(90, 0);

            playerCamera.ReverseCameraPos(true);
            ChangePlayerGun(false);
        }

        buttonPanel.anchoredPosition = pos;

        // 화면 밖을 벗어나지 못하도록
        var yPos = eventData.position.y;
        if (eventData.position.y < buttonPanel.rect.height * 0.5f)
        {
            yPos = buttonPanel.rect.height * 0.5f;
        }

        else if (eventData.position.y > Screen.height - buttonPanel.rect.height * 0.5f)
        {
            yPos = Screen.height - buttonPanel.rect.height * 0.5f;
        }    

        buttonPanel.Translate(new Vector3(0, yPos, 0));
    }

    private void ChangePlayerGun(bool isLeft)
    {
        if (networkPlayer is null)
            networkPlayer = GameManager.Instance.NetworkManager.PlayerCharacter.GetComponent<Network.NetworkPlayer>();

        networkPlayer.ChangeGunPos(isLeft);
    }
}
