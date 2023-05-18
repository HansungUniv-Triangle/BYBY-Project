using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIHolder;
using Random = UnityEngine.Random;

namespace Network
{
    // 데이터
    public partial class NetworkManager
    {
        private struct RoomPlayerData : INetworkStruct
        {
            public NetworkString<_16> NickName;
            public NetworkBool IsReady;
            public NetworkBool IsDoneLoadScene;
            public Color PlayerColor;
        }

        public NetworkPlayer PlayerCharacter { get; private set; }

        public NetworkPlayer EnemyCharacter;

        private PlayerRef _enemyRef;
        public PlayerRef EnemyRef
        {
            get
            {
                if (_enemyRef.IsNone)
                {
                    foreach (var runnerActivePlayer in Runner.ActivePlayers)
                    {
                        if (runnerActivePlayer != Runner.LocalPlayer)
                        {
                            _enemyRef = runnerActivePlayer;
                        }
                    }
                }
                
                return _enemyRef;
            }
        }

        [Networked]
        public int Seed { get; set; }
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
        private List<NetworkObject> _networkObjectList;
        
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
        
        private GameUI _gameUI;

        private GameUI GameUI
        {
            get
            {
                if (_gameUI is null)
                {
                    _gameUI = GameManager.Instance.UIHolder as GameUI;
                }

                return _gameUI;
            }
        }

        private SynergyPageManager _synergyPageManager;

        private SynergyPageManager SynergyPageManager
        {
            get
            {
                if (_synergyPageManager is null)
                {
                    _synergyPageManager = GameManager.Instance.SynergyPageManager;
                }
                return _synergyPageManager;
            }
        }
        
        private PlayerRef _winnerRef;
        private RoundState _round = RoundState.None;
        public RoundState GameRoundState => _round;
        private int roundNum = 0;
        
