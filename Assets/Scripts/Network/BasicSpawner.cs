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
        private bool _mouseButton0;
        private FixedJoystick _fixedJoystick;
        
        // Network
        private NetworkRunner _runner;
    
        // Player
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private NetworkPrefabRef _handGun;
        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

        private void Start()
        {
            _fixedJoystick = GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>();
        }

        private void Update()
        {
            _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        }

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0,0,200,40), "Host"))
                {
                    StartGame(GameMode.Shared);
                }
                if (GUI.Button(new Rect(0,40,200,40), "Join"))
                {
                    StartGame(GameMode.Shared);
                }
            }
        }

        #region Fusion

        async void StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = SceneManager.GetActiveScene().buildIndex,
            });
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player.PlayerId} 입장!");
        }
    
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }
    
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            data.horizontal = _fixedJoystick.Horizontal;
            data.vertical = _fixedJoystick.Vertical;

            if (_mouseButton0)
                data.buttons |= NetworkInputData.MOUSEBUTTON1;
            
            _mouseButton0 = false;
            input.Set(data);
        }
    
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner)
        {
            var player = runner.LocalPlayer;
            
            Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.DefaultPlayers) * 3,1,0);
            NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            NetworkObject gun = runner.Spawn(_handGun, spawnPosition + Vector3.up, Quaternion.identity, player);
            gun.transform.SetParent(networkPlayerObject.transform);
            networkPlayerObject.GetComponent<NetworkPlayer>().InitialWeapon();
            
            _spawnedCharacters.Add(player, networkPlayerObject);
        }
        
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
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