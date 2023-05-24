using GameStatus;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Types;
using UIHolder;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class SubCrosshair : MonoBehaviour
{
    private RectTransform _subCrossHairTransform;
    private Joystick _joystick;
    private Network.NetworkPlayer _networkPlayer;

    public float MaxAimingRange
    {
        get
        {
            return StatConverter.ConversionStatValue(_networkPlayer.GetCharStat(CharStat.Calm));
        }
    }
    public float Speed = 15;
    public bool ReverseMove = true;
    public bool OnlyHorizontal;
    
    #region UI Settings
    public void IncreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (++Speed).ToString(); }
    public void DecreaseSpeed(GameObject text) { text.GetComponent<TextMeshProUGUI>().text = (--Speed).ToString(); }
    public void IncreaseMaxMoveOffset(GameObject text)
    {
        _networkPlayer.IncreaseCalm();
        text.GetComponent<TextMeshProUGUI>().text = MaxAimingRange.ToString();
    }
    public void DecreaseMaxMoveOffset(GameObject text)
    {
        _networkPlayer.DecreaseCalm();
        text.GetComponent<TextMeshProUGUI>().text = MaxAimingRange.ToString();
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
    
    public void SetNetworkPlayer(Network.NetworkPlayer networkPlayer)
    {
        _networkPlayer = networkPlayer;
    }

    private void Start()
    {
        _subCrossHairTransform = GetComponent<RectTransform>();
        _joystick = (GameManager.Instance.UIHolder as GameUI)?.joystick;
    }

    private void Update()
    {
        if (_networkPlayer)
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
}
