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
    public float Speed;
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

        var distance = Vector3.Distance(start, dest);
        if (distance != 0)
        {
            SubCrossHairTransform.anchoredPosition = 
                distance <= 0.01f ? dest : Vector3.Lerp(start, dest, Time.deltaTime * Speed);
        }
    }
}
