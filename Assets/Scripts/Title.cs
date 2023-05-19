using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Title : MonoBehaviour
{
    public GameObject nicknameSetPanel;
    public GameObject settingsPanel;
    public GameObject titleButtons;
    public RectTransform titleLogo;
    public RectTransform touchToStart;
    private Sequence titleLogoScaleSequence;
    private Sequence touchToStartScaleSequence;

    void Start()
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
        titleButtons = transform.GetChild(0).GetChild(7).gameObject;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchToStartScaleSequence.Kill();
            touchToStart.gameObject.SetActive(false);
            titleButtons.SetActive(true);
        }
    }

    public void ShowSettingsPanel()
    {
        titleButtons.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowNickSetPanel()
    {
        titleButtons.SetActive(false);
        nicknameSetPanel.SetActive(true);
    }

    public void BackToTitle()
    {
        settingsPanel.SetActive(false);
        nicknameSetPanel.SetActive(false);
        titleButtons.SetActive(true);
    }
}
