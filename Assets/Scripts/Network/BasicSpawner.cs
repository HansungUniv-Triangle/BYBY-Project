using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        // Network
        private NetworkRunner _runner;
    
        // Player
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private NetworkPrefabRef _handGun;
        [SerializeField] private NetworkPrefabRef _roomManager;

        private List<NetworkObject> _networkObjectList = new List<NetworkObject>();
        
        //private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0,0,400,80), "싱글"))
                {
                    StartGame(GameMode.Single);
                }
                if (GUI.Button(new Rect(0,80,400,80), "벙글"))
                {
                    StartGame(GameMode.Shared);
                }
            }
        }
        
        #region Fusion
        public async void StartGame(GameMode mode)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            GameManager.Instance.ActiveLoadingUI();

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                Scene = SceneManager.GetActiveScene().buildIndex,
            });
            
            GameManager.Instance.DeActiveLoadingUI();
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.ActivePlayers.Count() == 1)
            {
                runner.Spawn(_roomManager, Vector3.zero, Quaternion.identity);
            }
            
            if (runner.LocalPlayer.Equals(player))
            {
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3,1,0);
                NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                
                NetworkObject gun = runner.Spawn(_handGun, spawnPosition + Vector3.up, Quaternion.identity, player);
                gun.transform.SetParent(networkPlayerObject.transform);
                
                _networkObjectList.Add(networkPlayerObject);
                _networkObjectList.Add(gun);
            }
        }
        
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            var networkRoom = FindObjectOfType<NetworkRoom>();
            if (networkRoom)
            {
                networkRoom.OnPlayerLeft(player);
            }
            else
            {
                throw new Exception("플레이어 종료, 네트워크 룸 찾을 수 없음");
            }
        }
        
        public void OnConnectedToServer(NetworkRunner runner) { }
        
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("shut down player");
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("disconenected from server");
        }
        
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        
        #endregion
    }
}