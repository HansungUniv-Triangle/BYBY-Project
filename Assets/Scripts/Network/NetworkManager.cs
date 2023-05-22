using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        [Networked] public int Seed { get; set; }
        
        private GameUI _gameUI;
        private GameUI GameUIInstance
        {
            get
            {
                if (GameManager.Instance.UIHolder is not GameUI)
                {
                    return null;
                }
                
                if (_gameUI is null)
                {
                    _gameUI = GameManager.Instance.UIHolder as GameUI;
                }

                return _gameUI;
            }
        }
        
        private RoomUI _roomUI;
        private RoomUI RoomUIInstance
        {
            get
            {
                if (GameManager.Instance.UIHolder is not RoomUI)
                {
                    return null;
                }
                
                if (_roomUI is null)
                {
                    _roomUI = GameManager.Instance.UIHolder as RoomUI;
                }

                return _roomUI;
            }
        }

        // Player
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private readonly Color32 _ready = Color.green;
        private readonly Color32 _notReady = Color.red;

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
        
        public bool IsPlayerWin => Runner.LocalPlayer == BattleLogs[^1].Winner;
        public bool CanControlCharacter => SinglePlayMode || GameRoundState == RoundState.RoundStart;
        
        // 카메라
        private PlayerCamera _playerCamera;
        private PlayerCamera PlayerCamera
        {
            get
            {
                if (_playerCamera is not null)
                {
                    return _playerCamera;
                }
                
                if (Camera.main is not null)
                {
                    _playerCamera = Camera.main.GetComponent<PlayerCamera>();
                    return _playerCamera;
                }

                throw new Exception("카메라가 없음");
            }
        }
        
        
    }

    // 룸 데이터 기반 UI 업데이트
    public partial class NetworkManager
    {
        private struct RoomPlayerData : INetworkStruct
        {
            public NetworkString<_16> NickName;
            public NetworkBool IsReady;
            public NetworkBool IsDoneLoadScene;
            public Color PlayerColor;
            public int Score;
        }
        
        [Networked(OnChanged = nameof(UpdateCanvasData)), Capacity(8)]
        private NetworkDictionary<PlayerRef, RoomPlayerData> RoomPlayerList { get; }
        
        public static void UpdateCanvasData(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateCanvasData();
        }
    
        private void UpdateCanvasData()
        {
            if(RoomUIInstance is null) return;

            // 임시로 캔버스 지우는 동작
            RoomUIInstance.text1.text = "-";
            RoomUIInstance.text1.color = Color.black;
            RoomUIInstance.player1Ready.color = _notReady;
            RoomUIInstance.text2.text = "-";
            RoomUIInstance.text2.color = Color.black;
            RoomUIInstance.player2Ready.color = _notReady;
        
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
                    RoomUIInstance.text1.text = nick;
                    RoomUIInstance.text1.color = color;
                    RoomUIInstance.player1Ready.color = ready ? _ready : _notReady;
                    break;
                case 1:
                    RoomUIInstance.text2.text = nick;
                    RoomUIInstance.text2.color = color;
                    RoomUIInstance.player2Ready.color = ready ? _ready : _notReady;
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

    // 게임 승리 관리
    public partial class NetworkManager
    {
        private const int MaxRound = 3;
        private const int WinRound = MaxRound / 2 + 1;

        private struct BattleLog : INetworkStruct
        {
            public int Round;
            public int Winner;
            public int Defeater;
            public NetworkBool IsDraw;
        }
        
        [Networked(OnChanged = nameof(UpdateBattleLog)), Capacity(10)]
        private NetworkLinkedList<BattleLog> BattleLogs { get; }
        
        public static void UpdateBattleLog(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateBattleLog();
        }

        private void UpdateBattleLog()
        {
            if (BattleLogs.Count > 0)
            {
                ViewRoundResult();
            }
        }
    }
    
    // 라운드 조작 관련
    public partial class NetworkManager
    {
        [Networked] private TickTimer RoundChangeTimer { get; set; }
        
        // 상태 관련
        public RoundState GameRoundState => (RoundState)NetworkRoundState;
        
        [Networked(OnChanged = nameof(UpdateRoundState))] 
        private int NetworkRoundState { get; set; }

        public bool SinglePlayMode { get; private set; } = false;

        public static void UpdateRoundState(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateRoundState();
        }

        private void UpdateRoundState()
        {
            InitialRound((RoundState)NetworkRoundState);
        }

        private void ChangeRound()
        {
            if (!HasStateAuthority) return;
            
            RoundState roundState;
            
            switch (GameRoundState)
            {
                case RoundState.None:
                    roundState = RoundState.GameStart;
                    break;
                case RoundState.GameStart:
                    roundState = RoundState.SynergySelect;
                    break;
                case RoundState.SynergySelect:
                    roundState = RoundState.WaitToStart;
                    break;
                case RoundState.WaitToStart:
                    roundState = RoundState.RoundStart;
                    break;
                case RoundState.RoundStart:
                    roundState = RoundState.RoundEnd;
                    break;
                case RoundState.RoundEnd:
                    roundState = RoundState.RoundResult;
                    break;
                case RoundState.RoundResult:
                    roundState = RoundState.RoundAnalysis;
                    break;
                case RoundState.RoundAnalysis:
                    var playerWin = BattleLogs.Count(log => !log.IsDraw && log.Winner == Runner.LocalPlayer);
                    var enemyWin = BattleLogs.Count(log => !log.IsDraw && log.Winner != Runner.LocalPlayer);

                    if (playerWin == WinRound)
                    {
                        RPCEndGame(Runner.LocalPlayer);
                        roundState = RoundState.GameEnd;
                        break;
                    }

                    if (enemyWin == WinRound)
                    {
                        RPCEndGame(EnemyRef);
                        roundState = RoundState.GameEnd;
                        break;
                    }

                    roundState = RoundState.SynergySelect;
                    break;
                case RoundState.GameEnd:
                    roundState = RoundState.None;
                    DisconnectingServer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            NetworkRoundState = (int)roundState;
        }
        
        private void InitialRound(RoundState roundState)
        {
            switch (roundState)
            {
                case RoundState.GameStart:
                    var tempData1 = new NetworkPlayer.BehaviorData
                    {
                        HitRate = 1,
                        DodgeRate = 1,
                        Accuracy = 1,
                        Damage = 1,
                        Special = 1,
                        DestroyBullet = 1,
                        Reload = 1
                    };
                
                    var tempData2 = new NetworkPlayer.BehaviorData
                    {
                        HitRate = 1,
                        DodgeRate = 1,
                        Accuracy = 1,
                        Damage = 1,
                        Special = 1,
                        DestroyBullet = 1,
                        Reload = 1
                    };

                    ActionBehaviourAnalysis(tempData1, tempData2);
                    SetTimerSec(10f);
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
                    ViewRoundStart();
                    SetTimerSec(10f);
                    break;
                case RoundState.RoundEnd:
                    OrganizeRound();
                    SetTimerSec(2f);
                    break;
                case RoundState.RoundResult:
                    SetTimerSec(5f);
                    break;
                case RoundState.RoundAnalysis:
                    ViewRoundAnalysis();
                    SetTimerSec(10f);
                    break;
                case RoundState.GameEnd:
                    SetTimerSec(30f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void FixedUpdate()
        {
            switch (GameRoundState)
            {
                case RoundState.None:
                    break;
                case RoundState.GameStart:
                case RoundState.WaitToStart:
                case RoundState.RoundStart:
                case RoundState.RoundEnd:
                case RoundState.RoundResult:
                case RoundState.RoundAnalysis:
                case RoundState.GameEnd:
                    UpdateTimerInGame();
                    break;
                case RoundState.SynergySelect:
                    UpdateSynergySelect();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (RoundChangeTimer.Expired(Runner) && HasStateAuthority)
            {
                RoundChangeTimer = TickTimer.None;
                ChangeRound();
            }
        }

        private void UpdateTimerInGame()
        {
            var time = RoundChangeTimer.RemainingTime(Runner) ?? 00;
            var min = time / 60f;
            var sec = time % 60f;
            GameUIInstance.timeText.text = $"{min:00}:{sec:00}";
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
        
        public void OrganizeRound()
        {
            PlayerCharacter.ConversionBehaviorData();
            
            if(!HasStateAuthority) return;
            
            PlayerRef winnerRef;
            PlayerRef defeaterRef;
            bool IsDrawTemp;
            
            if (PlayerCharacter is null || EnemyCharacter is null)
            {
                winnerRef = Runner.LocalPlayer;
                defeaterRef = Runner.LocalPlayer;
                IsDrawTemp = false;
            }
            else
            {
                winnerRef = 
                    PlayerCharacter.GetNowHp() > EnemyCharacter.GetNowHp()
                    ? PlayerCharacter.Object.StateAuthority
                    : EnemyCharacter.Object.StateAuthority;
                defeaterRef =
                    PlayerCharacter.GetNowHp() < EnemyCharacter.GetNowHp()
                        ? PlayerCharacter.Object.StateAuthority
                        : EnemyCharacter.Object.StateAuthority;
                IsDrawTemp = (int)PlayerCharacter.GetNowHp() == (int)EnemyCharacter.GetNowHp();
            }
            
            BattleLogs.Add(new BattleLog
            {
                Round = BattleLogs.Count + 1,
                Winner = winnerRef,
                Defeater = defeaterRef,
                IsDraw = IsDrawTemp,
            });
        }
        
        public void EndedRound()
        {
            if(!HasStateAuthority) return;
            ChangeRound();
        }

        /// <summary>
        /// 시너지 선택 할 때 화면
        /// </summary>
        private void ViewSynergySelect()
        {
            PlayerCamera.ChangeCameraMode(CameraMode.None);
            GameUIInstance.gameUIGroup.SetActive(false);
            SynergyPageManager.SetActiveSynergyPanel(true);
            SynergyPageManager.MakeSynergyPage();
        }

        /// <summary>
        /// 시너시 선택 후 게임 시작 전 잠시 대기
        /// </summary>
        private void ViewWait()
        {
            PlayerCamera.ChangeCameraMode(CameraMode.Game);
            GameUIInstance.gameUIGroup.SetActive(true);
            SynergyPageManager.SetActiveSynergyPanel(false);
            GameUIInstance.roundText.text = $"시너지 선택 완료! 기다리세요";
        }
        
        private void ViewRoundStart()
        {
            PlayerCharacter.InitialStatus();
            GameUIInstance.roundText.text = $"ROUND {BattleLogs.Count + 1}";
        }

        private void ViewRoundResult()
        {
            PlayerCamera.ChangeCameraMode(CameraMode.Winner);
            var currentData = BattleLogs[^1];

            if (currentData.IsDraw)
            {
                GameUIInstance.roundText.text = $"비겼습니다!";
            }
            else
            {
                var nickName = RoomPlayerList.Get(currentData.Winner).NickName;
                GameUIInstance.roundText.text = $"{nickName}님이 승리하셨습니다!";
            }

            int playerScore = 0;
            int enemyScore = 0;
            foreach (var battleLog in BattleLogs)
            {
                if (battleLog.IsDraw) continue;
                
                if (battleLog.Winner == Runner.LocalPlayer)
                {
                    playerScore++;
                }
                else
                {
                    enemyScore++;
                }
            }
            
            GameUIInstance.playerScoreText.text = playerScore.ToString();
            GameUIInstance.enemyScoreText.text = enemyScore.ToString();
        }

        private void ViewRoundAnalysis()
        {
            PlayerCamera.ChangeCameraMode(CameraMode.Player);
            PlayerCharacter.Healing(999f);
            
            if (EnemyCharacter is null)
            {
                GameManager.Instance.ResetBehaviourEventCount();
                return;
            }
            
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

            GameUIInstance.roundText.text = message;
            GameManager.Instance.ResetBehaviourEventCount();

            ActionBehaviourAnalysis(playerData, enemyData);
        }
        
        private void ViewGameWinner(PlayerRef winnerRef)
        {
            PlayerCamera.ChangeCameraMode(CameraMode.Player);
            
            if (RoomPlayerList.TryGet(winnerRef, out var data))
            {
                GameUIInstance.roundText.text = $"{data.NickName}님이 최종 승리하셨습니다!";
            }
        }

        private void ActionBehaviourAnalysis(NetworkPlayer.BehaviorData playerData, NetworkPlayer.BehaviorData enemyData)
        {
            var analyzer = GameManager.Instance.PlayerBehaviorAnalyzer;
            
            // 분석기 초기화
            analyzer.ClearStatCorrelation();
            
            // 스탯 연관도 추가, 계산
            analyzer.AddCharStatCorrelation(PlayerCharacter.GetCharBaseStat());
            analyzer.AddWeaponStatCorrelation(PlayerCharacter.GetWeaponBaseStat());
            analyzer.CalculateStatCorrelation();

            // 행동 연관도 추가
            analyzer.AddBehaviourEventCount(BehaviourEvent.피격, playerData.HitRate / (float)enemyData.HitRate);
            analyzer.AddBehaviourEventCount(BehaviourEvent.회피, playerData.DodgeRate / (float)enemyData.DodgeRate);
            analyzer.AddBehaviourEventCount(BehaviourEvent.명중, playerData.Accuracy / (float)enemyData.Accuracy);
            analyzer.AddBehaviourEventCount(BehaviourEvent.피해, playerData.Damage / (float)enemyData.Damage);
            analyzer.AddBehaviourEventCount(BehaviourEvent.특화, playerData.Special / (float)enemyData.Special);
            analyzer.AddBehaviourEventCount(BehaviourEvent.파괴, playerData.DestroyBullet / (float)enemyData.DestroyBullet);
            analyzer.AddBehaviourEventCount(BehaviourEvent.장전, playerData.Reload / (float)enemyData.Reload);

            // 최종 계산
            analyzer.CalculateFinalCorrelation();
        }
    }
    
    public partial class NetworkManager : NetworkBehaviour
    {
        public override void Spawned()
        {
            DontDestroyOnLoad(this);
            GameManager.Instance.SetNetworkManager(this);
            GameManager.Instance.DeActiveLoadingUI();
            SinglePlayMode = false;
            _networkObjectList = new List<NetworkObject>();
            Seed = Random.Range(0, 10000);
            RPCAddPlayer(Runner.LocalPlayer, $"Nick{Random.Range(1,100)}", Random.ColorHSV());
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            DisconnectedGame();
        }

        // Mono에서는 Runner를 가져올 수 없어서 만든 Adapter 역할
        public void OnReady()
        {
            RPCReady(Runner.LocalPlayer);
            if (Runner.ActivePlayers.Count() == 1)
            {
                SinglePlayMode = true;
            }
        }

        public void OnPlayerLeft(PlayerRef playerRef)
        {
            if (RoomPlayerList.ContainsKey(playerRef))
            {
                RoomPlayerList.Remove(playerRef);
                if (PlayerCharacter is not null && EnemyCharacter is not null)
                {
                    DisconnectedGame();
                }
            }
            else
            {
                throw new Exception("플레이어 퇴장, 해당 플레이어 리스트에 없음");
            }
        }

        private void DisconnectedGame()
        {
            DOTween.Sequence()
                .SetAutoKill(false)
                .OnStart(() =>
                {
                    GameManager.Instance.ActiveDisconnectUI();
                })
                .AppendInterval(5f)
                .OnComplete(() =>
                {
                    GameManager.Instance.DeActiveDisconnectUI();
                    FindObjectOfType<NetworkRunner>().Shutdown();
                    SceneManager.LoadSceneAsync(0);
                })
                .Play();
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

            var weaponData = GameManager.Instance.SelectWeapon;
            SpawnWeapon(playerRef, weaponData, spawnPosition, networkPlayerObject.transform);
        }

        private void SpawnWeapon(PlayerRef playerRef, Weapon weaponData, Vector3 spawnPosition, Transform parent)
        {
            var weapon = Runner.Spawn(
                weaponData.weaponPrefabRef, 
                spawnPosition + Vector3.right + Vector3.up, 
                Quaternion.identity, 
                playerRef
            );
            
            weapon.GetComponent<NetworkProjectileHolder>().InitialHolder(weaponData);
            weapon.transform.SetParent(parent);

            if (weaponData.isMainWeapon)
            {
                PlayerCharacter.SetGunPos(weapon.transform);
            }
        }
        
        public void AddBlockHitData(Vector3 pos, int damage)
        {
            PlayerCharacter.AddBlockHitData(pos, 0, damage);
        }
        
        public void AddBlockHitData(Vector3 pos, int radius, int damage)
        {
            PlayerCharacter.AddBlockHitData(pos, radius, damage);
        }
        
        public void AddCharacterHitData(NetworkObject networkObject, int damage, bool isMainWeapon)
        {
            PlayerCharacter.AddCharacterHitData(networkObject, damage, isMainWeapon);
        }

        private void InitialGame()
        {
            NetworkRoundState = 0;
            GameManager.Instance.ResetBehaviourEventCount();
            WorldManager.Instance.GeneratorMap(Seed);
            SpawnPlayerCharacter(Runner.LocalPlayer);

            if (SinglePlayMode)
            {
                ChangeRound();
            }
            else
            {
                PlayerCharacter.InitialStatus();
            }
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
            SceneManager.LoadSceneAsync(0);
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
                PlayerColor = color,
                Score = 0
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
        private void RPCEndGame(PlayerRef playerRef)
        {
            ViewGameWinner(playerRef);
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
    }
}