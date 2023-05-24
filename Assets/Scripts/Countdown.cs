using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Countdown : MonoBehaviour
{
    private GameObject countdownObject;
    [SerializeField]
    private float disableDelay = 3f;
    private bool isActive = false;
    private Coroutine disableCoroutine;
    private GameObject CountText;

    // Start is called before the first frame update
    private void Awake()
    {
        countdownObject = gameObject;
        CountText = countdownObject.GetComponentInChildren<TextMeshProUGUI>().gameObject;
    }

    private void Start()
    {
        countdownObject.SetActive(isActive);      
    }

    private void OnEnable()
    {
        isActive = true;

       
    }
}
