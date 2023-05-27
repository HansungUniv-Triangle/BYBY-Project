using System;
using DG.Tweening;
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
        public Button disconnectButton;

        public Image playerHpBarImage;
        public Image enemyHpBarImage;

        public GameObject gameUIGroup;

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI roundText;
        
        public TextMeshProUGUI playerScoreText;
        public TextMeshProUGUI enemyScoreText;
        public TextMeshProUGUI playerNickText;
        public TextMeshProUGUI enemyNickText;

        public GameObject bulletCircle;
        public Image bulletLine;
        public TextMeshProUGUI bulletText;

        public GameObject attackCircle;

        public GameEndUI gameWin;
        public GameEndUI gameDefeat;
        
        [Header("행동분석용")]
        public GameObject behaviourObject;

        public TextMeshProUGUI playerNickResultText;
        public TextMeshProUGUI enemyNickResultText;
        public TextMeshProUGUI playerScoreResultText;
        public TextMeshProUGUI enemyScoreResultText;
        public GameObject playerResultWin;
        public GameObject enemyResultWin;
        
        public TextMeshProUGUI playerHitValueText;
        public TextMeshProUGUI enemyHitValueText;
        public Slider hitSlider;
    
        public TextMeshProUGUI playerDodgeValueText;
        public TextMeshProUGUI enemyDodgeValueText;
        public Slider dodgeSlider;
    
        public TextMeshProUGUI playerAccValueText;
        public TextMeshProUGUI enemyAccValueText;
        public Slider accSlider;
    
        public TextMeshProUGUI playerDamageValueText;
        public TextMeshProUGUI enemyDamageValueText;
        public Slider damageSlider;
    
        public TextMeshProUGUI playerDestroyValueText;
        public TextMeshProUGUI enemyDestroyValueText;
        public Slider destroySlider;
    
        public TextMeshProUGUI playerReloadValueText;
        public TextMeshProUGUI enemyReloadValueText;
        public Slider reloadSlider;

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
            
            disconnectButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.DisconnectingServer();
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

        public void ActiveGameWin(string playerNick, string enemyNick, int playerScore, int enemyScore)
        {
            gameWin.player.text = playerNick;
            gameWin.enemy.text = enemyNick;
            gameWin.playerScore.text = playerScore.ToString();
            gameWin.enemyScore.text = enemyScore.ToString();
            gameWin.endButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.DisconnectingServer();
            });
            gameWin.gameObject.SetActive(true);
        }
        
        public void ActiveGameDefeat(string playerNick, string enemyNick, int playerScore, int enemyScore)
        {
            gameDefeat.player.text = playerNick;
            gameDefeat.enemy.text = enemyNick;
            gameDefeat.playerScore.text = playerScore.ToString();
            gameDefeat.enemyScore.text = enemyScore.ToString();
            gameDefeat.endButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.DisconnectingServer();
            });
            gameDefeat.gameObject.SetActive(true);
        }
        
        public void OpenCloseMenu(GameObject menu) 
        {
            menu.SetActive(!menu.activeSelf);
        }

        public void SetRoundResult(string playerNick, string enemyNick, int playerRound, int enemyRound)
        {
            playerNickResultText.text = playerNick;
            enemyNickResultText.text = enemyNick;
            playerScoreResultText.text = playerRound.ToString();
            enemyScoreResultText.text = enemyRound.ToString();
        }
        
        public void ActivePlayerRoundWin()
        {
            playerResultWin.SetActive(true);
            enemyResultWin.SetActive(false);
        }
        
        public void ActiveEnemyRoundWin()
        {
            playerResultWin.SetActive(false);
            enemyResultWin.SetActive(true);
        }

        public void SetHitAnalysis(int value1, int value2)
        {
            playerHitValueText.text = value1.ToString();
            enemyHitValueText.text = value2.ToString();
            hitSlider.value = value1 / (float)(value1 + value2);
        }
        
        public void SetDodgeAnalysis(int value1, int value2)
        {
            playerDodgeValueText.text = value1.ToString();
            enemyDodgeValueText.text = value2.ToString();
            dodgeSlider.value = value1 / (float)(value1 + value2);
        }

        public void SetAccAnalysis(int value1, int value2)
        {
            playerAccValueText.text = value1.ToString();
            enemyAccValueText.text = value2.ToString();
            accSlider.value = value1 / (float)(value1 + value2);
        }
        
        public void SetDamageAnalysis(int value1, int value2)
        {
            playerDamageValueText.text = value1.ToString();
            enemyDamageValueText.text = value2.ToString();
            damageSlider.value = value1 / (float)(value1 + value2);
        }
        
        public void SetDestroyAnalysis(int value1, int value2)
        {
            playerDestroyValueText.text = value1.ToString();
            enemyDestroyValueText.text = value2.ToString();
            destroySlider.value = value1 / (float)(value1 + value2);
        }
        
        public void SetReloadAnalysis(int value1, int value2)
        {
            playerReloadValueText.text = value1.ToString();
            enemyReloadValueText.text = value2.ToString();
            reloadSlider.value = value1 / (float)(value1 + value2);
        }
        
        public void SetBulletUI(float nowBullet, float maxBullet)
        {
            bulletLine.DOFillAmount(nowBullet == 0 ? 0 : nowBullet / maxBullet, 0.1f);
            bulletText.text = $"{nowBullet:F0}/{maxBullet:F0}";
        }
    }
}

