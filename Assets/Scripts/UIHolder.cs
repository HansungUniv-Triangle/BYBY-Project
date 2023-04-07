using System;
using UnityEngine;

public abstract class UIHolder : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.SetUICanvasHolder(this);
    }
}