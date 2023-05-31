using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using GameStatus;
using TMPro;
using Types;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class SynergyPageManager : MonoBehaviour
{
    public GameObject prefabPanel;
    private GameObject synergyPanel;
    
    private SynergyPage[] _synergyPages = new SynergyPage[7];
    private SynergySelectPanel _synergySelectPanel;
    public int CurrentPage { get; private set; }

    [SerializeField]
    private Transform _spawnPointOrigin;
    private RectTransform[] _spawnPoint;

    private RectTransform _thisRectTransform;
    private float _canvasHeight;

    private void Awake() 
    {
        synergyPanel = Instantiate(prefabPanel, transform);

        _synergySelectPanel = synergyPanel.GetComponent<SynergySelectPanel>();
        _synergySelectPanel.SetSynergyPageManager(this);

        _spawnPoint = _spawnPointOrigin.GetComponentsInChildren<RectTransform>();
        _thisRectTransform = gameObject.GetComponent<RectTransform>();
        _canvasHeight = _thisRectTransform.rect.height;
        _thisRectTransform.DOAnchorPosY(_canvasHeight + 1000, 0);
    }

    private void Start()
    {
        GameManager.Instance.SetSynergyPageManager(this);
    }

    public void SetActiveSynergyPanel(bool value)
    {
        StartCoroutine(_synergySelectPanel.ResetSwipeCoroutine(1f));
        if (value)
        {
            DOTween.Sequence()
                .Append(_thisRectTransform.DOAnchorPosY(0, 1f))
                .OnStart(() => synergyPanel.SetActive(true));
        }
        else
        {
            DOTween.Sequence()
                .OnStart(() =>
                {
                    ApplySelectedSynergyToCharacter();
                    foreach (var synergyPage in _synergyPages)
                    {
                        synergyPage.synergyObj.transform.DOPause();
                    }
                })
                .Append(_thisRectTransform.DOAnchorPosY(_canvasHeight, 1f))
                .OnComplete(() =>
                {
                    synergyPanel.SetActive(false);
                });
        }
        _synergySelectPanel.DisableStat();
    }

    public void MakeSynergyPage()
    {
        _synergySelectPanel.RemoveSynergyPages(_synergyPages);
        CurrentPage = 0;

        for (var i = 0; i < _synergyPages.Length; i++)
        {
            _synergyPages[i] = new SynergyPage();
            _synergyPages[i].synergyObj = i switch
            {
                0 => _synergySelectPanel.SpawnSynergy(_spawnPoint[2]),
                _ => _synergySelectPanel.SpawnSynergy(_spawnPoint[3])
            };
            _synergyPages[i].pageNumber = i;
            _synergyPages[i].Clear();
            
            if (i == 3)
            {
                CreateRandomWeapon(_synergyPages[i]);
                _synergySelectPanel.ApplyWeaponToObj(_synergyPages[i]);
            }
            else
            {
                CreateRandomSynergy(_synergyPages[i]);
                
                for (var count = 0; count < _synergyPages[i].synergies.Length; count++)
                {
                    var totalRecommendation = 0f;
                    foreach (var stat in _synergyPages[i].synergies[count].charStatList)
                    {
                        // ratio로 실질적으로 증가하는 양
                        var increaseRatio = stat.Ratio * GameManager.Instance.NetworkManager.PlayerCharacter.GetCharStat(stat.Type).Amount;
                        var increaseFinalValue = stat.Amount + increaseRatio;
                        totalRecommendation += increaseFinalValue * GameManager.Instance.PlayerBehaviorAnalyzer.GetRecommendation(stat.Type);
                    }
                
                    foreach (var stat in _synergyPages[i].synergies[count].weaponStatList)
                    {
                        // ratio로 실질적으로 증가하는 양
                        var increaseRatio = stat.Ratio * GameManager.Instance.NetworkManager.PlayerCharacter.GetWeaponStat(stat.Type).Amount;
                        var increaseFinalValue = stat.Amount + increaseRatio;
                        totalRecommendation += increaseFinalValue * GameManager.Instance.PlayerBehaviorAnalyzer.GetRecommendation(stat.Type);
                    }
                    
                    var recommend = (int)(totalRecommendation * 100f);
                    _synergyPages[i].synergyRecommendationPercentage[count] = recommend > 0 ? recommend : 1;
                }

                var sum = _synergyPages[i].synergyRecommendationPercentage.Sum();
                for (var index = 0; index < _synergyPages[i].synergyRecommendationPercentage.Length; index++)
                {
                    var percent = (int)(_synergyPages[i].synergyRecommendationPercentage[index] / (float)sum * 100f);
                    _synergyPages[i].synergyRecommendationPercentage[index] = percent;
                }
            
                _synergySelectPanel.DisplayRecommendation(_synergyPages[i], FindMaxRecommendation(_synergyPages[i]));
                _synergySelectPanel.ApplySynergyToObj(_synergyPages[i]);
            }
        }
        _synergySelectPanel.ChangeOrder();
    }

    public void SelectSynergy()
    {
        string explain = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        GameObject selectedSynergy = EventSystem.current.currentSelectedGameObject;
        _synergySelectPanel.DisplaySynergySelected(_synergyPages[CurrentPage], selectedSynergy);

        if (CurrentPage == 3)
        {
            _synergyPages[CurrentPage].FindSelectedWeaponInSynergies(explain);
        }
        else
        {
            _synergyPages[CurrentPage].FindSelectedSynergyInSynergies(explain);
        }
    }

    public void ApplySelectedSynergyToCharacter()
    {
        for (var i = 0; i < _synergyPages.Length; i++)
        {
            var synergyPage = _synergyPages[i];
            
            if (i == 3)
            {
                var selectedWeaponName = synergyPage.selectedWeapon.weaponName;
                var weapon = GameManager.Instance.WeaponList.Find(weapon => weapon.weaponName == selectedWeaponName);
                
                if (weapon is not null)
                {
                    GameManager.Instance.NetworkManager.SpawnWeapon(weapon);
                }
                else
                {
                    throw new Exception("선택된 무기 찾기 실패");
                }
            }
            else
            {
                var selectedSynergyName = synergyPage.selectedSynergy.synergyName;
                var index = GameManager.Instance.SynergyList.FindIndex(synergy => synergy.synergyName == selectedSynergyName);

                if (index != -1)
                {
                    GameManager.Instance.NetworkManager.PlayerCharacter.AddSynergy(index);
                }
                else
                {
                    throw new Exception("선택된 시너지 찾기 실패");
                }
            }
        }
    }

    public void MoveSynergyPageRight()
    {
        var condition = CurrentPage > 0;
        var thisPage = _synergyPages[CurrentPage];
        var prevPage = condition ? _synergyPages[CurrentPage - 1] : _synergyPages[^1];

        if (condition)
        {
            CurrentPage--;
        }
        else
        {
            CurrentPage = _synergyPages.Length - 1;
        }
        
        prevPage.synergyObj.transform.position = _spawnPoint[1].transform.position;
        thisPage.synergyObj.transform.DOMove(_spawnPoint[3].transform.position, 1);
        prevPage.synergyObj.transform.DOMove(_spawnPoint[2].transform.position, 1);
        
        _synergyPages[CurrentPage].RerollCountClear();
        _synergySelectPanel.DisplayRerolled(_synergyPages[CurrentPage],_synergyPages[CurrentPage].rerollCount);
    }

    public void MoveSynergyPageLeft()
    {
        var condition = CurrentPage < (_synergyPages.Length - 1);
        var thisPage = _synergyPages[CurrentPage];
        var nextPage = condition ? _synergyPages[CurrentPage + 1] : _synergyPages[0];
        
        if (condition)
        {
            CurrentPage++;
        }
        else
        {
            CurrentPage = 0;
        }

        nextPage.synergyObj.transform.position = _spawnPoint[3].transform.position;
        thisPage.synergyObj.transform.DOMove(_spawnPoint[1].transform.position, 1);
        nextPage.synergyObj.transform.DOMove(_spawnPoint[2].transform.position, 1);
        
        _synergyPages[CurrentPage].RerollCountClear();
        _synergySelectPanel.DisplayRerolled(_synergyPages[CurrentPage], _synergyPages[CurrentPage].rerollCount);
    }

    public void RerollSynergy()
    {
        if (_synergyPages[CurrentPage].isRerolled == false)
        {
            _synergyPages[CurrentPage].Clear();

            if (CurrentPage == 3)
            {
                CreateRandomWeapon(_synergyPages[CurrentPage]);
                _synergySelectPanel.ApplyWeaponToObj(_synergyPages[CurrentPage]);
            }
            else
            {
                CreateRandomSynergy(_synergyPages[CurrentPage]);
                _synergySelectPanel.ApplySynergyToObj(_synergyPages[CurrentPage]);
            }
            
            _synergyPages[CurrentPage].rerollCount--;
            _synergySelectPanel.DisplayRerolled(_synergyPages[CurrentPage], _synergyPages[CurrentPage].rerollCount);
            _synergyPages[CurrentPage].isRerolled = true;
        }
    }

    private int FindMaxRecommendation(SynergyPage synergyPage)
    {
        int Max = 0;
        int i = 0;
        for(int j = 0; j < synergyPage.synergyRecommendationPercentage.Length; j++)
        {
            if (synergyPage.synergyRecommendationPercentage[j] > Max)
            {
                Max = synergyPage.synergyRecommendationPercentage[j];
                i = j;
            }
        }
        return i;
    }

    public void SetSynergySelectTimer(float value, float max)
    {
        _synergySelectPanel.SetTimerValue(value, max);
    }
    
    public void SetSynergySelectStats(BaseStat<CharStat> charBaseStat, BaseStat<WeaponStat> weaponBaseStat)
    {
        _synergySelectPanel.SetCharStats(charBaseStat);
        _synergySelectPanel.SetWeaponStats(weaponBaseStat);
    }

    private Rarity GetRandomRarity()
    {
        return Random.Range(0f, 1f) switch
        {
            < 0.45f => Rarity.Common,
            < 0.8f => Rarity.UnCommon,
            _ => Rarity.Rare
        };
    }
    
    public void CreateRandomSynergy(SynergyPage synergyPage)
    {
        var rarity = GetRandomRarity();
        synergyPage.synergyRarity = rarity;

        for (int i = 0; i < synergyPage.synergies.Length; i++)
        {
            while (synergyPage.synergies[2] == null) {
                var rarityGroup = GameManager.Instance.SynergyList.FindAll(s => s.rarity.Equals(rarity));
                var randomNumberRarityGroup = Random.Range(0, rarityGroup.Count);
                var randomSynergy = rarityGroup[randomNumberRarityGroup];
                
                if (synergyPage.CheckIsNumInSynergyList(randomSynergy) == false)
                {
                    synergyPage.IsNumInSynergyList.Add(randomSynergy);
                    synergyPage.AddSynergy(randomSynergy);
                }
            }
        }
    }
    
    public void CreateRandomWeapon(SynergyPage synergyPage)
    {
        for (int i = 0; i < synergyPage.weapons.Length; i++)
        {
            while (synergyPage.weapons[2] == null)
            {
                var weaponGroup = GameManager.Instance.WeaponList.FindAll(w => !w.isMainWeapon);
                var randomNumberWeaponGroup = Random.Range(0, weaponGroup.Count);
                var randomWeapon = weaponGroup[randomNumberWeaponGroup];

                if (synergyPage.CheckIsNumInWeaponList(randomWeapon) == false)
                {
                    synergyPage.IsNumInWeaponList.Add(randomWeapon);
                    synergyPage.AddWeapon(randomWeapon);
                }
            }
        }
    }
}
