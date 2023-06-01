using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using GameStatus;
using TMPro;
using Types;
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

    Color backgroundColor_Common = new Color(0.78f, 0.78f, 0.78f);
    Color backgroundColor_Uncommon = new Color32(135, 206, 235, 255);
    Color backgroundColor_Rare = new Color32(255, 180, 195, 255);

    Color rarityTextColor_Common =new Color(0.5f, 0.5f, 0.5f);
    Color rarityTextColor_Uncommon = new Color(0.06f, 0.44f, 0.79f);
    Color rarityTextColor_Rare = new Color(1f, 0.1f, 0.2f);

    public SynergyPageManager synergyPageManager;

    public TMP_Text health;
    public TMP_Text speed;
    public TMP_Text rolling;
    public TMP_Text armor;
    public TMP_Text calm;
    public TMP_Text interval;
    public TMP_Text special;
    public TMP_Text attack;
    public TMP_Text range;
    public TMP_Text reload;
    public TMP_Text bullet;
    public TMP_Text velocity;
    
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
        instance.transform.SetSiblingIndex(2);
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
        child.transform.GetChild(1).GetComponent<Image>().gameObject.SetActive(true);
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
        switch (synergyPage.synergyRarity.ToString())
        {
            case "Common":
                {
                    synergyPage.synergyObj.GetComponent<Image>().color = backgroundColor_Common;
                    break;
                }
            case "UnCommon":
                {
                    synergyPage.synergyObj.GetComponent<Image>().color = backgroundColor_Uncommon;
                    break;
                }
            case "Rare":
                {
                    synergyPage.synergyObj.GetComponent<Image>().color = backgroundColor_Rare;
                    break;
                }
            default:
                {
                    break;
                }
        }

        for (int i = 0; i < synergyPage.synergyObj.transform.childCount; i++)
        {
            GameObject child = synergyPage.synergyObj.transform.GetChild(i).gameObject;
            if (i == 0)
            {
                child.GetComponent<TextMeshProUGUI>().text = synergyPage.synergyRarity.ToString();
                switch (synergyPage.synergyRarity.ToString())
                {
                    case "Common":
                        {
                            child.GetComponent<TextMeshProUGUI>().color = rarityTextColor_Common;
                            break;
                        }
                    case "UnCommon":
                        {
                            child.GetComponent<TextMeshProUGUI>().color = rarityTextColor_Uncommon;
                            break;
                        }
                    case "Rare":
                        {
                            child.GetComponent<TextMeshProUGUI>().color = rarityTextColor_Rare;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            else
            {
                child.transform.GetChild(0).GetComponentsInChildren<Image>()[0].sprite = synergyPage.synergies[i - 1].sprite;
                child.transform.GetChild(2).GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.synergies[i - 1].synergyExplain;
                child.transform.GetChild(2).GetComponentsInChildren<TextMeshProUGUI>()[1].text = synergyPage.synergies[i - 1].synergyName;
                child.transform.GetChild(3).GetComponentsInChildren<Image>()[0].GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.synergyRecommendationPercentage[i - 1] + "%";
                child.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }
    
    public void ApplyWeaponToObj(SynergyPage synergyPage)
    {
        // 시너지 그려서 화면에 적용
        for (int i = 0; i < synergyPage.synergyObj.transform.childCount; i++)
        {
            GameObject child = synergyPage.synergyObj.transform.GetChild(i).gameObject;
            if (i == 0)
            {
                child.GetComponent<TextMeshProUGUI>().text = "보조 무기";
            }
            else
            {
                child.transform.GetChild(0).GetComponentsInChildren<Image>()[0].sprite = synergyPage.weapons[i - 1].sprite;
                child.transform.GetChild(2).GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.weapons[i - 1].weaponExplain;
                child.transform.GetChild(2).GetComponentsInChildren<TextMeshProUGUI>()[1].text = synergyPage.weapons[i - 1].weaponName;
                child.transform.GetChild(3).GetComponentsInChildren<Image>()[0].GetComponentsInChildren<TextMeshProUGUI>()[0].text = synergyPage.synergyRecommendationPercentage[i - 1].ToString() + "%";
                child.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }
 

    public void ChangeOrder()
    {
        for (int i = 0; i < 7; i++)
        {
            Image temp = synergySelectPanel.transform.GetChild(1).GetChild(i).GetComponent<Image>();
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
            StartCoroutine(ResetSwipeCoroutine(0.5f));
        }
    }

    public IEnumerator ResetSwipeCoroutine(float waitingTime)
    {
        isSwiping = true;

        yield return new WaitForSecondsRealtime(waitingTime); // 1프레임 대기

        isSwiping = false; // 스와이프 상태 초기화
    }

    private void ActiveStat()
    {
        var thisRectTransform = statPage.GetComponent<RectTransform>();
        var statPageRectTransform = statPage.transform.GetChild(1).gameObject.GetComponent<RectTransform>();
        var canvasHeight = statPageRectTransform.rect.height;
 
        if (statPageStatus == false)
        {
            statPage.transform.GetChild(1).gameObject.SetActive(true);
            thisRectTransform.DOAnchorPosY(canvasHeight, 0.5f);
            statPageStatus = true;
        }
        else
        {
            thisRectTransform.DOAnchorPosY(25, 0.5f).OnComplete(() =>
            {
                statPage.transform.GetChild(1).gameObject.SetActive(false);
                statPageStatus = false;
            });
        }
    }

    public void DisableStat()
    {
        var thisRectTransform = statPage.GetComponent<RectTransform>();

        thisRectTransform.DOAnchorPosY(25, 0.5f).OnComplete(() =>
        {
            statPage.transform.GetChild(1).gameObject.SetActive(false);
            statPageStatus = false;
        });
    }

    public void SetTimerValue(float value, float max)
    {
        timer.value = value / max;
        timerText.text = value.ToString("F0");
    }

    public void SetCharStats(BaseStat<CharStat> baseStat)
    {
        foreach (CharStat charStatKey in Enum.GetValues(typeof(CharStat)))
        {
            var amount = baseStat.GetStat(charStatKey).Amount;
            var ratio = baseStat.GetStat(charStatKey).Ratio;
            var sum = amount * ratio;
            var text = $"+{amount} * {ratio * 100}% = {sum}";
            
            switch (charStatKey)
            {
                case CharStat.Health:
                    health.text = text + $"\n나의 체력: {Utils.StatConverter.ConversionStatValue(baseStat.GetStat(CharStat.Health))}";
                    break;
                case CharStat.Speed:
                    speed.text = text + $"\n이동 속도: {Utils.StatConverter.ConversionStatValue(baseStat.GetStat(CharStat.Speed))}";
                    break;
                case CharStat.Dodge:
                    rolling.text = text + $"\n회피: {Utils.StatConverter.ConversionStatValue(baseStat.GetStat(CharStat.Dodge))}"; ;
                    break;
                case CharStat.Armor:
                    armor.text = text + $"\n데미지 {100 - (int)(Utils.StatConverter.ConversionStatValue(baseStat.GetStat(CharStat.Armor))*100f)}% 감소";
                    break;
                case CharStat.Calm:
                    calm.text = text + $"\n차분함: {Utils.StatConverter.ConversionStatValue(baseStat.GetStat(CharStat.Calm))}"; ;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public void SetWeaponStats(BaseStat<WeaponStat> baseStat)
    {
        foreach (WeaponStat weaponStatKey in Enum.GetValues(typeof(WeaponStat)))
        {
            var amount = baseStat.GetStat(weaponStatKey).Amount;
            var ratio = baseStat.GetStat(weaponStatKey).Ratio;
            var text = $"+{amount} * {ratio * 100}% = {amount * ratio}증가";
            
            switch (weaponStatKey)
            {
                case WeaponStat.Interval:
                    interval.text = text + $"\n{Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Interval))}초";
                    break;
                case WeaponStat.Special:
                    special.text = text + $"\n특화: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Special))}";
                    break;
                case WeaponStat.Attack:
                    attack.text = text + $"\n공격력: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Attack))}";
                    break;
                case WeaponStat.Range:
                    range.text = text + $"\n사거리: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Range))}";
                    break;
                case WeaponStat.Reload:
                    reload.text = text + $"\n재장전: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Reload))}";
                    break;
                case WeaponStat.Bullet:
                    bullet.text = text + $"\n탄환 수: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Bullet))}";
                    break;
                case WeaponStat.Velocity:
                    velocity.text = text + $"\n탄속: {(int)Utils.StatConverter.ConversionStatValue(baseStat.GetStat(WeaponStat.Velocity))}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
