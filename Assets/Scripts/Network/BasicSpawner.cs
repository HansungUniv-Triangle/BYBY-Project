using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
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

        [SerializeField]
        private NetworkPrefabRef NetworkManagerPrefab;
        private NetworkManager _networkManager;

        public int roomNumber = 5123;

        #region Fusion

        public async void StartMultiGame()
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            GameManager.Instance.ActiveLoadingUI();

            await _runner.StartGame(new StartGameArgs()
            {
                SessionName = roomNumber.ToString(),
                GameMode = GameMode.Shared,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            }).ContinueWithOnMainThread(_ => SceneManager.LoadSceneAsync("RoomScene"));
        }

        public async void StartSingleGame()
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            
            GameManager.Instance.ActiveLoadingUI();

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Single,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            }).ContinueWithOnMainThread(_ => SceneManager.LoadSceneAsync("RoomScene"));
        }
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (player.PlayerId == 0)
            {
                var obj = runner.Spawn(NetworkManagerPrefab);
                _networkManager = obj.GetComponent<NetworkManager>();
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            _networkManager ??= FindObjectOfType<NetworkManager>();
            _networkManager.OnPlayerLeft(player);
        }
        
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
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