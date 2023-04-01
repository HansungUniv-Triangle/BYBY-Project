using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SubCrosshair : MonoBehaviour
{
    public RectTransform SubCrossHairTransform;
    public VariableJoystick Joystick;
    
    public int MaxMoveOffset;
    public int Speed;
    public bool ReverseMove;
    public bool OnlyHorizontal;
    private void Update()
    {
        var reverse = ReverseMove ? -1 : 1;
        var h = Joystick.Horizontal * reverse;
        var v = Joystick.Vertical * reverse;
        v = OnlyHorizontal ? 0 : v;
        
        var start = SubCrossHairTransform.anchoredPosition;
        var dest = new Vector3(h * MaxMoveOffset, v * MaxMoveOffset);
        
        SubCrossHairTransform.anchoredPosition = 
            Vector3.Lerp(start, dest, Time.deltaTime * Speed);
    }
}
