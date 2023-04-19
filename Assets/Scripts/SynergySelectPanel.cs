using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;

public class SynergySelectPanel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public List<Synergy> commonSynergyList = new List<Synergy>();
    public List<Synergy> uncommonSynergyList = new List<Synergy>();
    public List<Synergy> rareSynergyList = new List<Synergy>();
    private List<int> isNumIn = new List<int>(); 
    public List<GameObject> synergyBtn = new List<GameObject>();
    public List<Synergy> currentSynergyList = new List<Synergy>();
    public List<GameObject> synergyPageList = new List<GameObject>();
    private List<List<Synergy>> totalPageList = new List<List<Synergy>>();
    //private NetworkPlayer _NetworkPlayer = GetComponent<NetworkPlayer>();

    public GameObject prefabBtn;
    private GameObject synergyPage;
    private GameObject rerollBtn;

    [SerializeField]
    private Transform spawnPointOrigin;
    private Transform[] spawnPoint;
    
    private GameObject synergyOrder;
    private GameObject synergyListText;
    public GameObject statPage;

    private bool statPageStatus = false;
    private bool[] isRerollBefore = new bool[7];
    private bool isSwipe = false;
    private int synergyBtnStatus = 0;
    private int rerollCount = 1;
    private int order = 6;

    [SerializeField] private float swipeThreshold = 100f;
    [SerializeField] private float swipeDurationThreshold = 0.3f;
    [SerializeField] private int spawnNum = 6;

    private Vector2 swipeStartPos;
    private float swipeStartTime;

    public class SynergyPage
    {
        public bool isRerolled = false;
        public Synergy[] synergies = new Synergy[3];
        public GameObject synegyObj;

        public bool AddSynergy(Synergy synergy)
        {
            if (synergies[0] == null)
            {
                synergies[0] = synergy;
                return true;
            }
            else if (synergies[1] == null)
            {
                synergies[1] = synergy;
                return true;
            }
            else if (synergies[2] == null)
            {
                synergies[2] = synergy;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Clear()
        {
            synergies[0] = null;
            synergies[1] = null;
            synergies[2] = null;
            // 초기화 작업...
        }
        
        // 시너지를 랜덤으로 생성하는 함수
        
        // 시너지 리스트에 중복된 숫자 있는지 확인
        
        // 시너지 오브제에 시너지를 적용하는 함수
        public void ApplySynergyToObj()
        {
            // 
        }
    }

    public SynergyPage[] SynergyPages = new SynergyPage[6];
    public int currentPage = 1;
    SynergyPage a = SynergyPages[currentPage];

    void Awake()
    {
        synergyPage = GameObject.Find("ItemPage");
        synergyListText = GameObject.Find("ItemPageText");
        synergyOrder = GameObject.Find("ItemOrder");
        rerollBtn = GameObject.Find("RerollBtn");
    }
    
    public void MakeSynergyPage()
    {
        spawnPoint = new Transform[spawnPointOrigin.transform.childCount];
        var index = 0;
        for (var i = 6; i < spawnPoint.Length; i++)
        {
            SpawnSynergy(SynergyPages[index], spawnPoint[i]);
        }

        SetCurrentPage(spawnNum);
    }

    private void GetSynergyList()
    {
        int synergyPageRnd = Random.Range(0, 100);
        if (synergyPageRnd <= 50)
        {
            currentSynergyList = commonSynergyList;
            synergyListText.GetComponent<TextMeshProUGUI>().text = "Common";
        }
        else if(synergyPageRnd > 50 && synergyPageRnd < 80)
        {
            currentSynergyList = uncommonSynergyList;
            synergyListText.GetComponent<TextMeshProUGUI>().text = "Uncommon";
        }
        else
        {
            currentSynergyList = rareSynergyList;
            synergyListText.GetComponent<TextMeshProUGUI>().text = "Rare";
        }
        totalPageList.Add(currentSynergyList);
    }

    private void GetSynergy()
    {
        if (synergyBtnStatus != 3)
        {
            int rnd = Random.Range(0, currentSynergyList.Count);
            if (isNumIn.Contains(rnd) == false)
            {
                isNumIn.Add(rnd);
                synergyBtn[synergyBtnStatus].GetComponentsInChildren<TextMeshProUGUI>()[0].text = currentSynergyList[rnd].synergyName;
                synergyBtnStatus += 1;
                GetSynergy();
            }
            else
                GetSynergy();
        }
    }

    private void ValueInit()
    {
        isReduplicationNum[0] = 0;
        isReduplicationNum[1] = 0;
        isReduplicationNum[2] = 0;
        synergyBtnStatus = 0;

        if (IsRerolledBefore(_pageArray[order]))
        {
            
        }
        
        if (isRerollBefore[6 - order] != true) 
        {
            rerollCount = 1;
            rerollBtn.GetComponentsInChildren<TextMeshProUGUI>()[0].text = rerollCount.ToString();
        }
        else
        {
            rerollCount = 0;
            rerollBtn.GetComponentsInChildren<TextMeshProUGUI>()[0].text = rerollCount.ToString();
        }
    }

    private GameObject GetPage(int index)
    {
        
    }

    private bool IsRerolledBefore(int index)
    {
        return isRerollBefore[index];
    }

    public void RerollSynergy()
    {
        if(rerollBtn.GetComponentsInChildren<TextMeshProUGUI>()[0].text != "0")
        {
            ValueInit();
            GetSynergyList();
            GetSynergy();
            rerollCount--;
            rerollBtn.GetComponentsInChildren<TextMeshProUGUI>()[0].text = rerollCount.ToString();
            isRerollBefore[6 - order] = true;
        }
    }
    private void SelectSynergy(int index)
    {
        string btnName = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        //Debug.Log(btnName);
        for(int i = 0; i < totalPageList.Count; i++)
        {
            for (int j = 0; j < totalPageList[i].Count; j++)
            {
                //Debug.Log(totalPageList[i][j]);
                if(btnName == totalPageList[i][j].synergyName)
                {
                    //_NetworkPlayer.NetworkSynergyList.Add(totalPageList[i][j].charStatList);
                    //_NetworkPlayer.NetworkSynergyList.Add(totalPageList[i][j].WeaponStatList);
                    return;
                }
            }
        }
    }

    public void SpawnSynergy(SynergyPage synergyPage, Transform spawnPoint)
    {
        synergyPage.Clear();
        var instance = Instantiate(prefabBtn, spawnPoint);
        
        // 프리팹의 자식 오브젝트들을 리스트에 추가
        for (int i = 0; i < instance.transform.childCount; i++)
        {
            GameObject child = instance.transform.GetChild(i).gameObject;
            child.GetComponent<Button>().onClick.AddListener(() =>
            {
                // 게임매니저에서 시너지를 가져온다
                // 그 시저지를 이러쿵 저러쿵 한다.
            }
                );
        }
        
        synergyListText = synergyPage.transform.GetChild(3).gameObject;
        synergyBtn[0].transform.GetComponent<Button>().onClick.AddListener(delegate { SelectSynergy(); });
        synergyBtn[1].transform.GetComponent<Button>().onClick.AddListener(delegate { SelectSynergy(); });
        synergyBtn[2].transform.GetComponent<Button>().onClick.AddListener(delegate { SelectSynergy(); });
        GetSynergyList();
        GetSynergy();
        synergyPageList.Add(synergyPage);
        //프리팹 복사해서 원래 위치로 애니매이션 원래 버튼 지우고 복사된 버튼 연결 
        synergyBtn.Clear();


        synergyPage.synegyObj = instance;
    }
    private void SetCurrentPage(int spawnNum)
    {
        synergyBtn.Clear();
        for (int i = 0; i < synergyPageList.Count; i++)
        {
            if (synergyPageList[i].transform.position == spawnPoint[spawnNum].transform.position)
            {
                for (int j = 0; j < synergyPageList[i].transform.childCount; j++)
                {
                    GameObject child = synergyPageList[i].transform.GetChild(j).gameObject;
                    synergyBtn.Add(child);
                }
                synergyListText = synergyPageList[i].transform.GetChild(3).gameObject;
                synergyPage = synergyPageList[i];
            }
        }
        ChangeOrder();
    }

    private void DeleteBtn()
    {  
        Destroy(synergyPage); 
        synergyBtn.Clear();
    }

    private void ChangeOrder()
    {
        Image[] tempArray = synergyOrder.GetComponentsInChildren<Image>();
        /*Sprite sprite1 = Resources.Load<Sprite>("Handle_Plane");
        Sprite sprite2 = Resources.Load<Sprite>("Handle_Outline");*/

        for (int i = 0; i < tempArray.Length; i++)
        {
            if(i == 6 - order)
            {
                tempArray[i].color = Color.red;
            }
            else
            {
                tempArray[i].color = Color.white;
            }
        }
    }

    private void NextSynergy()
    {
        if (order > 0 && isSwipe == false)
        {
            isSwipe = true;
            order--;
            ValueInit();
            //전체 페이지 왼쪽으로 옮기고 현재 페이지 연결 ,order 수정
            synergyPageList[0].transform.DOMove(spawnPoint[order].transform.position, 1);
            for (int i = 1; i < synergyPageList.Count; i++)
            {
                synergyPageList[i].transform.DOMove(synergyPageList[i - 1].transform.position, 1);
            }

            synergyBtn.Clear();
            SetCurrentPage(spawnNum + 1);
            isSwipe = false;
        }
    }

    private void BeforeSynergy()
    {
        if (order < 6 && isSwipe == false)
        {
            isSwipe = true;
            order++;
            ValueInit();
            synergyPageList[6].transform.DOMove(spawnPoint[order + 6].transform.position, 1);
            for (int i = 0; i < synergyPageList.Count - 1; i++)
            {
                synergyPageList[i].transform.DOMove(synergyPageList[i + 1].transform.position, 1);
            }

            synergyBtn.Clear();
            SetCurrentPage(spawnNum - 1);
            isSwipe = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 스와이프 시작점 저장
        if (eventData.delta.magnitude > 0)
        {
            if (swipeStartPos == Vector2.zero)
            {
                swipeStartPos = eventData.position;
                swipeStartTime = Time.time;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (swipeStartPos != Vector2.zero)
        {
            Vector2 swipeDelta = eventData.position - swipeStartPos;
            float swipeDuration = Time.time - swipeStartTime;

            // 스와이프 거리와 시간 계산
            if (swipeDelta.magnitude > swipeThreshold && swipeDuration < swipeDurationThreshold)
            {
                swipeDelta.Normalize();

                // 스와이프 방향에 따라 실행될 코드 작성
                
                if (swipeDelta.y < 0f)
                {
                    if (swipeDelta.x < 0f)
                    {
                        NextSynergy();
                        // 왼쪽으로 스와이프됨
                    }
                    else if (swipeDelta.x > 0f)
                    {
                        BeforeSynergy();
                        // 오른쪽으로 스와이프됨
                    }
                    else
                         ActiveStat();
                    // 아래쪽으로 스와이프됨
                }
                else if (swipeDelta.y > 0f)
                {
                    if (swipeDelta.x < 0f)
                    {
                        NextSynergy();
                    }
                    else if (swipeDelta.x > 0f)
                    {
                        BeforeSynergy();
                    }
                    else
                        ActiveStat();
                    // 위쪽으로 스와이프됨
                }
            }

            // 스와이프 초기화
            swipeStartPos = Vector2.zero;
            swipeStartTime = 0f;
        }
    }
    public void ActiveStat()
    {
        if(statPageStatus == false)
        {
            statPage.SetActive(true);
            statPageStatus = true;
        }
        else
        {
            statPage.SetActive(false);
            statPageStatus = false;
        }
        
    }
}
