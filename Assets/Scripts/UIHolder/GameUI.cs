using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIHolder
{
    public class GameUI : UIHolder
    {
        public RectTransform crossHair;
        public Joystick joystick;
        public Button resetPlayerLocation;
        public TextMeshProUGUI hitDamageText;
        public Button ultButton;
        public Button disconnectButton;
        public TextMeshProUGUI roundText;
        public TextMeshProUGUI bulletText;
        public Button hitButton;
        public Button rotateButton;

        protected override void Initial()
        {
            resetPlayerLocation.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.InitPosition();
            });
            
            ultButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.GetUlt();
            });
            
            disconnectButton.onClick.AddListener(() =>
            {
                GameManager.Instance.DisconnectedSever();
            });
            
            hitButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.OnHitDebugging(5f);
            });
            
            rotateButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.LocalCharacter.RotateDebugging();
            });
        }
    }
}

