using System;
using System.Collections.Generic;
using DG.Tweening;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UIHolder
{
    public class GameUI : UIHolder
    {
        public bool singleMode;
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
        public TextMeshProUGUI playerNickText;
        public TextMeshProUGUI enemyNickText;

        public GameObject bulletCircle;
        public Image bulletLine;
        public Image bulletImage;
        public Image bulletReloadImage;
        public TextMeshProUGUI bulletText;

        public GameObject attackCircle;

        public GameEndUI gameWin;
        public GameEndUI gameDefeat;

        public RawImage hitEffect;

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
        public class SubWeapon
        {
            public List<Button> buttons;

            public void SetInfo(Button button, Weapon weapon = null)
            {
                string name, explain;
                if (weapon)
                {
                    name = weapon.weaponName;
                    explain = weapon.weaponExplain;
                }
                else
                {
                    name = "없음";
                    explain = "";
                }

                button.transform.parent.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
                button.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = explain;
            }

            public void DisableCheckMarkAll()
            {
                foreach (var button in buttons)
                {
                    button.transform.GetChild(0).gameObject.SetActive(false);
                }
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

            [Header("Calm")]
            public TextMeshProUGUI calmText;
            public Button CalmUp;
            public Button CalmDown;

            [Header("Special")]
            public TextMeshProUGUI specialText;
            public Button SpecialUp;
            public Button SpecialDown;

            [Header("Moving")]
            public Toggle ReverseHorizontalMove;

            [Header("Gyro")]
            public Toggle IsGyroOn;
            public GameObject ResetGyro;
        }
        public Settings settings;
        
        protected override void Initial()
        {
            canvasManager = new CanvasManager();

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
            
            reloadButton.onClick.AddListener(() =>
            {
                var weapon = GameManager.Instance.NetworkManager.PlayerCharacter.gameObject.GetComponentInChildren<NetworkProjectileHolder>();
                weapon.CallReload(false);
            });
            
            if (singleMode)
            {
                disconnectButton.onClick.AddListener(() =>
                {
                    GameManager.Instance.NetworkManager.DisconnectingServer();
                });
                
                resetPositionButton.onClick.AddListener(() =>
                {
                    GameManager.Instance.NetworkManager.PlayerCharacter.InitPosition();
                });

                /* Sub Weapons */
                // None Button
                subWeapon.SetInfo(subWeapon.buttons[0]);
                subWeapon.buttons[0].onClick.AddListener(() =>
                {
                    subWeapon.DisableCheckMarkAll();
                    subWeapon.Enable(subWeapon.buttons[0]);

                    // 보조무기 없애는 코드
                    GameManager.Instance.NetworkManager.DespawnSubWeapon();
                });

                var weaponList = GameManager.Instance.WeaponList;
                List<Weapon> subWeaponList = new List<Weapon>();

                foreach(var weapon in weaponList)
                {
                    if (!weapon.isMainWeapon)
                        subWeaponList.Add(weapon);
                }

                for (int i = 0; i < subWeapon.buttons.Count - 1; i++)
                {
                    var weapon = subWeaponList[i];
                    var button = subWeapon.buttons[i + 1];
                    subWeapon.SetInfo(button, weapon);

                    button.onClick.AddListener(() =>
                    {
                        GameManager.Instance.NetworkManager.DespawnSubWeapon();

                        subWeapon.DisableCheckMarkAll();
                        subWeapon.Enable(button);

                        // 보조무기 생성 코드
                        var name = button.transform.parent.GetChild(0).GetComponent<TextMeshProUGUI>().text;
                        var weapon = GameManager.Instance.WeaponList.Find(weapon => weapon.weaponName == name);

                        if (weapon is not null)
                        {
                            GameManager.Instance.NetworkManager.SpawnWeapon(weapon);
                        }
                        else
                        {
                            throw new Exception("선택된 무기 찾기 실패");
                        }
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

                // calm
                settings.CalmUp.onClick.AddListener(() =>
                {
                    settings.calmText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseCalm();
                });

                settings.CalmDown.onClick.AddListener(() =>
                {
                    settings.calmText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseCalm();
                });

                // weapon special 
                settings.SpecialUp.onClick.AddListener(() =>
                {
                    settings.specialText.text = GameManager.Instance.NetworkManager.PlayerCharacter.IncreaseSpecial();
                });

                settings.SpecialDown.onClick.AddListener(() =>
                {
                    settings.specialText.text = GameManager.Instance.NetworkManager.PlayerCharacter.DecreaseSpecial();
                });

                settings.IsGyroOn.isOn = GameManager.Instance.IsGyroOn;
                if (!GameManager.Instance.IsGyroOn)
                    CloseMenu(settings.ResetGyro);

                settings.IsGyroOn.onValueChanged.AddListener(delegate
                {
                    Camera.main.GetComponent<PlayerCamera>().ToggleGyro();
                    OpenCloseMenu(settings.ResetGyro);
                });
            }
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

        public void OnHitEffect(float strength)
        {
            hitEffect.DOKill();
            hitEffect.DOColor(new Color(127, 0, 0, strength), 0.5f).SetEase(Ease.OutElastic)
                .OnComplete(() => hitEffect.DOColor(new Color(127, 0, 0, 0), 0.1f));
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
        
        public void SetBulletUI(float nowBullet, float maxBullet)
        {
            bulletLine.DOFillAmount(nowBullet == 0 ? 0 : nowBullet / maxBullet, 0.1f);
            bulletText.text = $"{nowBullet:F0}/{maxBullet:F0}";
        }

        public void UpdateBulletCircle(bool active)
        {
            if (active)
            {
                bulletCircle.SetActive(true);
                attackCircle.SetActive(false);
            }
            else
            {
                attackCircle.SetActive(true);
                bulletCircle.SetActive(false);
            }
        }
        
        public void UpdateCircleReload(bool active)
        {
            if (active)
            {
                bulletText.color = new Color(1f, 1f, 1f, 0.3f);
                bulletLine.color = new Color(1f, 0.78f, 0f, 0.3f);
                bulletImage.gameObject.SetActive(false);
                bulletReloadImage.gameObject.SetActive(true);
            }
            else
            {
                bulletText.color = new Color(1f, 1f, 1f, 1f);
                bulletLine.color = new Color(1f, 0.78f, 0f, 1f);
                bulletImage.gameObject.SetActive(true);
                bulletReloadImage.gameObject.SetActive(false);
            }
        }
    }
}

