using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Firebase.Extensions;
using Network;
using UnityEngine.EventSystems;
using TMPro;
using Random = UnityEngine.Random;

public class Lobby : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Transform _buttonOrigin;
    private Button[] _buttons = new Button[4];

    [SerializeField]
    private Transform _menuOrigin;
    private Transform[] _menu = new Transform[4];

    [SerializeField]
    private Transform _tutorialOrigin;
    private Transform[] _tutorials = new Transform[17];

    [SerializeField]
    private Transform _tutorialOrderOrigin;

    private GameObject _rankingPopup;
    private GameObject _settingsPopup;
    private GameObject _searchPopup;
    private GameObject _tutorialPopup;

    public GameObject _tab_NumberOfWin;
    public GameObject _tab_Odds;
    public GameObject _tab_WinningStreak;

    [SerializeField]
    private Transform _spawnPointOrigin;
    private RectTransform[] _spawnPoint;

    public int CurrentPage { get; private set; }
    public int TutorialCurrentPage { get; private set; }

    [SerializeField] private float swipeThreshold = 100f;
    [SerializeField] private float swipeDurationThreshold = 0.3f;

    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private bool isSwiping = false;

    public TextMeshProUGUI nickName;
    public TextMeshProUGUI win;
    public BasicSpawner spawner;
    public TextMeshProUGUI rankingTemp;
    public GameObject prefabRankpage;
    public GameObject rankingHint;

    public Toggle IsGyroOn;
    public Toggle IsVibrateOn;
    public Slider BGM;
    public Slider SoundEffect;

    private void Awake()
    {
        CurrentPage = 0;
        TutorialCurrentPage = 0;
        for(int i = 0; i < _menuOrigin.childCount; i++)
        {
            _menu[i] = _menuOrigin.GetChild(i);
        }

        for (int i = 0; i < _tutorialOrigin.childCount; i++)
        {
            _tutorials[i] = _tutorialOrigin.GetChild(i);
        }

        _buttons = _buttonOrigin.GetComponentsInChildren<Button>();
        _spawnPoint = _spawnPointOrigin.GetComponentsInChildren<RectTransform>();
        _rankingPopup = transform.GetChild(1).transform.GetChild(7).gameObject;
        _settingsPopup = transform.GetChild(1).transform.GetChild(8).gameObject;
        _searchPopup = transform.GetChild(1).transform.GetChild(9).gameObject;
        _tutorialPopup = transform.GetChild(1).transform.GetChild(10).gameObject;

        IsGyroOn.isOn = GameManager.Instance.IsGyroOn;
        IsGyroOn.onValueChanged.AddListener(delegate
        {
            ToggleIsGyro();
        });
        IsGyroOn.transform.GetChild(1).gameObject.SetActive(GameManager.Instance.IsGyroOn);

        IsVibrateOn.isOn = GameManager.Instance.IsVibrateOn;
        IsVibrateOn.onValueChanged.AddListener(delegate
        {
            ToggleIsVibrate();
        });
        IsVibrateOn.transform.GetChild(1).gameObject.SetActive(GameManager.Instance.IsVibrateOn);

        BGM.value = SoundManager.Instance.GetVolume(Types.Sound.BGM);
        SoundEffect.value = SoundManager.Instance.GetVolume(Types.Sound.Effect);
    }

    void Start()
    {
        nickName.text = DBManager.Instance.NickName;

        DBManager.Instance.GetUserWin().ContinueWithOnMainThread(task =>
        {
            win.text = $"{task.Result.Item1.ToString()}승 | {task.Result.Item2.ToString()}패";
        });
        
        ChangeButtons();
    }

    public void TabClicked()
    {
        rankingHint.SetActive(false);
        
        string tabName = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        switch (tabName)
        {
            case "승리 수":
                { 
                    _tab_NumberOfWin.SetActive(true);
                    _tab_Odds.SetActive(false);
                    _tab_WinningStreak.SetActive(false);
                    DBManager.Instance.GetManyWinRanking().ContinueWithOnMainThread(task =>
                    {
                        for (int i = 0; i < _tab_NumberOfWin.transform.childCount; i++)
                        {
                            Destroy(_tab_NumberOfWin.transform.GetChild(i).gameObject);
                        }

                        var list = task.Result;
                        int count = 1;

                        foreach (var (item1, item2) in list)
                        {
                            GameObject rankpageTemp = Instantiate(prefabRankpage, gameObject.transform.position, Quaternion.identity, GameObject.Find("NumberOfWin").transform);
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[0].text = count++.ToString();
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[1].text = item1;
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[2].text = item2 + "승";
                        }
                    });
                    break;
                }
            case "승률":
                {    
                    _tab_NumberOfWin.SetActive(false);
                    _tab_Odds.SetActive(true);
                    _tab_WinningStreak.SetActive(false);
                    DBManager.Instance.GetWinRatingRanking().ContinueWithOnMainThread(task =>
                    {
                        for (int i = 0; i < _tab_Odds.transform.childCount; i++)
                        {
                            Destroy(_tab_Odds.transform.GetChild(i).gameObject);
                        }

                        var list = task.Result;
                        int count = 1;
                        
                        foreach (var (item1, item2) in list)
                        {
                            
                            GameObject rankpageTemp = Instantiate(prefabRankpage, gameObject.transform.position, Quaternion.identity, GameObject.Find("Odds").transform);
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[0].text = count++.ToString();
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[1].text = item1;
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[2].text = item2 + "%";
                        }
                    });
                    break;
                }
            case "연승":
                {
                    _tab_NumberOfWin.SetActive(false);
                    _tab_Odds.SetActive(false);
                    _tab_WinningStreak.SetActive(true);
                    DBManager.Instance.GetWinStraightRanking().ContinueWithOnMainThread(task =>
                    {
                        for (int i = 0; i < _tab_WinningStreak.transform.childCount; i++)
                        {
                            Destroy(_tab_WinningStreak.transform.GetChild(i).gameObject);
                        }

                        var list = task.Result;
                        int count = 1;

                        foreach (var (item1, item2) in list)
                        {
                            GameObject rankpageTemp = Instantiate(prefabRankpage, gameObject.transform.position, Quaternion.identity, GameObject.Find("WinningStreak").transform);
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[0].text = count++.ToString();
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[1].text = item1;
                            rankpageTemp.GetComponentsInChildren<TextMeshProUGUI>()[2].text = item2 + "연승";
                        }
                    });
                    break;
                }
        }
    }

    public void PlayButtonClicked_Battle()
    {
        spawner.StartMultiGameRandomRoom();
    }

    public void PlayButtonClicked_Search()
    {
        _searchPopup.SetActive(true);
    }
    
    public void PlayButtonClicked_SearchBattle(TMP_InputField field)
    {
        int number = int.TryParse(field.text, out var result) ? result : Random.Range(1000, 10000);
        spawner.StartMultiGameNumberRoom(number);
    }

    public void PlayButtonClicked_Practice()
    {
        spawner.StartSingleGame();
    }

    public void PlayButtonClicked_Ranking()
    {
        _rankingPopup.SetActive(true);
    }

    public void PlayButtonClicked_Settings()
    {
        _settingsPopup.SetActive(true);
    }

    public void PopupClose_Ranking()
    {
        _rankingPopup.SetActive(false);
    }

    public void PopupClose_Settings()
    {
        _settingsPopup.SetActive(false);
    }

    public void PopupClose_Search()
    {
        _searchPopup.SetActive(false);
    }

    public void PopupClose_Tutorial()
    {
        _tutorialPopup.SetActive(false);
    }

    public void ButtonClicked_Tutorial()
    {
        _tutorialPopup.SetActive(true); 
        _tutorials[TutorialCurrentPage].gameObject.SetActive(true);
        ChangeTutorialOrder();
    }

    public void UnderButtonClicked_Battle()
    {
        int destinationPage = 0;
        if (CurrentPage - destinationPage > 0)
        {
            while (CurrentPage != destinationPage)
            {
                BeforeMenu();
            }
        }
        else
        {
            while(CurrentPage != destinationPage)
            {
                NextMenu();
            }
        }
    }

    public void UnderButtonClicked_Practice()
    {
        int destinationPage = 1;
        if (CurrentPage - destinationPage > 0)
        {
            while (CurrentPage != destinationPage)
            {
                BeforeMenu();
            }
        }
        else
        {
            while (CurrentPage != destinationPage)
            {
                NextMenu();
            }
        }
    }

    public void UnderButtonClicked_Ranking()
    {
        int destinationPage = 2;
        if (CurrentPage - destinationPage > 0)
        {
            while (CurrentPage != destinationPage)
            {
                BeforeMenu();
            }
        }
        else
        {
            while (CurrentPage != destinationPage)
            {
                NextMenu();
            }
        }
    }

    public void UnderButtonClicked_Settings()
    {
        int destinationPage = 3;
        if (CurrentPage - destinationPage > 0)
        {
            while (CurrentPage != destinationPage)
            {
                BeforeMenu();
            }
        }
        else
        {
            while (CurrentPage != destinationPage)
            {
                NextMenu();
            }
        }
    }

    public void ChangeTutorialOrder()
    {
        for (int i = 0; i < _tutorialOrderOrigin.childCount; i++)
        {
            if (i == TutorialCurrentPage)

            {
                _tutorialOrderOrigin.transform.GetChild(TutorialCurrentPage).transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                _tutorialOrderOrigin.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(false);
            }
        }

    }

    public void BeforeTutorial()
    {
        var condition = TutorialCurrentPage > 0;
        var thisPage = _tutorials[TutorialCurrentPage];
        var prevPage = condition ? _tutorials[TutorialCurrentPage - 1] : _tutorials[^1];

        if (condition)
        {
            TutorialCurrentPage--;
        }
        else
        {
            TutorialCurrentPage = _tutorials.Length - 1;
        }

        thisPage.gameObject.SetActive(false);
        prevPage.gameObject.SetActive(true);

        ChangeTutorialOrder();
    }

    public void NextTutorial()
    {
        var condition = TutorialCurrentPage < (_tutorials.Length - 1);
        var thisPage = _tutorials[TutorialCurrentPage];
        var nextPage = condition ? _tutorials[TutorialCurrentPage + 1] : _tutorials[0];

        if (condition)
        {
            TutorialCurrentPage++;
        }
        else
        {
            TutorialCurrentPage = 0;
        }

        thisPage.gameObject.SetActive(false);
        nextPage.gameObject.SetActive(true);

        ChangeTutorialOrder();
    }

    private void ChangeButtons()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            if (i == CurrentPage)

            {
                _buttons[CurrentPage].transform.DOMoveY(150f, 0.5f);
            }
            else
            {
                _buttons[i].transform.DOMoveY(0f, 0.5f);
            }
        }
    }

    public void BeforeMenu()
    {
        var condition = CurrentPage > 0;
        var thisPage = _menu[CurrentPage];
        var prevPage = condition ? _menu[CurrentPage - 1] : _menu[^1];

        if (condition)
        {
            CurrentPage--;
        }
        else
        {
            CurrentPage = _menu.Length - 1;
        }

        prevPage.transform.position = _spawnPoint[1].transform.position;
        thisPage.transform.DOMove(_spawnPoint[3].transform.position, 1);
        prevPage.transform.DOMove(_spawnPoint[2].transform.position, 1);

        ChangeButtons();
    }

    public void NextMenu()
    {
        var condition = CurrentPage < (_menu.Length - 1);
        var thisPage = _menu[CurrentPage];
        var nextPage = condition ? _menu[CurrentPage + 1] : _menu[0];

        if (condition)
        {
            CurrentPage++;
        }
        else
        {
            CurrentPage = 0;
        }

        nextPage.transform.position = _spawnPoint[3].transform.position;
        thisPage.transform.DOMove(_spawnPoint[1].transform.position, 1);
        nextPage.transform.DOMove(_spawnPoint[2].transform.position, 1);

        ChangeButtons();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isSwiping) // 스와이프 중이 아닐 때만 실행
        {
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
        if (swipeStartPos != Vector2.zero && !isSwiping)
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
                        NextMenu();
                        // 왼쪽으로 스와이프됨
                    }
                    else if (swipeDelta.x > 0f)
                    {
                        BeforeMenu();
                        // 오른쪽으로 스와이프됨
                    }
                   
                    // 아래쪽으로 스와이프됨

                }
                else if (swipeDelta.y > 0f)
                {
                    if (swipeDelta.x < 0f)
                    {
                        NextMenu();
                    }
                    else if (swipeDelta.x > 0f)
                    {
                        BeforeMenu();
                    }

                }
 
            }

            // 스와이프 초기화
            swipeStartPos = Vector2.zero;
            swipeStartTime = 0f;
            StartCoroutine(ResetSwipeCoroutine());
        }
    }

    public void ToggleIsGyro()
    {
        GameManager.Instance.ToggleGyro();
        IsGyroOn.transform.GetChild(1).gameObject.SetActive(GameManager.Instance.IsGyroOn);
    }

    public void ToggleIsVibrate()
    {
        GameManager.Instance.ToggleVibrate();
        IsVibrateOn.transform.GetChild(1).gameObject.SetActive(GameManager.Instance.IsVibrateOn);
    }

    public void SetBGMVolume(Slider slider)
    {
        SoundManager.Instance.SetVolume(Types.Sound.BGM, slider.value);
    }

    public void SetEffectVolume(Slider slider)
    {
        SoundManager.Instance.SetVolume(Types.Sound.Effect, slider.value);
    }

    private IEnumerator ResetSwipeCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f); // 1프레임 대기
        isSwiping = false; // 스와이프 상태 초기화
    }
}
