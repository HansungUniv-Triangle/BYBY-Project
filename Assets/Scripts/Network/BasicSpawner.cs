using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        // Network
        private NetworkRunner _runner;
        private NetworkManager _manager;

        private List<NetworkObject> _networkObjectList = new List<NetworkObject>();
        
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
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            
        }
        
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_manager)
            {
                _manager.OnPlayerLeft(player);
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