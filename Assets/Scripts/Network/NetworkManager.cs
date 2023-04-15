using System;
using System.Collections;
using DG.Tweening.Core;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
            public Color PlayerColor;
        }
    
        private struct DestroyBlockData : INetworkStruct
        {
            public int Tick { get; set; }
            [Networked] public Vector3 BlockPos { get; set; }
            [Networked] public float Damage { get; set; }
        }
    }

    // 블록 지우기
    public partial class NetworkManager
    {
        [Networked(OnChanged = nameof(UpdateDestroyBlock)), Capacity(50)] 
        private NetworkLinkedList<DestroyBlockData> HitList1 { get; }
        
        [Networked(OnChanged = nameof(UpdateDestroyBlock)), Capacity(50)] 
        private NetworkLinkedList<DestroyBlockData> HitList2 { get; }
        
        private int _lastTick = 0;
        
        public static void UpdateDestroyBlock(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateDestroyBlock();
        }

        private void UpdateDestroyBlock()
        {
            if (Runner.GetPlayerActorId(Runner.LocalPlayer) == 1)
            {
                int tick = 0;
                for (var i = 0; i < HitList2.Count; i++)
                {
                    var data = HitList2.Get(i);
                    tick = data.Tick;
                    if(_lastTick < data.Tick)
                        WorldManager.Instance.GetWorld().HitBlock(data.BlockPos, (int)data.Damage);
                }

                _lastTick = tick;
            }
            else if (Runner.GetPlayerActorId(Runner.LocalPlayer) == 2)
            {
                int tick = 0;
                for (var i = 0; i < HitList1.Count; i++)
                {
                    var data = HitList1.Get(i);
                    tick = data.Tick;
                    if(_lastTick < data.Tick)
                        WorldManager.Instance.GetWorld().HitBlock(data.BlockPos, (int)data.Damage);
                }

                _lastTick = tick;
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
            _roomUI.readyButton.onClick.AddListener(OnReady);
            RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
        }
    
        public override void FixedUpdateNetwork()
        {
            Debug.Log(Runner.GetPlayerActorId(Runner.LocalPlayer));
            
            if (_timer.Expired(Runner))
            {
                _timer = TickTimer.None;
                GameObject.Find("테스트입니다").SetActive(false);
            }
            else if (_timer.IsRunning)
            {
                GameObject.Find("테스트입니다").GetComponent<TMP_Text>().text = _timer.RemainingTime(Runner).ToString();
            }
        }
        
        // Mono에서는 Runner를 가져올 수 없어서 만든 Adapter 역할
        public void OnReady()
        {
            RPCReady(Runner.LocalPlayer);
        }
        
        // Mono에서는 Runner를 가져올 수 없어서 만든 Adapter 역할
        public void SendHitData(Vector3 pos, float damage)
        {
            RPCSendHitData(Runner.LocalPlayer, pos, damage);
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
            
            NetworkObject gun = Runner.Spawn(_handGun, spawnPosition + Vector3.up, Quaternion.identity, playerRef);
            gun.transform.SetParent(networkPlayerObject.transform);
        
            // _networkObjectList.Add(networkPlayerObject);
            // _networkObjectList.Add(gun);
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
        private void RPCSendHitData(PlayerRef playerRef, Vector3 pos, float damage)
        {
            if (Runner.GetPlayerActorId(playerRef) == 1)
            {
                if (HitList1.Count == HitList1.Capacity)
                {
                    HitList1.Remove(HitList1.Get(0));
                }

                HitList1.Add(new DestroyBlockData
                {
                    Tick = Runner.Tick,
                    BlockPos = pos,
                    Damage = damage
                });
            }
            else if (Runner.GetPlayerActorId(playerRef) == 2)
            {
                if (HitList2.Count == HitList2.Capacity)
                {
                    HitList2.Remove(HitList2.Get(0));
                }
                
                HitList2.Add(new DestroyBlockData
                {
                    Tick = Runner.Tick,
                    BlockPos = pos,
                    Damage = damage
                });
            }
        }
        
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