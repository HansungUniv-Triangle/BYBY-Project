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

        private int _spawnedWeapon;
    }

    // 룸 데이터 기반 UI 업데이트
    public partial class NetworkManager
    {
        private struct RoomPlayerData : INetworkStruct
        {
            public NetworkString<_16> NickName;
            public NetworkBool IsReady;
            public NetworkBool IsDoneLoadScene;
        }
        
        [Networked(OnChanged = nameof(UpdateCanvasData)), Capacity(8)]
        private NetworkDictionary<PlayerRef, RoomPlayerData> RoomPlayerList { get; }
        
        public static void UpdateCanvasData(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateCanvasData();
        }
    
        public void UpdateCanvasData()
        {
            if(RoomUIInstance is null) return;

            RoomUIInstance.ClearRoom();

            var count = 0;
            foreach (var (_, playerData) in RoomPlayerList)
            {
                RoomUIInstance.UpdateRoomItem(count, playerData.NickName.ToString(), playerData.IsReady);
                count++;
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

        public void DeSpawnAllNetworkObject()
        {
            var list = _networkObjectList.Where(networkObject => networkObject.HasStateAuthority).ToList();
            foreach (var networkObject in list)
            {
                Runner.Despawn(networkObject);
            }
        }
        
        public NetworkObject FindNetworkObject(NetworkId networkId)
        {
            return _networkObjectList.FirstOrDefault(networkObject => networkObject.Id.Equals(networkId));
        }
    }

    // 게임 승리 관리
    public partial class NetworkManager
    {
        private const int MaxRound = 5;
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
        private TickTimer RoundChangeTimer { get; set; }
        public bool SinglePlayMode { get; set; } = false;
        private bool _gameOut = false;
        
        // 상태 관련
        public RoundState GameRoundState => (RoundState)NetworkRoundState;

        [Networked(OnChanged = nameof(UpdateRoundState))]
        private int NetworkRoundState { get; set; } = 0;
        
        public static void UpdateRoundState(Changed<NetworkManager> changed)
        {
            changed.Behaviour.UpdateRoundState();
        }

        private void UpdateRoundState()
        {
            InitialRound(GameRoundState);
        }

        private void ChangeRound()
        {
            if (!HasStateAuthority) return;
         
            RoundChangeTimer = TickTimer.None;
            
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
                    _gameOut = true;
                    roundState = RoundState.None;
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
                case RoundState.None:
                    break;
                case RoundState.GameStart:
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
                    SetTimerSec(3f);
                    break;
                case RoundState.RoundResult:
                    SetTimerSec(3f);
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

            if (RoundChangeTimer.Expired(Runner))
            {
                ChangeRound();
            }
            
            if (_gameOut)
            {
                DisconnectingServer();
            }
        }

        private void UpdateTimerInGame()
        {
            var time = RoundChangeTimer.RemainingTime(Runner) ?? 00;
            var min = time / 60f;
            var sec = time % 60f;
            GameUIInstance.timeText.text = $"{min:00}:{sec:00}";
        }
        
        public void UpdateBullet(float now, float max)
        {
            GameUIInstance.SetBulletUI(now, max);
        }
        
        private void UpdateSynergySelect()
        {
            SynergyPageManager.SetSynergySelectTimer(RoundChangeTimer.RemainingTime(Runner) ?? 0, 60f);
        }

        private void SetTimerSec(float sec)
        {
            RoundChangeTimer = TickTimer.CreateFromSeconds(Runner, sec);
        }
        
        public void OrganizeRound()
        {
            DeSpawnAllNetworkObject();
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
                    PlayerCharacter.GetNowHp() > EnemyCharacter.GetNowHp()
                        ? EnemyCharacter.Object.StateAuthority
                        : PlayerCharacter.Object.StateAuthority;
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
            if (GameRoundState == RoundState.RoundStart)
            {
                ChangeRound();
            }
        }

        /// <summary>
        /// 시너지 선택 할 때 화면
        /// </summary>
        private void ViewSynergySelect()
        {
            GameUIInstance.behaviourObject.SetActive(false);
            GameUIInstance.gameUIGroup.SetActive(false);
            PlayerCamera.ChangeCameraMode(CameraMode.None);
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
            GameUIInstance.roundText.text = $"시너지 선택 완료! 잠시 기다려 주세요.";
            InitialPosition();
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
            var winnerNick = "";
            var defeaterNick = "";

            if (currentData.IsDraw)
            {
                GameUIInstance.roundText.text = $"비겼습니다.";
            }
            else
            {
                GameUIInstance.roundText.text = $"게임이 종료되었습니다!";
                winnerNick = RoomPlayerList.Get(currentData.Winner).NickName.ToString();
                defeaterNick = RoomPlayerList.Get(currentData.Defeater).NickName.ToString();
            }

            int winnerScore = BattleLogs.Count(data => data.Winner == currentData.Winner && data.IsDraw == false);
            int defeaterScore = BattleLogs.Count(data => data.Winner == currentData.Defeater && data.IsDraw == false);
            
            if (currentData.Winner == Runner.LocalPlayer)
            {
                GameUIInstance.SetRoundResult(winnerNick, defeaterNick, winnerScore, defeaterScore);
                GameUIInstance.ActivePlayerRoundWin();
                GameUIInstance.playerScoreText.text = winnerScore.ToString();
                GameUIInstance.enemyScoreText.text = defeaterScore.ToString();
            }
            else
            {
                GameUIInstance.SetRoundResult(defeaterNick, winnerNick, defeaterScore, winnerScore);
                GameUIInstance.ActiveEnemyRoundWin();
                GameUIInstance.playerScoreText.text = defeaterScore.ToString();
                GameUIInstance.enemyScoreText.text = winnerScore.ToString();
            }
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

            var playerData = PlayerCharacter.CharacterBehaviorData;
            var enemyData = EnemyCharacter.CharacterBehaviorData;
            
            GameUIInstance.SetHitAnalysis(playerData.HitRate, enemyData.HitRate);
            GameUIInstance.SetDodgeAnalysis(playerData.DodgeRate, enemyData.DodgeRate);
            GameUIInstance.SetAccAnalysis(playerData.Accuracy, enemyData.Accuracy);
            GameUIInstance.SetDamageAnalysis(playerData.Damage, enemyData.Damage);
            GameUIInstance.SetDestroyAnalysis(playerData.DestroyBullet, enemyData.DestroyBullet);
            GameUIInstance.SetReloadAnalysis(playerData.Reload, enemyData.Reload);
            GameUIInstance.behaviourObject.SetActive(true);
            
            GameManager.Instance.ResetBehaviourEventCount();

            ActionBehaviourAnalysis(playerData, enemyData);
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
            analyzer.AddBehaviourEventCount(BehaviourEvent.피격, playerData.HitRate, enemyData.HitRate);
            analyzer.AddBehaviourEventCount(BehaviourEvent.회피, playerData.DodgeRate, enemyData.DodgeRate);
            analyzer.AddBehaviourEventCount(BehaviourEvent.명중, playerData.Accuracy, enemyData.Accuracy);
            analyzer.AddBehaviourEventCount(BehaviourEvent.피해, playerData.Damage, enemyData.Damage);
            analyzer.AddBehaviourEventCount(BehaviourEvent.특화, playerData.Special, enemyData.Special);
            analyzer.AddBehaviourEventCount(BehaviourEvent.파괴, playerData.DestroyBullet, enemyData.DestroyBullet);
            analyzer.AddBehaviourEventCount(BehaviourEvent.장전, playerData.Reload, enemyData.Reload);

            // 최종 계산
            analyzer.CalculateFinalCorrelation();
        }
        
        private void ViewGameWinner(PlayerRef winnerRef)
        {
            GameUIInstance.behaviourObject.SetActive(false);
            PlayerCamera.ChangeCameraMode(CameraMode.Player);

            var win = "?";
            var defeat = "?";
            var winScore = 0;
            var defeatScore = 0;
            
            foreach (var (key, data) in RoomPlayerList)
            {
                if (key == winnerRef)
                {
                    win = data.NickName.ToString();
                    winScore = BattleLogs.Count(log => log.Winner == key && log.IsDraw == false);
                }
                else
                {
                    defeat = data.NickName.ToString();
                    defeatScore = BattleLogs.Count(log => log.Winner == key && log.IsDraw == false);
                }
            }

            if (winnerRef == Runner.LocalPlayer)
            {
                DBManager.Instance.IncreaseWinData();
                GameUIInstance.ActiveGameWin(win, defeat, winScore, defeatScore);
            }
            else
            {
                DBManager.Instance.IncreaseDefeatData();
                GameUIInstance.ActiveGameDefeat(win, defeat, winScore, defeatScore);
            }
        }

        public void InitialPosition()
        {
            Vector3 spawnPoint;
            
            if (Runner.LocalPlayer.PlayerId == 0)
            {
                spawnPoint = new Vector3(30, 50, 5);
            }
            else if (Runner.LocalPlayer.PlayerId == 1)
            {
                spawnPoint = new Vector3(30, 50, 55);
            }
            else
            {
                spawnPoint = new Vector3(30, 50, 30);
            }
            
            if (Physics.Raycast(spawnPoint, Vector3.down, out var hit, 100f, layerMask: (int)Layer.World))
            {
                PlayerCharacter.transform.position = hit.point + new Vector3(0, 1, 0);
            }
            else
            {
                PlayerCharacter.transform.position = spawnPoint;
            }
        }
    }
    
    public partial class NetworkManager : NetworkBehaviour
    {
        public override void Spawned()
        {
            DontDestroyOnLoad(this);
            GameManager.Instance.SetNetworkManager(this);
            GameManager.Instance.DeActiveLoadingUI();
            SinglePlayMode = (Runner.GameMode == GameMode.Single);
            _networkObjectList = new List<NetworkObject>();
            Seed = Random.Range(0, 10000);
            RPCAddPlayer(Runner.LocalPlayer, DBManager.Instance.NickName);
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
                if (PlayerCharacter is not null && EnemyCharacter is not null && playerRef != Runner.LocalPlayer)
                {
                    DisconnectedGame();
                }
            }
            else
            {
                DisconnectedGame();
                throw new Exception("플레이어 퇴장, 해당 플레이어 리스트에 없음");
            }
        }

        private void DisconnectedGame()
        {
            DOTween.Sequence()
                .OnStart(() =>
                {
                    GameManager.Instance.ActiveDisconnectUI();
                })
                .AppendInterval(5f)
                .OnComplete(() =>
                {
                    GameManager.Instance.DeActiveDisconnectUI();
                    FindObjectOfType<NetworkRunner>().Shutdown();
                    SceneManager.LoadSceneAsync(1);
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
        
        private void SpawnPlayerCharacter()
        {
            NetworkObject networkPlayerObject = Runner.Spawn(_playerPrefab, new Vector3(30, 30, 30), Quaternion.identity, Runner.LocalPlayer);
            PlayerCharacter = networkPlayerObject.GetComponent<NetworkPlayer>();
            SpawnWeapon(GameManager.Instance.SelectWeapon);
        }

        public void SpawnWeapon(Weapon weaponData)
        {
            Vector3 position;

            switch (_spawnedWeapon)
            {
                case 0:
                    position = Vector3.right;
                    break;
                case 1:
                    position = new Vector3(-1.5f, 1f, -1.5f);
                    break;
                case 2: 
                    position = new Vector3(0, 1f, -1.5f);
                    break;
                case 3: 
                    position = new Vector3(1.5f, 1f, -1.5f);
                    break;
                case 4: 
                    position = new Vector3(-1.5f, 0, -1.5f);
                    break;
                case 5: 
                    position = new Vector3(1.5f, 0, -1.5f);
                    break;
                default:
                    position = new Vector3(0, 0, -1.5f);
                    break;
            }

            var weapon = Runner.Spawn(
                weaponData.weaponPrefabRef,
                PlayerCharacter.transform.position,
                Quaternion.identity, 
                Runner.LocalPlayer
            );
            
            weapon.GetComponent<NetworkProjectileHolder>().InitialHolder(weaponData);
            weapon.transform.SetParent(PlayerCharacter.transform);
            weapon.transform.localPosition = position;
            _spawnedWeapon++;
            
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
            _spawnedWeapon = 0;
            GameManager.Instance.ResetBehaviourEventCount();
            WorldManager.Instance.GeneratorMap(Seed);
            SpawnPlayerCharacter();
            
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
            
            if (SinglePlayMode)
            {
                PlayerCharacter.InitialStatus();
            }
            else
            {
                ActionBehaviourAnalysis(tempData1, tempData2);
                if (RoomPlayerList.TryGet(Runner.LocalPlayer, out var playerData))
                {
                    GameUIInstance.playerNickText.text = playerData.NickName.ToString();
                }
            
                if (RoomPlayerList.TryGet(EnemyRef, out var enemyData))
                {
                    GameUIInstance.playerNickText.text = enemyData.NickName.ToString();
                }
                
                ChangeRound();
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
            SceneManager.LoadSceneAsync(1);
        }
    }

    // RPC 메서드 모음
    public partial class NetworkManager
    {
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPCAddPlayer(PlayerRef playerRef, NetworkString<_16> nick)
        {
            if (RoomPlayerList.ContainsKey(playerRef))
            {
                throw new Exception("플레이어 추가, 방 리스트에 해당 플레이어가 이미 존재함.");
            }

            RoomPlayerList.Add(playerRef, new RoomPlayerData {
                NickName = nick,
                IsReady = false,
                IsDoneLoadScene = false,
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
            StartCoroutine(LoadAsyncScene(3));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPCStartGame()
        {
            InitialGame();
        }
    }
}