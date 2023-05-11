using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class SynergyPageManager : MonoBehaviour
{
    public GameObject prefabPanel;
    private GameObject synergyPanel;

    public List<Synergy> commonSynergyList = new List<Synergy>();
    public List<Synergy> uncommonSynergyList = new List<Synergy>();
    public List<Synergy> rareSynergyList = new List<Synergy>();
    public Dictionary<string, List<Synergy>> rarityGroup = new Dictionary<string, List<Synergy>>();

    //private NetworkPlayer _NetworkPlayer;
    public SynergyPage[] synergyPages = new SynergyPage[7];
    public SynergySelectPanel synergySelectPanel;
    public int currentPage = 0;

    [SerializeField]
    private Transform spawnPointOrigin;
    private static Transform[] spawnPoint;

    void Awake() 
    {
        rarityGroup.Add("common", commonSynergyList);
        rarityGroup.Add("uncommon", uncommonSynergyList);
        rarityGroup.Add("rare", rareSynergyList);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (synergyPanel == null)
            {
                synergyPanel = Instantiate(prefabPanel, transform);
                MakeSynergyPage();
            }
            else
            {
                synergyPanel.SetActive(true);
                MakeSynergyPage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            synergyPanel.SetActive(false);
            
            synergySelectPanel.RemoveSynergyPages(synergyPages);  
        }
    }

    public void MakeSynergyPage()
    {
        //spawnPointOrigin = synergyPanel.transform.GetChild(3);
        spawnPoint = new Transform[spawnPointOrigin.transform.childCount];
        spawnPoint = spawnPointOrigin.GetComponentsInChildren<Transform>();
        synergySelectPanel = synergyPanel.GetComponent<SynergySelectPanel>();
    
        for (var i = 0; i < synergyPages.Length; i++)
        {
            synergyPages[i] = new SynergyPage();
            if (i + 2 >= 3)
            {
                synergyPages[i].synergyObj = synergySelectPanel.SpawnSynergy(synergyPages[i], spawnPoint[3]);
            }
            else
            {
                synergyPages[i].synergyObj = synergySelectPanel.SpawnSynergy(synergyPages[i], spawnPoint[2]);
            }
            CreateRandomSynergy(synergyPages[i]);
            synergySelectPanel.ApplySynergyToObj(synergyPages[i]);
            synergyPages[i].pageNumber = i;
        }
        synergySelectPanel.ChangeOrder();
        GetRecommendationPercentage();
        synergySelectPanel.ApplySynergyToObj(synergyPages[currentPage]);
        int j = FindMaxRecommendation(synergyPages[currentPage]);
        synergySelectPanel.DisplayRecommendation(synergyPages[currentPage], j);
    }

    public void SelectSynergy()
    {
        string synergyName = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        Debug.Log(synergyName);
        synergyPages[currentPage].FindSelectedSynergyInSynergies(synergyName);
    }

    public void ApplySelectedSynergyToCharacter()
    {
        
    }

    public void MoveSynergyPageRight()
    {
        /*if(synergyPages[currentPage + 1] != null)
        {
            Vector3 leftSpawnPoint = spawnPoint[1].transform.position;
            synergyPages[currentPage + 1].synergyObj.transform.position = leftSpawnPoint;
        }*/
        if (currentPage > 0)
        {
            synergyPages[currentPage].synergyObj.transform.DOMove(spawnPoint[3].transform.position, 1);
            synergyPages[currentPage - 1].synergyObj.transform.DOMove(spawnPoint[2].transform.position, 1);
            currentPage--;
            synergyPages[currentPage].RerollCountClear();
            synergySelectPanel.DisplayRerolled(synergyPages[currentPage],synergyPages[currentPage].rerollCount);
        }
    }

    public void MoveSynergyPageLeft()
    {
        /*if(synergyPages[currentPage - 1] != null)
        {
            Vector3 rightSpawnPoint = spawnPoint[3].transform.position;
            synergyPages[currentPage - 1].synergyObj.transform.position = rightSpawnPoint;
        }*/
        if (currentPage < 6)
        {
            synergyPages[currentPage].synergyObj.transform.DOMove(spawnPoint[1].transform.position, 1);
            synergyPages[currentPage + 1].synergyObj.transform.DOMove(spawnPoint[2].transform.position, 1);
            currentPage++;
            synergyPages[currentPage].RerollCountClear();
            synergySelectPanel.DisplayRerolled(synergyPages[currentPage], synergyPages[currentPage].rerollCount);
        }
    }

    public void RerollSynergy()
    {
        if (synergyPages[currentPage].isRerolled == false)
        {
            synergyPages[currentPage].Clear();
            CreateRandomSynergy(synergyPages[currentPage]);
            synergySelectPanel.ApplySynergyToObj(synergyPages[currentPage]);
            synergyPages[currentPage].rerollCount--;
            synergySelectPanel.DisplayRerolled(synergyPages[currentPage], synergyPages[currentPage].rerollCount);
            synergyPages[currentPage].isRerolled = true;
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
        for (int i = 0; i < synergyPages[currentPage].synergies.Length; i++)
        {
            if (i == 2)
            {
                synergyPages[currentPage].synergyRecommendationPercentage[i] = max;
            }
            else
            {
                recommendation = Random.Range(0, max + 1);
                max -= recommendation;
                synergyPages[currentPage].synergyRecommendationPercentage[i] = recommendation;
            }
        }
    }

    private string GetRandomRarity()
    {
        // 무작위 레어도 선택
        /*List<string> rarityList = new List<string>(rarityGroup.Keys);
        float randomRarityChoosingNumber = Random.Range(0f, 1f);
        string randomRarity;

        if (randomRarityChoosingNumber < 0.5f)
        {
            randomRarity = "common";
        }
        else if (randomRarityChoosingNumber < 0.8f)
        {
            randomRarity = "uncommon";
        }
        else
        {
            randomRarity = "rare";
        }
        return randomRarity;*/
        return "common";
    }

    // 시너지를 랜덤으로 생성하는 함수
    public void CreateRandomSynergy(SynergyPage synergyPage)
    {
        synergyPage.Clear();
        synergyPage.synergyRarity = GetRandomRarity();

        for (int i = 0; i < synergyPage.synergies.Length; i++)
        {
            while (synergyPage.synergies[2] == null) {
                // 선택된 레어도의 아이템 무리에서 무작위 아이템 선택
                List<Synergy> itemGroup = rarityGroup[synergyPage.synergyRarity];
                int itemGroupRandomNumber = Random.Range(0, itemGroup.Count - 1);
                Synergy randomSynergy = itemGroup[itemGroupRandomNumber];

                if (synergyPage.CheckIsNumInSynergyList(randomSynergy) == false)
                {
                    synergyPage.IsNumInSynergyList.Add(randomSynergy);
                    synergyPage.AddSynergy(randomSynergy);
                }
            }
        }
    }
}
