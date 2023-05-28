﻿using System;
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
        public Button reloadButton;
        public Button dodgeButton;
        public Button disconnectButton;

        public Image playerHpBarImage;
        public Image enemyHpBarImage;

        public GameObject gameUIGroup;

        public TextMeshProUGUI timeText;
        public TextMeshProUGUI roundText;
        
        public TextMeshProUGUI playerScoreText;
        public TextMeshProUGUI enemyScoreText;
        
        [Header("행동분석용")]
        public GameObject behaviourObject;
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
        public class SubWeapon
        {
            public Button None;
            public Button Shield;
            public Button BuffMachine;
            public Button HeallingGun;

            public void DisableAll()
            {
                None.transform.GetChild(0).gameObject.SetActive(false);
                Shield.transform.GetChild(0).gameObject.SetActive(false);
                BuffMachine.transform.GetChild(0).gameObject.SetActive(false);
                HeallingGun.transform.GetChild(0).gameObject.SetActive(false);
            }

            public void Enable(Button button)
            {
                button.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
        public SubWeapon subWeapon;

        [Serializable]
        public class Settings
        {
            [Header("HP")]
            public TextMeshProUGUI hpText;
            public Button HpUp;
            public Button HpDown;

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
            
            reloadButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.ReloadWeapon();
            });

            dodgeButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.Dodge();
            });
            
            disconnectButton.onClick.AddListener(() =>
            {
                GameManager.Instance.NetworkManager.DisconnectingServer();
            });

            if (subWeapon.None != null)
            {
                subWeapon.None.onClick.AddListener(() =>
                {
                    subWeapon.DisableAll();
                    subWeapon.Enable(subWeapon.None);

                    // 보조무기 없애는 코드
                });

                subWeapon.Shield.onClick.AddListener(() =>
                {
                    subWeapon.DisableAll();
                    subWeapon.Enable(subWeapon.Shield);

                    // 보조무기 바꾸는 코드
                });

                subWeapon.BuffMachine.onClick.AddListener(() =>
                {
                    subWeapon.DisableAll();
                    subWeapon.Enable(subWeapon.BuffMachine);

                    // 보조무기 바꾸는 코드
                });

                subWeapon.HeallingGun.onClick.AddListener(() =>
                {
                    subWeapon.DisableAll();
                    subWeapon.Enable(subWeapon.HeallingGun);

                    // 보조무기 바꾸는 코드
                });
            }

            /* Settings */
            // hp (only in single mode)
            if (settings.hpText != null)
            {
                settings.HpUp.onClick.AddListener(() =>
                {
                    settings.hpText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseHp();
                });

                settings.HpDown.onClick.AddListener(() =>
                {
                    settings.hpText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseHp();
                });
            }

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
            
            /*
            // reverse horizontal moving
            settings.ReverseHorizontalMove.onValueChanged.AddListener( state => 
            {
                GameManager.Instance.NetworkManager.PlayerCharacter.ToggleReverseHorizontalMove(state);
            });
            */
        }
        
        public void OpenCloseMenu(GameObject menu) 
        {
            menu.SetActive(!menu.activeSelf);
        }

        public void CloseMenu(GameObject menu)
        {
            menu.SetActive(false);
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
    }
}

