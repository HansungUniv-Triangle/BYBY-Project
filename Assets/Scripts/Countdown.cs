using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    private GameObject countdownObject;
    [SerializeField]
    private float countdownDuration = 3f;
    private float countdownTimer;
    private TextMeshProUGUI countdownText;

    // Start is called before the first frame update
    private void Awake()
    {
        countdownObject = gameObject;
        countdownText = countdownObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        countdownTimer = countdownDuration;
        UpdateCountdownText();
        InvokeRepeating("UpdateCountdown", 1f, 1f);
    }

    private void UpdateCountdown()
    {
        countdownTimer -= 1f;
        UpdateCountdownText();

        if (countdownTimer <= 0f)
        {
            gameObject.SetActive(false);
        }
    }

    private void UpdateCountdownText()
    {
        countdownText.text = countdownTimer.ToString();
    }
}