        private void ChangeRound(RoundState roundState)
        {
            switch (roundState)
            {
                case RoundState.None:
                    _round = RoundState.GameStart;
                    break;
                case RoundState.GameStart:
                    _round = RoundState.SynergySelect;
                    break;
                case RoundState.SynergySelect:
                    _round = RoundState.WaitToStart;
                    break;
                case RoundState.WaitToStart:
                    _round = RoundState.RoundStart;
                    break;
                case RoundState.RoundStart:
                    _round = RoundState.RoundEndResult;
                    break;
                case RoundState.RoundEndResult:
                    _round = RoundState.RoundEndAnalysis;
                    break;
                case RoundState.RoundEndAnalysis:
                    _round = RoundState.SynergySelect;
                    break;
                case RoundState.GameEnd:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            StartRound();
        }
        
        private void StartRound()
        {
            switch (_round)
            {
                case RoundState.GameStart:
                    SetTimerSec(5f);
                    break;
                case RoundState.SynergySelect:
                    ViewSynergySelect();
                    SetTimerSec(10f);
                    break;
                case RoundState.WaitToStart:
                    ViewWait();
                    SetTimerSec(5f);
                    break;
                case RoundState.RoundStart:
                    IncreaseRound();
                    SetTimerSec(10f);
                    break;
                case RoundState.RoundEndResult:
                    ViewRoundResult();
                    SetTimerSec(5f);
                    break;
                case RoundState.RoundEndAnalysis:
                    ViewRoundAnalysis();
                    SetTimerSec(10f);
                    break;
                case RoundState.GameEnd: // 게임 나가는 코드 넣어야해
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void FixedUpdate()
        {
            switch (_round)
            {
                case RoundState.None:
                    break;
                case RoundState.GameStart:
                case RoundState.WaitToStart:
                case RoundState.RoundStart:
                case RoundState.RoundEndResult:
                case RoundState.RoundEndAnalysis:
                case RoundState.GameEnd:
                    UpdateTimerInGame();
                    break;
                case RoundState.SynergySelect:
                    UpdateSynergySelect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if(!HasStateAuthority) return;
            
            if (RoundChangeTimer.Expired(Runner))
            {
                RoundChangeTimer = TickTimer.None;
                RPCChangeRound(_round);
            }
        }

        private void UpdateTimerInGame()
        {
            var time = RoundChangeTimer.RemainingTime(Runner) ?? 00;
            var min = time / 60f;
            var sec = time % 60f;
            GameUI.timeText.text = $"{min:00}:{sec:00}";
        }

        private void UpdateSynergySelect()
        {
            SynergyPageManager.SetSynergySelectTimer(RoundChangeTimer.RemainingTime(Runner) ?? 0, 60f);
        }

        private void SetTimerSec(float sec)
        {
            if(!HasStateAuthority) return;
            
            RoundChangeTimer = TickTimer.CreateFromSeconds(Runner, sec);
        }

        private void ViewSynergySelect()
        {
            _gameUI.gameUIGroup.SetActive(false);
            SynergyPageManager.SetActiveSynergyPanel(true);
            SynergyPageManager.MakeSynergyPage();
        }

        private void ViewWait()
        {
            _gameUI.gameUIGroup.SetActive(true);
            SynergyPageManager.SetActiveSynergyPanel(false);
            PlayerCharacter.InitialStatus();
            GameUI.roundText.text = $"시너지 선택 완료! 기다리세요";
        }
        
        private void IncreaseRound()
        {
            roundNum++;
            GameUI.roundText.text = $"ROUND {roundNum}";
        }

        private void ViewRoundResult()
        {
            _winnerRef = Runner.LocalPlayer;
            PlayerCharacter.ConversionBehaviorData();

            if (RoomPlayerList.TryGet(_winnerRef, out var data))
            {
                GameUI.roundText.text = $"{data.NickName}님이 승리하셨습니다!";
            }
        }
        
        private void ViewRoundAnalysis()
        {
            var message = "";
            var playerData = PlayerCharacter.CharacterBehaviorData;
            var enemyData = EnemyCharacter.CharacterBehaviorData;

            message += $"피격 : {playerData.HitRate} vs {enemyData.HitRate} : 피격\n";
            message += $"회피 : {playerData.DodgeRate} vs {enemyData.DodgeRate} : 회피\n";
            message += $"명중 : {playerData.Accuracy} vs {enemyData.Accuracy} : 명중\n";
            message += $"피해 : {playerData.Damage} vs {enemyData.Damage} : 피해\n";
            message += $"특화 : {playerData.Special} vs {enemyData.Special} : 특화\n";
            message += $"파괴 : {playerData.DestroyBullet} vs {enemyData.DestroyBullet} : 파괴\n";
            message += $"장전 : {playerData.Reload} vs {enemyData.Reload} : 장전\n";

            GameUI.roundText.text = message;
            GameManager.Instance.ResetBehaviourEventCount();
        }
        
        private void ViewGameWinner()
        {
            _winnerRef = Runner.LocalPlayer;
            
            if (RoomPlayerList.TryGet(_winnerRef, out var data))
            {
                GameUI.roundText.text = $"{data.NickName}님이 최종 승리하셨습니다!";
            }
        }
    }
    
    public partial class NetworkManager : NetworkBehaviour
    {
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
            _networkObjectList = new List<NetworkObject>();
            Seed = Random.Range(0, 10000);

            RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
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
        
        private bool IsAllPlayerLoadScene()
        {
            foreach (var (_, playerData) in RoomPlayerList)
            {
                if (playerData.IsDoneLoadScene.Equals(false))
                {
                    return false;
                }
            }

            return true;
        }

        private void SpawnPlayerCharacter(PlayerRef playerRef)
        {
            Vector3 spawnPosition = new Vector3((playerRef.RawEncoded % Runner.Config.Simulation.DefaultPlayers) + 20, 30, 10);
            NetworkObject networkPlayerObject = Runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, playerRef);
            PlayerCharacter = networkPlayerObject.GetComponent<NetworkPlayer>();

            var mainWeapon = GameManager.Instance.SelectWeapon;
            NetworkObject mainWeaponSpawn = Runner.Spawn(mainWeapon, spawnPosition + Vector3.right + Vector3.up, Quaternion.identity, playerRef);
            mainWeaponSpawn.transform.SetParent(networkPlayerObject.transform);

            var subWeapon = GameManager.Instance.SelectSubWeapon;
            NetworkObject subWeaponSpawn = Runner.Spawn(subWeapon, spawnPosition + Vector3.up * 2, Quaternion.identity, playerRef);
            subWeaponSpawn.transform.SetParent(networkPlayerObject.transform);
            
            PlayerCharacter.SetGunPos(mainWeaponSpawn.transform);
        }
        
        public void AddBlockHitData(Vector3 pos, int damage)
        {
            PlayerCharacter.AddBlockHitData(pos, 0, damage);
        }
        
        public void AddBlockHitData(Vector3 pos, int radius, int damage)
        {
            PlayerCharacter.AddBlockHitData(pos, radius, damage);
        }
        
        public void AddCharacterHitData(NetworkObject networkObject, int damage)
        {
            PlayerCharacter.AddCharacterHitData(networkObject, damage);
        }

        private void InitialGame()
        {
            WorldManager.Instance.GeneratorMap(Seed);
            SpawnPlayerCharacter(Runner.LocalPlayer);
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

            if (!Runner.IsShutdown)
            {
                RPCLoadSceneCheck(Runner.LocalPlayer);
            }
            
            GameManager.Instance.DeActiveLoadingUI();
        }
        
        public void DisconnectingServer()
        {
            Runner.Shutdown();
            StartCoroutine(LoadAsyncScene(0));
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
                IsDoneLoadScene = false,
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
                    RPCLoadScene();
                }
            }
            else
            {
                throw new Exception("플레이어 준비, 해당 플레이어 리스트에 없음");
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCLoadSceneCheck(PlayerRef playerRef)
        {
            if (RoomPlayerList.TryGet(playerRef, out RoomPlayerData roomPlayerData))
            {
                roomPlayerData.IsDoneLoadScene = true;
                RoomPlayerList.Set(playerRef, roomPlayerData);
            
                if (HasStateAuthority && IsAllPlayerLoadScene())
                {
                    RPCStartGame();
                }
            }
            else
            {
                throw new Exception("플레이어 준비, 해당 플레이어 리스트에 없음");
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPCLoadScene()
        {
            Runner.SessionInfo.IsOpen = false;
            StartCoroutine(LoadAsyncScene(1));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPCStartGame()
        {
            InitialGame();
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCSendDefeat(PlayerRef playerRef)
        {
            foreach (var (key, _) in RoomPlayerList)
            {
                if (key != playerRef)
                {
                    _winnerRef = key;
                    return;
                }
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPCChangeRound(RoundState roundState)
        {
            ChangeRound(roundState);
        }
    }
}