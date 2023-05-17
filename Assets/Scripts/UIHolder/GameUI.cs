using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIHolder
{
    public class GameUI : UIHolder
    {
        public CanvasManager canvasManager;
        
        public RectTransform crossHair;
        public Joystick joystick;
        public TextMeshProUGUI hitDamageText;
        
        public Button resetPositionButton;
        public Button ultButton;
        public Button attackButton;
        public Button vibrateButton;
        public Button dodgeButton;

        public Image playerHpBarImage;
        public Image enemyHpBarImage;

        public GameObject gameUIGroup;

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI roundText;

        [Serializable]
        public class Settings
        {
            [Header("Speed")]
            public TextMeshProUGUI speedText;
            public Button SpeedUp;
            public Button SpeedDown;
            
            [Header("Jump")]
            public TextMeshProUGUI jumpText;
            public Button JumpUp;
            public Button JumpDown;
            
            [Header("Dodge")]
            public TextMeshProUGUI dodgeText;
            public Button DodgeUp;
            public Button DodgeDown;
            
            [Header("Shake")]
            public TextMeshProUGUI shakeSensitivityText;
            public Button ShakeSensitivityUp;
            public Button ShakeSensitivityDown;
            
            [Header("Shoot")]
            public TextMeshProUGUI shootDistanceText;
            public Button ShootDistanceUp;
            public Button ShootDistanceDown;
            
            [Header("Moving")]
            public Toggle ReverseHorizontalMove;
        }
        public Settings settings;
        
        protected override void Initial()
        {
            canvasManager = new CanvasManager();
            
            resetPositionButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.InitPosition();
            });
            
            ultButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.GetUlt();
            });
            
            attackButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.ToggleShooting();
            });
            
            vibrateButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.VibrateHeartBeat();
            });

            dodgeButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.Dodge();
            });

            /* Settings */
            // speed
            settings.SpeedUp.onClick.AddListener(() =>
            {
                settings.speedText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseSpeed();
            });
            
            settings.SpeedDown.onClick.AddListener(() =>
            {
                settings.speedText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseSpeed();
            });
            
            // jump
            settings.JumpUp.onClick.AddListener(() =>
            {
                settings.jumpText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseJump();
            });
            
            settings.JumpDown.onClick.AddListener(() =>
            {
                settings.jumpText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseJump();
            });
            
            // dodge
            settings.DodgeUp.onClick.AddListener(() =>
            {
                settings.dodgeText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseDodge();
            });
            
            settings.DodgeDown.onClick.AddListener(() =>
            {
                settings.dodgeText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseDodge();
            });
            
            // shake sensitivity
            settings.ShakeSensitivityUp.onClick.AddListener(() =>
            {
                settings.shakeSensitivityText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseShakeSensitivity();
            });
            
            settings.ShakeSensitivityDown.onClick.AddListener(() =>
            {
                settings.shakeSensitivityText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseShakeSensitivity();
            });
            
            // shoot distance
            settings.ShootDistanceUp.onClick.AddListener(() =>
            {
                settings.shootDistanceText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseShootDistance();
            });
            
            settings.ShootDistanceDown.onClick.AddListener(() =>
            {
                settings.shootDistanceText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseShootDistance();
            });
            
            // reverse horizontal moving
            settings.ReverseHorizontalMove.onValueChanged.AddListener( state => 
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.ToggleReverseHorizontalMove(state);
            });
        }
        
        public void OpenCloseMenu(GameObject menu) 
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
}

