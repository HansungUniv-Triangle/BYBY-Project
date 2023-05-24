using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Reloading : MonoBehaviour
{
    private GameObject reloadingObject;
    [SerializeField]
    private float reloadingDuration = 3f;
    private float reloadingTimer;
    private TextMeshProUGUI reloadingText;
    private Image reloadingOutline;

    private void Awake()
    {
        reloadingObject = gameObject;
        reloadingText = reloadingObject.GetComponentInChildren<TextMeshProUGUI>();
        reloadingOutline = reloadingObject.GetComponentsInChildren<Image>()[1];
    }

    private void OnEnable()
    {
        reloadingTimer = reloadingDuration;
    }

    private void Update()
    {
        reloadingTimer -= Time.deltaTime;

        if (reloadingTimer <= 0f)
        {
            gameObject.SetActive(false);
        }

        UpdateCountdownText();
        UpdateFillAmount();
    }

    private void UpdateCountdownText()
    {
        string remainningTime = reloadingTimer.ToString("F1") + "S";
        reloadingText.text = remainningTime;
    }

    private void UpdateFillAmount()
    {
        float fillRatio = 1f - (reloadingTimer / reloadingDuration);
        reloadingOutline.fillAmount = fillRatio;
    }
}
