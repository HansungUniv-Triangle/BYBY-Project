using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using JetBrains.Annotations;
using Network;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;
using Random = UnityEngine.Random;

public class NetworkRoom : NetworkBehaviour
{
    public struct RoomPlayerData : INetworkStruct
    {
        public NetworkString<_16> NickName;
        public NetworkBool IsReady;
        public Color PlayerColor;
    }
    
    [Networked(OnChanged = nameof(UpdateCanvasData)), Capacity(8)]
    public NetworkDictionary<PlayerRef, RoomPlayerData> RoomPlayerList { get; }

    private RoomUI _roomUI;

    private readonly Color32 _ready = Color.green;
    private readonly Color32 _notReady = Color.red;

    private void Start()
    {
        _roomUI = GameManager.Instance.UIHolder as RoomUI;
        _roomUI.readyButton.onClick.AddListener(OnReady);
        RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
    }

    public static void UpdateCanvasData(Changed<NetworkRoom> changed)
    {
        changed.Behaviour.UpdateCanvasData();
    }
    
    public void UpdateCanvasData()
    {
        if(_roomUI is null) return;
        
        // 임시로 캔버스 지우는 동작임
        _roomUI.text1.text = "-";
        _roomUI.text1.color = Color.black;
        _roomUI.player1Ready.color = _notReady;
        _roomUI.text2.text = "-";
        _roomUI.text2.color = Color.black;
        _roomUI.player2Ready.color = _notReady;
        
        var count = 0;
        foreach (var (_, playerData) in RoomPlayerList)
        {
            UpdateRoomItem(count, playerData.NickName.ToString(), playerData.IsReady, playerData.PlayerColor);
            count++;
        }
    }

    public void UpdateRoomItem(int index, string nick, bool ready, Color color)
    {
        switch (index)
        {
            case 0:
                _roomUI.text1.text = nick;
                _roomUI.text1.color = color;
                _roomUI.player1Ready.color = ready ? _ready : _notReady;
                break;
            case 1:
                _roomUI.text2.text = nick;
                _roomUI.text2.color = color;
                _roomUI.player2Ready.color = ready ? _ready : _notReady;
                break;
        }
    }
    
    public void OnReady()
    {
        RPCReady(Runner.LocalPlayer);
    }
    
    
    public void OnPlayerLeft(PlayerRef playerRef)
    {
        if (RoomPlayerList.ContainsKey(playerRef))
        {
            RoomPlayerList.Remove(playerRef);
        }
        else
        {
            throw new Exception("플레이어 준비, 해당 플레이어 리스트에 없음");
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCAddPlayer(PlayerRef playerRef, NetworkString<_16> nick, Color color)
    {
        if (RoomPlayerList.ContainsKey(playerRef))
        {
            throw new Exception("플레이어 추가, 방 리스트에 해당 플레이어가 이미 존재함.");
        }

        RoomPlayerList.Add(playerRef, new RoomPlayerData {
            NickName = nick,
            IsReady = false,
            PlayerColor = color
        });
    }
    
    private bool IsAllPlayerReady()
    {
        if (RoomPlayerList.Count < 2) return false;
        
        foreach (var (_, playerData) in RoomPlayerList)
        {
            if (playerData.IsReady.Equals(false))
            {
                return false;
            }
        }

        return true;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCReady(PlayerRef playerRef)
    {
        if (RoomPlayerList.TryGet(playerRef, out RoomPlayerData roomPlayerData))
        {
            roomPlayerData.IsReady = !roomPlayerData.IsReady;
            RoomPlayerList.Set(playerRef, roomPlayerData);

            if (HasStateAuthority && IsAllPlayerReady())
            {
                RPCStart();
            }
        }
        else
        {
            throw new Exception("플레이어 준비, 해당 플레이어 리스트에 없음");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCStart()
    {
        Runner.SessionInfo.IsOpen = false;
        GameManager.Instance.ActiveLoadingUI();
        StartCoroutine(LoadYourAsyncScene());
    }
    
    IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        GameManager.Instance.DeActiveLoadingUI();
    }
}
