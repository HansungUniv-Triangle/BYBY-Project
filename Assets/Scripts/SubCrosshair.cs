using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class SubCrosshair : MonoBehaviour
{
    public RectTransform SubCrossHairTransform;
    public VariableJoystick Joystick;
    
    public int MaxAimingRange = 120;
    public float Speed = 15;
    public bool ReverseMove;
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
    
    private void Update()
    {
        var reverse = ReverseMove ? -1 : 1;
        var h = Joystick.Horizontal * reverse;
        var v = Joystick.Vertical * reverse;
        v = OnlyHorizontal ? 0 : v;
        
        var start = SubCrossHairTransform.anchoredPosition;
        var dest = new Vector3(h * MaxAimingRange, v * MaxAimingRange);

        var distance = Vector3.Distance(start, dest);
        if (distance != 0)
        {
            SubCrossHairTransform.anchoredPosition = 
                distance <= 0.01f ? dest : Vector3.Lerp(start, dest, Time.deltaTime * Speed);
        }
    }
}
