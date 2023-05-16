using System.Collections;
using System.Collections.Generic;
using Types;
using UnityEngine;

public class CanvasManager
{
    private GameObject _canvas;
    private CanvasController[] _canvasControllers;
    private CanvasType _lastActiveCanvasType;

    public CanvasManager()
    {
        _canvas = GameObject.Find("Canvas");
        _canvasControllers = _canvas.GetComponentsInChildren<CanvasController>(true);
        foreach (var canvas in _canvasControllers)
        {
            canvas.gameObject.SetActive(false);
        }
        _lastActiveCanvasType = CanvasType.None;
    }

    public void SwitchUI(CanvasType canvasType)
    {
        if (_lastActiveCanvasType != CanvasType.None)
        {
            foreach (var canvas in _canvasControllers)
            {
                if (canvas.canvasType == _lastActiveCanvasType)
                    canvas.gameObject.SetActive(false);
            }
        }

        foreach (var canvas in _canvasControllers)
        {
              if (canvas.canvasType == canvasType)
                canvas.gameObject.SetActive(true);
        }

        _lastActiveCanvasType = canvasType;
    }
}
