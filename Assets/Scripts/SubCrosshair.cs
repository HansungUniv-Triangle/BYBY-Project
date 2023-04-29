using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIHolder;
using UnityEngine;
using UnityEngine.Serialization;

public class SubCrosshair : MonoBehaviour
{
    private RectTransform _subCrossHairTransform;
    private Joystick _joystick;
    
    public int MaxAimingRange = 120;
    public float Speed = 15;
    public bool ReverseMove = true;
    public bool OnlyHorizontal;
    
    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++Speed).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--Speed).ToString(); }
    public void IncreaseMaxMoveOffset(GameObject text)
    {
        text.GetComponent<TextMeshProUGUI>().text = (MaxAimingRange += 10).ToString();
    }
    public void DecreaseMaxMoveOffset(GameObject text)
    {
        text.GetComponent<TextMeshProUGUI>().text = (MaxAimingRange -= 10).ToString();
    }

    public void ToggleReverseMove()
    {
        ReverseMove = !ReverseMove;
    }
    public void ToggleOnlyHorizontal()
    {
        OnlyHorizontal = !OnlyHorizontal;
    }
    #endregion
    
    private void Start()
    {
        _subCrossHairTransform = GetComponent<RectTransform>();
        _joystick = (GameManager.Instance.UIHolder as GameUI)?.joystick;
    }

    private void Update()
    {
        var reverse = ReverseMove ? -1 : 1;
        var h = _joystick.Horizontal * reverse;
        var v = _joystick.Vertical * reverse;
        v = OnlyHorizontal ? 0 : v;
        
        var start = _subCrossHairTransform.anchoredPosition;
        var dest = new Vector3(h * MaxAimingRange, v * MaxAimingRange);

        var distance = Vector3.Distance(start, dest);
        if (distance != 0)
        {
            _subCrossHairTransform.anchoredPosition = 
                distance <= 0.01f ? dest : Vector3.Lerp(start, dest, Time.deltaTime * Speed);
        }
    }
}
