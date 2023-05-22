using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Lobby : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Transform _buttonOrigin;
    private Button[] _buttons = new Button[4];

    [SerializeField]
    private Transform _menuOrigin;
    private RawImage[] _menu = new RawImage[4];

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
        _menu = _menuOrigin.GetComponentsInChildren<RawImage>();
        _buttons = _buttonOrigin.GetComponentsInChildren<Button>();
        _spawnPoint = _spawnPointOrigin.GetComponentsInChildren<RectTransform>();
    }

    void Start()
    {
        ChangeButtons();
    }

    public void BattleButtonClicked()
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

    public void PracticeButtonClicked()
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

    public void SettingsButtonClicked()
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

    public void ExitButtonClicked()
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

    private IEnumerator ResetSwipeCoroutine()
    {

        yield return new WaitForSecondsRealtime(0.5f); // 1프레임 대기

        isSwiping = false; // 스와이프 상태 초기화
    }
}
