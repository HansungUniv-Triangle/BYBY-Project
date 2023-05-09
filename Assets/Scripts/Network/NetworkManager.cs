using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UIHolder;

namespace Network
{
    // 구조체
    public partial class NetworkManager
    {
        private struct RoomPlayerData : INetworkStruct
        {
            public NetworkString<_16> NickName;
            public NetworkBool IsReady;
            public NetworkBool DoneLoading;
            public NetworkBehaviourId Character;
            public Color PlayerColor;
        }

        private NetworkPlayer _localCharacter;

        public NetworkPlayer LocalCharacter
        {
            get
            {
                if (_localCharacter is null)
                {
                    if (RoomPlayerList.TryGet(Runner.LocalPlayer, out RoomPlayerData roomPlayerData))
                    {
                        Runner.TryFindBehaviour(roomPlayerData.Character, out NetworkPlayer networkPlayer);
                        _localCharacter = networkPlayer;
                        if (networkPlayer is null)
                        {
                            throw new Exception("networkplayer를 찾을 수 없었음");
                        }
                    }
                    else
                    {
                        throw new Exception("히트 데이터 전송 시, 플레이어 데이터 없음");
                    }
                }

                return _localCharacter;
            }
        }
    }

    // 룸 데이터 기반 UI 업데이트
    public partial class NetworkManager
    {
        [Networked(OnChanged = nameof(UpdateCanvasData)), Capacity(8)]
        private NetworkDictionary<PlayerRef, RoomPlayerData> RoomPlayerList { get; }
        
        public static void UpdateCanvasData(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateCanvasData();
        }
    
        private void UpdateCanvasData()
        {
            if(_roomUI is null) return;
        
            // 임시로 캔버스 지우는 동작
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
        
        private void UpdateRoomItem(int index, string nick, bool ready, Color color)
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
    }

    // 네트워크 오브젝트 보관
    public partial class NetworkManager
    {
        private readonly List<NetworkObject> _networkObjectList = new List<NetworkObject>();
        
        public void AddNetworkObjectInList(NetworkObject networkObject)
        {
            if (_networkObjectList.Count > 10)
            {
                RemoveNetworkObjectInList(_networkObjectList[0]);
            }
            _networkObjectList.Add(networkObject);
        }
        
        public void RemoveNetworkObjectInList(NetworkObject networkObject)
        {
            Runner.Despawn(networkObject);
        }
        
        public void RemoveDeSpawnNetworkObject(NetworkObject networkObject)
        {
            _networkObjectList.Remove(networkObject);
        }
        
        public NetworkObject FindNetworkObject(NetworkId networkId)
        {
            return _networkObjectList.FirstOrDefault(networkObject => networkObject.Id.Equals(networkId));
        }
    }
    
    public partial class NetworkManager : NetworkBehaviour
    {
        [Networked]
        private TickTimer _timer { get; set; }
    
        private RoomUI _roomUI;

        // Player
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private NetworkPrefabRef _handGun;
    
        private readonly Color32 _ready = Color.green;
        private readonly Color32 _notReady = Color.red;

        public override void Spawned()
        {
            DontDestroyOnLoad(this);
            GameManager.Instance.DeActiveLoadingUI();
            _roomUI = GameManager.Instance.UIHolder as RoomUI;
            RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
        }
    
        public override void FixedUpdateNetwork()
        {
            // if (_timer.Expired(Runner))
            // {
            //     _timer = TickTimer.None;
            //     GameObject.Find("테스트입니다").SetActive(false);
            // }
            // else if (_timer.IsRunning)
            // {
            //     GameObject.Find("테스트입니다").GetComponent<TMP_Text>().text = _timer.RemainingTime(Runner).ToString();
            // }
        }
        
        // Mono에서는 Runner를 가져올 수 없어서 만든 Adapter 역할
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

        private bool IsAllPlayerReady()
        {
            // 혼자서도 인게임 들어갈 수 있게 임시 주석
            // if (RoomPlayerList.Count < 2) return false;
        
            foreach (var (_, playerData) in RoomPlayerList)
            {
                if (playerData.IsReady.Equals(false))
                {
                    return false;
                }
            }

            return true;
        }

        private void AllPlayerInGame()
        {
            _timer = TickTimer.CreateFromSeconds(Runner, 5f);
        }

        private void SpawnPlayerCharacter(PlayerRef playerRef)
        {
            //Vector3 spawnPosition = new Vector3((playerRef.RawEncoded % Runner.Config.Simulation.DefaultPlayers) * 3,1,0);
            Vector3 spawnPosition = new Vector3((playerRef.RawEncoded % Runner.Config.Simulation.DefaultPlayers) + 20, 30, 10);
            NetworkObject networkPlayerObject = Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, playerRef);
            
            NetworkObject gun = Runner.Spawn(_handGun, spawnPosition + Vector3.up * 1.8f, Quaternion.identity, playerRef);
            gun.transform.SetParent(networkPlayerObject.transform);

            var networkPlayer = networkPlayerObject.GetComponent<NetworkPlayer>();
            networkPlayer.SetGunPos(gun.transform);
            
            RPCAddCharacterInPlayerData(playerRef, networkPlayer);
        }
        
        public void AddHitData(Vector3 pos, float damage)
        {
            LocalCharacter.AddBlockHitData(pos, damage);
        }

        private void InitialGame()
        {
            SpawnPlayerCharacter(Runner.LocalPlayer);
            RPCLoadSceneComplete(Runner.LocalPlayer);
            GameManager.Instance.DeActiveLoadingUI();
        }

        private IEnumerator LoadYourAsyncScene()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            InitialGame();
        }
    }

    // RPC 메서드 모음
    public partial class NetworkManager
    {
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCAddPlayer(PlayerRef playerRef, NetworkString<_16> nick, Color color)
        {
            if (RoomPlayerList.ContainsKey(playerRef))
            {
                throw new Exception("플레이어 추가, 방 리스트에 해당 플레이어가 이미 존재함.");
            }

            RoomPlayerList.Add(playerRef, new RoomPlayerData {
                NickName = nick,
                IsReady = false,
                DoneLoading = false,
                PlayerColor = color
            });
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCAddCharacterInPlayerData(PlayerRef playerRef, NetworkBehaviourId networkBehaviourId)
        {
            if (RoomPlayerList.TryGet(playerRef, out RoomPlayerData roomPlayerData))
            {
                roomPlayerData.Character = networkBehaviourId;
                RoomPlayerList.Set(playerRef, roomPlayerData);
            }
            else
            {
                throw new Exception("캐릭터 추가 시, 플레이어의 데이터가 존재하지 않음");
            }
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
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCLoadSceneComplete(PlayerRef playerRef)
        {
            if (RoomPlayerList.TryGet(playerRef, out RoomPlayerData roomPlayerData))
            {
                roomPlayerData.DoneLoading = true;
                RoomPlayerList.Set(playerRef, roomPlayerData);
            
                if (HasStateAuthority && IsAllPlayerReady())
                {
                    AllPlayerInGame();
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
    }
}