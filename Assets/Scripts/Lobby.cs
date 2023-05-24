using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class Lobby : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Transform _buttonOrigin;
    private Button[] _buttons = new Button[4];

    [SerializeField]
    private Transform _menuOrigin;
    private Transform[] _menu = new Transform[4];

    private GameObject _rankingPopup;
    private GameObject _settingsPopup;


    public GameObject _tab_NumberOfWin;
    public GameObject _tab_Odds;
    public GameObject _tab_WinningStreak;

    [SerializeField]
    private Transform _spawnPointOrigin;
    private RectTransform[] _spawnPoint;

    public int CurrentPage { get; private set; }

    [SerializeField] private float swipeThreshold = 100f;
    [SerializeField] private float swipeDurationThreshold = 0.3f;

    private Vector2 swipeStartPos;
    private float swipeStartTime;
    private bool isSwiping = false;

    private void Awake()
    {
        CurrentPage = 0;
        for(int i = 0; i < _menuOrigin.childCount; i++)
        {
            _menu[i] = _menuOrigin.GetChild(i);
        }
        _buttons = _buttonOrigin.GetComponentsInChildren<Button>();
        _spawnPoint = _spawnPointOrigin.GetComponentsInChildren<RectTransform>();
        _rankingPopup = transform.GetChild(6).gameObject;
        _settingsPopup = transform.GetChild(7).gameObject;
    }

    void Start()
    {
        ChangeButtons();
    }

    public void TabClicked()
    {
        string tabName = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        switch (tabName)
        {
            case "�¸� ��":
                {
                    _tab_NumberOfWin.SetActive(true);
                    _tab_Odds.SetActive(false);
                    _tab_WinningStreak.SetActive(false);
                    break;
                }
            case "�·�":
                {
                    _tab_NumberOfWin.SetActive(false);
                    _tab_Odds.SetActive(true);
                    _tab_WinningStreak.SetActive(false);
                    break;
                }
            case "����":
                {
                    _tab_NumberOfWin.SetActive(false);
                    _tab_Odds.SetActive(false);
                    _tab_WinningStreak.SetActive(true);
                    break;
                }
        }
    }

    public void PlayButtonClicked_Battle()
    {
        SceneManager.LoadScene("RoomScene");
    }

    public void PlayButtonClicked_Practice()
    {
        GameManager.Instance.NetworkManager.SinglePlayMode = true;
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
        if (!isSwiping) // �������� ���� �ƴ� ���� ����
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

            // �������� �Ÿ��� �ð� ���
            if (swipeDelta.magnitude > swipeThreshold && swipeDuration < swipeDurationThreshold)
            {
                swipeDelta.Normalize();

                // �������� ���⿡ ���� ����� �ڵ� �ۼ�
                if (swipeDelta.y < 0f)
                {
                    if (swipeDelta.x < 0f)
                    {
                        NextMenu();
                        // �������� ����������
                    }
                    else if (swipeDelta.x > 0f)
                    {
                        BeforeMenu();
                        // ���������� ����������
                    }
                   
                    // �Ʒ������� ����������

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

            // �������� �ʱ�ȭ
            swipeStartPos = Vector2.zero;
            swipeStartTime = 0f;
            StartCoroutine(ResetSwipeCoroutine());
        }
    }

    private IEnumerator ResetSwipeCoroutine()
    {

        yield return new WaitForSecondsRealtime(0.5f); // 1������ ���

        isSwiping = false; // �������� ���� �ʱ�ȭ
    }
}
