using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedBlockAnimationEvent : MonoBehaviour
{ 
    private Action _endEvent;
    public void SetEndEvent(Action endEvent)
    {
        _endEvent = endEvent;
    }
    public void EndEvent()
    {
        _endEvent();
    }
}
