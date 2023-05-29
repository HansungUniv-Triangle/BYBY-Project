using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    public GameObject nameSetPopup;
    public RectTransform titleLogo;
    public RectTransform touchToStart;
    public Button nickChangeButton;
    public TextMeshProUGUI nickField;
    private Sequence titleLogoScaleSequence;
    private Sequence touchToStartScaleSequence;

    private void Start()
    {
        titleLogoScaleSequence = DOTween.Sequence();
        touchToStartScaleSequence = DOTween.Sequence();

        titleLogoScaleSequence.Append(titleLogo.DOScale(Vector3.one * 1.1f, 1f));
        titleLogoScaleSequence.Append(titleLogo.DOScale(Vector3.one * 1f, 1f));

        touchToStartScaleSequence.Append(touchToStart.DOScale(Vector3.one * 1.1f, 1f));
        touchToStartScaleSequence.Append(touchToStart.DOScale(Vector3.one * 1f, 1f));

        titleLogoScaleSequence.SetLoops(-1, LoopType.Yoyo);
        touchToStartScaleSequence.SetLoops(-1, LoopType.Yoyo);

        titleLogoScaleSequence.Play();
        touchToStartScaleSequence.Play();
        
        nickChangeButton.onClick.AddListener(()=>
        {
            var nick = nickField.text;
            if (nick.Length > 10)
            {
                nick = nick.Substring(0, 10);
            }
            
            ChangeNickname(nick);
        });
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (DBManager.Instance.NickName is "null")
            {
                touchToStartScaleSequence.Kill();
                touchToStart.gameObject.SetActive(false);
                nameSetPopup.SetActive(true);
            }
            else if (DBManager.Instance.NickName is not null)
            {
                touchToStartScaleSequence.Kill();
                touchToStart.gameObject.SetActive(false);
                MoveToLobby();
            }
        }
    }

    private async void ChangeNickname(string nick)
    {
        await DBManager.Instance.ChangeNickname(nick);
        MoveToLobby();
    }

    private void MoveToLobby()
    {
        SceneManager.LoadSceneAsync(1);
    }
}
