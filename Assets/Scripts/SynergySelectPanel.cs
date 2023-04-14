using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using Status;

public class SynergySelectPanel : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private BaseStat baseStat;
    public List<Synergy> commonSynergyList = new List<Synergy>();
    public List<Synergy> uncommonSynergyList = new List<Synergy>();
    public List<Synergy> rareSynergyList = new List<Synergy>();
    private List<int> isNumIn = new List<int>(); 
    public List<GameObject> synergyBtn = new List<GameObject>();
    public List<Synergy> currentSynergyList = new List<Synergy>();
    public List<GameObject> synergyPageList = new List<GameObject>();

    public GameObject prefabBtn;
    private GameObject synergyPage;
    private GameObject rerollBtn;
    private GameObject[] spawnPoint = new GameObject [13];
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


    void Awake()
    {
        baseStat = new BaseStat();
        synergyPage = GameObject.Find("ItemPage");
        synergyListText = GameObject.Find("ItemPageText");
        synergyOrder = GameObject.Find("ItemOrder");
        rerollBtn = GameObject.Find("RerollBtn");
    }

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < spawnPoint.Length; i++)
        {
            spawnPoint[i] = GameObject.Find("ItemBtnSpawn ("+(i+1)+")");
        }
        for(int i = 0; i < 7; i++)
        {
            SpawnSynergy(spawnPoint[i + 6]);
        }
        SetCurrentPage(spawnNum);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GetSynergyList()
    {
        int synergyPageRnd = Random.Range(0, 100);
        if (synergyPageRnd <= 50)
        {
            currentSynergyList = commonSynergyList;
            synergyListText.GetComponent<Text>().text = "Common";
        }
        else if(synergyPageRnd > 50 && synergyPageRnd < 80)
        {
            currentSynergyList = uncommonSynergyList;
            synergyListText.GetComponent<Text>().text = "Uncommon";
        }
        else
        {
            currentSynergyList = rareSynergyList;
            synergyListText.GetComponent<Text>().text = "Rare";
        }
    }

    private void GetSynergy()
    {
        if (synergyBtnStatus != 3)
        {
            int rnd = Random.Range(0, currentSynergyList.Count);
            if (isNumIn.Contains(rnd) == false)
            {
                isNumIn.Add(rnd);
                synergyBtn[synergyBtnStatus].GetComponentsInChildren<Text>()[0].text = currentSynergyList[rnd].synergyName;
                synergyBtnStatus += 1;
                GetSynergy();
            }
            else
                GetSynergy();
        }
    }

    private void ValueInit()
    {
        isNumIn.Clear();
        synergyBtnStatus = 0;
        if (isRerollBefore[6 - order] != true) 
        {
            rerollCount = 1;
            rerollBtn.GetComponentsInChildren<Text>()[0].text = rerollCount.ToString();
        }
        else
        {
            rerollCount = 0;
            rerollBtn.GetComponentsInChildren<Text>()[0].text = rerollCount.ToString();
        }
    }

    public void RerollSynergy()
    {
        if(rerollBtn.GetComponentsInChildren<Text>()[0].text != "0")
        {
            ValueInit();
            GetSynergyList();
            GetSynergy();
            rerollCount--;
            rerollBtn.GetComponentsInChildren<Text>()[0].text = rerollCount.ToString();
            isRerollBefore[6 - order] = true;
        }
    }
    private void SelectSynergy()
    {
        /*string btnName = EventSystem.current.currentSelectedGameObject.name;
        for(int i = 0; i < currentSynergyList.Count; i++)
        {
            if(btnName == currentSynergyList[i].synergyName)
            {
                baseStat.AddRatioStat(currentSynergyList[i].statList);
            }
        }*/
        
    }

    public void SpawnSynergy(GameObject spawnPoint)
    {
        ValueInit();
        //synergyBtn.Clear();

        synergyPage = Instantiate(prefabBtn, spawnPoint.transform.position, Quaternion.identity, GameObject.Find("ItemSelectPanel").transform);

        // 프리팹의 자식 오브젝트들을 리스트에 추가
        for (int i = 0; i < synergyPage.transform.childCount; i++)
        {
            GameObject child = synergyPage.transform.GetChild(i).gameObject;
            synergyBtn.Add(child);
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
