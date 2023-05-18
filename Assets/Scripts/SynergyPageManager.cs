using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Types;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class SynergyPageManager : MonoBehaviour
{
    public GameObject prefabPanel;
    private GameObject synergyPanel;

    //private NetworkPlayer _NetworkPlayer;
    private SynergyPage[] _synergyPages = new SynergyPage[7];
    private SynergySelectPanel _synergySelectPanel;
    public int CurrentPage { get; private set; }

    [SerializeField]
    private Transform _spawnPointOrigin;
    private RectTransform[] _spawnPoint;

    private RectTransform _thisRectTransform;

    private void Awake() 
    {
        synergyPanel = Instantiate(prefabPanel, transform);

        _synergySelectPanel = synergyPanel.GetComponent<SynergySelectPanel>();
        _synergySelectPanel.SetSynergyPageManager(this);

        _spawnPoint = _spawnPointOrigin.GetComponentsInChildren<RectTransform>();
        _thisRectTransform = gameObject.GetComponent<RectTransform>();
    }

    private void Start()
    {
        GameManager.Instance.SetSynergyPageManager(this);
    }

    public void SetActiveSynergyPanel(bool value)
    {
        if (value)
        {
            DOTween.Sequence()
                .Append(_thisRectTransform.DOAnchorPosY(0, 1f))
                .OnStart(() => synergyPanel.SetActive(true));
        }
        else
        {
            DOTween.Sequence()
                .Append(_thisRectTransform.DOAnchorPosY(1920, 1f))
                .OnComplete(() => synergyPanel.SetActive(false));
        }
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
            CreateRandomSynergy(_synergyPages[i]);
            _synergySelectPanel.ApplySynergyToObj(_synergyPages[i]);
        }
        
        _synergySelectPanel.ChangeOrder();
        GetRecommendationPercentage();
        _synergySelectPanel.ApplySynergyToObj(_synergyPages[CurrentPage]);
        int j = FindMaxRecommendation(_synergyPages[CurrentPage]);
        _synergySelectPanel.DisplayRecommendation(_synergyPages[CurrentPage], j);
    }

    public void SelectSynergy()
    {
        string synergyExplain = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        GameObject selectedSynergy = EventSystem.current.currentSelectedGameObject;
        Debug.Log(synergyExplain);
        synergySelectPanel.DisplaySynergySelected(synergyPages[currentPage], selectedSynergy);
        synergyPages[currentPage].FindSelectedSynergyInSynergies(synergyExplain);
    }

    public void ApplySelectedSynergyToCharacter()
    {
        
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
            CreateRandomSynergy(_synergyPages[CurrentPage]);
            _synergySelectPanel.ApplySynergyToObj(_synergyPages[CurrentPage]);
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

    private void GetRecommendationPercentage()
    {
        int max = 100;
        int recommendation = 0;
        for (int i = 0; i < _synergyPages[CurrentPage].synergies.Length; i++)
        {
            if (i == 2)
            {
                _synergyPages[CurrentPage].synergyRecommendationPercentage[i] = max;
            }
            else
            {
                recommendation = Random.Range(0, max + 1);
                max -= recommendation;
                _synergyPages[CurrentPage].synergyRecommendationPercentage[i] = recommendation;
            }
        }
    }

    public void SetSynergySelectTimer(float value, float max)
    {
        _synergySelectPanel.SetTimerValue(value, max);
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

    // �ó����� �������� �����ϴ� �Լ�
    public void CreateRandomSynergy(SynergyPage synergyPage)
    {
        var rarity = GetRandomRarity();
        synergyPage.synergyRarity = rarity;

        for (int i = 0; i < synergyPage.synergies.Length; i++)
        {
            while (synergyPage.synergies[2] == null) {
                var rarityGroup = GameManager.Instance.SynergyList.FindAll(s => s.rarity.Equals(rarity));
                var randomNumberRarityGroup = Random.Range(0, rarityGroup.Count);
                var randomSynergy = GameManager.Instance.SynergyList[randomNumberRarityGroup];
                
                if (synergyPage.CheckIsNumInSynergyList(randomSynergy) == false)
                {
                    synergyPage.IsNumInSynergyList.Add(randomSynergy);
                    synergyPage.AddSynergy(randomSynergy);
                }
            }
        }
    }
}
