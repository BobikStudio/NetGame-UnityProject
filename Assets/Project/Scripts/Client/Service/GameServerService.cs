using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Multiplayer.Playmode;
using System.Linq;
using Zenject;
using System.Collections.Generic;

namespace GameClient
{
    public class GameServerService 
    {
        private readonly ICustomSerializer _serializer;
        private readonly ServerProvider.ServerConfig _config;
        private readonly NetworkManager _networkManager;

        private Dictionary<ulong, PlayerData> _playersData;

        public GameServerService(ICustomSerializer serializer, ServerProvider.ServerConfig serverConfig, NetworkManager networkManager)
        {
            _serializer = serializer;
            _config = serverConfig;
            _networkManager = networkManager;

            _playersData = new Dictionary<ulong, PlayerData>();
        }

        public void Initialize()
        {
            bool isServer = Application.isBatchMode || CurrentPlayer.ReadOnlyTags().Any(t => t == "Server");

            if (isServer)
            {
                _networkManager.ConnectionApprovalCallback += ApprovalCheck;

                _networkManager.OnClientConnectedCallback += OnClientConnected;
                _networkManager.OnClientDisconnectCallback += OnClientDisconnected;

                StartServer();
            }
        }

        public void DisconnectGameServer()
        {
            _networkManager.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScreen");
        }

        public void ConnectGameServer(string connectToken)
        {
            ConnectPayload connectPayload = new ConnectPayload(connectToken);

            _networkManager.NetworkConfig.ConnectionData = _serializer.ToByteArray(connectPayload);
            _networkManager.StartClient();
        }

        public void StartServer()
        {
            if (_networkManager.StartServer())
            {
                _networkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            NetworkObject playerObject = _networkManager.ConnectedClients[clientId].PlayerObject;

            PlayerCharacter playerCharacter = playerObject.GetComponent<PlayerCharacter>();

            PlayerData playerData = _playersData[clientId];

            playerCharacter.PlayerName.Value = playerData.nickname;
            playerCharacter.CoinCollected.Value = playerData.coins;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _playersData.Remove(clientId);
        }

        public async void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            ConnectPayload connectPayload = _serializer.FromByteArray<ConnectPayload>(request.Payload);

            if (connectPayload != null)
            {
                response.Pending = true;
                PlayerData result = await CheckConnectTokenAsync(connectPayload.clientToken);

                if (result != null)
                {
                    _playersData.Add(request.ClientNetworkId, result);

                    response.CreatePlayerObject = true;
                    response.Approved = true;
                }
 
                response.Pending = false;
            }
        }

        public async Task<PlayerData> CheckConnectTokenAsync(string clientToken)
        {
            ConnectGameServerRequest loginRequest = new ConnectGameServerRequest(clientToken);
            byte[] serialize = _serializer.ToByteArray(loginRequest);

            string url = _config.urlAdress + "gameserver/connect";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(serialize);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                Debug.Log("Connect game server result: " + request.result.ToString());

                if (request.result == UnityWebRequest.Result.Success)
                {
                    PlayerData playerData = _serializer.FromByteArray<PlayerData>(request.downloadHandler.data);
                    return playerData;
                } 
                else
                {
                    return null;
                }
            }
        }

 


        [Serializable]
        public class PlayerData
        {
            public string nickname;
            public int coins;
        }

        [Serializable]
        public class ConnectPayload
        {
            public string clientToken;
            public ConnectPayload(string clientToken)
            {
                this.clientToken = clientToken;
            }
        }

        [Serializable]
        public class ConnectGameServerRequest 
        {
            public string clientToken;

            public ConnectGameServerRequest(string clientToken)
            {
                this.clientToken = clientToken;
            }
        }

    }
}
