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
            public NetworkBool IsDoneLoading;
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
            if (_networkObjectList.Count > 50)
            {
                if (_networkObjectList[0].HasStateAuthority)
                {
                    Runner.Despawn(_networkObjectList[0]);
                }
            }
            _networkObjectList.Add(networkObject);
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

    // 라운드 조작 관련
    public partial class NetworkManager
    {
        [Networked] private TickTimer RoundChangeTimer { get; set; }
        
        private PlayerRef _defeatRef;
        private RoundState _round = RoundState.None;
        private int roundNum = 0;

        private enum RoundState
        {
            None,
            SynergySelect, 
            WaitToStart,
            RoundStart,
            RoundEnd,
            GameEnd,
        }

        private void ChangeRound(RoundState roundState)
        {
            switch (roundState)
            {
                case RoundState.None:
                    _round = RoundState.SynergySelect;
                    break;
                case RoundState.SynergySelect:
                    _round = RoundState.WaitToStart;
                    break;
                case RoundState.WaitToStart:
                    _round = RoundState.RoundStart;
                    break;
                case RoundState.RoundStart:
                    _round = RoundState.RoundEnd;
                    break;
                case RoundState.RoundEnd:
                    _round = RoundState.SynergySelect;
                    break;
                case RoundState.GameEnd:
                    // 게임 방 나가는 코드를 넣으쇼
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            StartRound();
        }
        
        private void StartRound()
        {
            switch (_round)
            {
                case RoundState.SynergySelect:
                    ViewSynergySelect();
                    ChangeRoundFiveSec();
                    break;
                case RoundState.WaitToStart:
                    ViewWait();
                    ChangeRoundFiveSec();
                    break;
                case RoundState.RoundStart:
                    IncreaseRound();
                    ChangeRoundFiveSec();
                    break;
                case RoundState.RoundEnd:
                    ViewRoundWinner();
                    ChangeRoundFiveSec();
                    break;
                case RoundState.GameEnd: // 게임 나가는 코드 넣어야해
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void FixedUpdate()
        {
            if(!HasStateAuthority) return;
            
            if (RoundChangeTimer.Expired(Runner))
            {
                RoundChangeTimer = TickTimer.None;
                RPCChangeRound(_round);
            }
        }

        private void ChangeRoundFiveSec()
        {
            if(!HasStateAuthority) return;
            
            RoundChangeTimer = TickTimer.CreateFromSeconds(Runner, 5.0f);
        }

        private void ViewSynergySelect()
        {
            var gameUI = GameManager.Instance.UIHolder as GameUI;
            gameUI.roundText.text = $"시너지 고르는 중 입니다.";
        }

        private void ViewWait()
        {
            var gameUI = GameManager.Instance.UIHolder as GameUI;
            gameUI.roundText.text = $"기다리십쇼";
        }
        
        private void IncreaseRound()
        {
            roundNum++;
            var gameUI = GameManager.Instance.UIHolder as GameUI;
            if (gameUI != null)
            {
                gameUI.roundText.text = $"ROUND {roundNum}";
            } 
        }

        private void ViewRoundWinner()
        {
            _defeatRef = Runner.LocalPlayer;
            foreach (var (key, value) in RoomPlayerList)
            {
                // 임시로 플레이어 이름 나오게 해놨음
                if (key == _defeatRef)
                {
                    var gameUI = GameManager.Instance.UIHolder as GameUI;
                    gameUI.roundText.text = $"{value.NickName}님이 승리하셨습니다!";
                    return;
                }
            }
        }
        
        private void ViewGameWinner()
        {
            _defeatRef = Runner.LocalPlayer;
            foreach (var (key, value) in RoomPlayerList)
            {
                if (key != _defeatRef)
                {
                    var gameUI = GameManager.Instance.UIHolder as GameUI;
                    gameUI.roundText.text = $"{value.NickName}님이 최종 승리하셨습니다!";
                    return;
                }
            }
        }
    }
    
    public partial class NetworkManager : NetworkBehaviour
    {
        [Networked] private TickTimer _timer { get; set; }

        private RoomUI _roomUI;

        // Player
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private readonly Color32 _ready = Color.green;
        private readonly Color32 _notReady = Color.red;
        
        public override void Spawned()
        {
            DontDestroyOnLoad(this);
            GameManager.Instance.SetNetworkManager(this);
            GameManager.Instance.DeActiveLoadingUI();
            _roomUI = GameManager.Instance.UIHolder as RoomUI;

            RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
        }

        // Mono에서는 Runner를 가져올 수 없어서 만든 Adapter 역할
        public void OnReady()
        {
            Debug.Log(Runner);
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

            var mainWeapon = GameManager.Instance.SelectWeapon;
            NetworkObject mainWeaponSpawn = Runner.Spawn(mainWeapon, spawnPosition + Vector3.up, Quaternion.identity, playerRef);
            mainWeaponSpawn.transform.SetParent(networkPlayerObject.transform);

            var subWeapon = GameManager.Instance.SelectSubWeapon;
            NetworkObject subWeaponSpawn = Runner.Spawn(subWeapon, spawnPosition + Vector3.up * 2, Quaternion.identity, playerRef);
            subWeaponSpawn.transform.SetParent(networkPlayerObject.transform);
        
            RPCAddCharacterInPlayerData(playerRef, networkPlayerObject.GetComponent<NetworkPlayer>());
            
            NetworkObject gun = Runner.Spawn(_handGun, spawnPosition + Vector3.right + Vector3.up, Quaternion.identity, playerRef);
            gun.transform.SetParent(networkPlayerObject.transform);

            var networkPlayer = networkPlayerObject.GetComponent<NetworkPlayer>();
            networkPlayer.SetGunPos(gun.transform);
            
            RPCAddCharacterInPlayerData(playerRef, networkPlayer);
        }
        
        public void AddBlockHitData(Vector3 pos, int damage)
        {
            LocalCharacter.AddBlockHitData(pos, 0, damage);
        }
        
        public void AddBlockHitData(Vector3 pos, int radius, int damage)
        {
            LocalCharacter.AddBlockHitData(pos, radius, damage);
        }
        
        public void AddCharacterHitData(NetworkObject networkObject)
        {
            LocalCharacter.AddCharacterHitData(networkObject);
        }

        private void InitialGame()
        {
            WorldManager.Instance.GeneratorMap();
            SpawnPlayerCharacter(Runner.LocalPlayer);
            RPCLoadSceneComplete(Runner.LocalPlayer);
            RPCChangeRound(RoundState.None);
        }

        private IEnumerator LoadAsyncScene(int sceneNum)
        {
            GameManager.Instance.ActiveLoadingUI();
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            InitialGame(); 
            GameManager.Instance.DeActiveLoadingUI();
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
                IsDoneLoading = false,
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
                roomPlayerData.IsDoneLoading = true;
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
            StartCoroutine(LoadAsyncScene(1));
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCSendDefeat(PlayerRef playerRef)
        {
            _defeatRef = playerRef;
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPCChangeRound(RoundState roundState)
        {
            ChangeRound(roundState);
        }
    }
}