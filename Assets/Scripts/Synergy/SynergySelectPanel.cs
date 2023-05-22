using System;
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
    public List<Synergy> currentSynergyList = new List<Synergy>();
    //private NetworkPlayer _NetworkPlayer = GetComponent<NetworkPlayer>();

    public GameObject prefabSynergyPage;
    public GameObject synergySelectPanel;
    private Button rerollBtn;
    private Button finishBtn;

    public GameObject statPage;

    private bool statPageStatus = false;
    public Sprite spriteNormal;
    public Sprite spriteCurrent;

    public Slider timer;
    public TextMeshProUGUI timerText;
    
    [SerializeField] private float swipeThreshold = 100f;
    [SerializeField] private float swipeDurationThreshold = 0.3f;

    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private bool isSwiping = false;

    public SynergyPageManager synergyPageManager;

    void Awake()
    {
        rerollBtn = synergySelectPanel.GetComponentsInChildren<Button>()[0];
        rerollBtn.onClick.AddListener(() => synergyPageManager.RerollSynergy());
    }

    public void SetSynergyPageManager(SynergyPageManager synergyPageManager)
    {
        this.synergyPageManager = synergyPageManager;
    }

    public GameObject SpawnSynergy(Transform spawnPoint)
    {
        var instance = Instantiate(prefabSynergyPage, spawnPoint.position, Quaternion.identity, gameObject.transform);
        instance.transform.SetSiblingIndex(3);
        for (int i = 0; i < instance.GetComponentsInChildren<Button>().Length; i++)
        {
            instance.GetComponentsInChildren<Button>()[i].onClick.AddListener(() => synergyPageManager.SelectSynergy());
        }
        return instance;
    }

    public void DisplaySynergySelected(SynergyPage synergyPage, GameObject synergyButton)
    {
        for (int i = 1; i < synergyPage.synergyObj.transform.childCount; i++)
        {
            GameObject child = synergyPage.synergyObj.transform.GetChild(i).gameObject;
            Debug.Log(child);
            if (child != synergyButton)
            {
                child.GetComponent<CanvasGroup>().alpha = 0.5f;
            }
            else
            {
                child.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }

    public void DisplayRerolled(SynergyPage synergy, int rerollCount)
    {
        rerollBtn.GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergy.rerollCount.ToString();
    }

    public void DisplayRecommendation(SynergyPage synergyPage, int i)
    {
        GameObject child = synergyPage.synergyObj.transform.GetChild(i + 1).gameObject;
        child.transform.GetChild(2).GetComponent<Image>().gameObject.SetActive(true);
    }

    public void RemoveSynergyPages(SynergyPage[] synergyPages)
    {
        for(int i = 0; i < synergyPages.Length; i++)
        {
            if(synergyPages[i] != null)
                Destroy(synergyPages[i].synergyObj);
        }
    }

    // 시너지 오브제에 시너지를 적용하는 함수
    public void ApplySynergyToObj(SynergyPage synergyPage)
    {
        // 시너지 그려서 화면에 적용
        for (int i = 0; i < synergyPage.synergyObj.transform.childCount; i++)
        {
            GameObject child = synergyPage.synergyObj.transform.GetChild(i).gameObject;
            if (i == 0)
            {

                child.GetComponent<TextMeshProUGUI>().text = synergyPage.synergyRarity.ToString();

            }
            else
            {
                child.transform.GetChild(0).GetComponentsInChildren<Image>()[0].sprite = synergyPage.synergies[i - 1].sprite;
                child.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.synergies[i - 1].synergyExplain;
                child.transform.GetChild(4).GetComponentsInChildren<Image>()[0].GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.synergyRecommendationPercentage[i - 1].ToString() + "%";
                child.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }
 

    public void ChangeOrder()
    {
        for (int i = 0; i < 7; i++)
        {
            Image temp = synergySelectPanel.transform.GetChild(2).GetChild(i).GetComponent<Image>();
            if (i == synergyPageManager.CurrentPage)
            {
                temp.sprite = spriteCurrent;
            }
            else
            {
                temp.sprite = spriteNormal;
            }
        }
    }

    private void NextSynergy()
    {
        synergyPageManager.MoveSynergyPageLeft();
        ChangeOrder();
    }

    private void BeforeSynergy()
    {
        synergyPageManager.MoveSynergyPageRight();
        ChangeOrder();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSwiping)
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
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (swipeStartPos != Vector2.zero)
        {
            Vector2 swipeDelta = eventData.position - swipeStartPos;
            float swipeDuration = Time.time - swipeStartTime;
            isSwiping = true;

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
                    {
                        if (statPageStatus == true)
                        {
                            ActiveStat();
                        }
                    }
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
                    {
                        if (statPageStatus == false)
                        {
                            ActiveStat();
                        }
                    }
                    // 위쪽으로 스와이프됨

                }
            }

            // 스와이프 초기화
            swipeStartPos = Vector2.zero;
            swipeStartTime = 0f;
            StartCoroutine(ResetSwipeCoroutine());
        }
    }

    private IEnumerator ResetSwipeCoroutine()
    {

        yield return new WaitForSecondsRealtime(0.5f); // 1프레임 대기

        isSwiping = false; // 스와이프 상태 초기화
    }

    public void ActiveStat()
    {
        if (statPageStatus == false)
        {
            statPage.transform.DOMoveY(1500f, 1f);
            statPageStatus = true;
        }
        else
        {
            statPage.transform.DOMoveY(25f, 1f);
            statPageStatus = false;
        }
    }

    public void SetTimerValue(float value, float max)
    {
        timer.value = value / max;
        timerText.text = value.ToString("F0");
    }
}