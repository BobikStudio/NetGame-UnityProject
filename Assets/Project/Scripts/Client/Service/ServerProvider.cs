using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameClient
{
    public class ServerProvider : IInitializable
    {
 

        public readonly AuthorizationService Authorization;
        public readonly GameServerService GameServerService;

        public readonly NetworkManager NetworkManager;

        private readonly LocalDatastore _localDatastore;
        private readonly ICustomSerializer _serializer;


        [Inject]
        public ServerProvider(ICustomSerializer customSerializer, LocalDatastore localDatastore, NetworkManager networkManager) 
        {
            _serializer = customSerializer;
            _localDatastore = localDatastore;

            NetworkManager = networkManager;
            

            string configPath;

            #if UNITY_EDITOR
            configPath = Path.Combine(Application.dataPath, "Project", "ServerConfig.json");
            #else
            configPath = Path.Combine(AppContext.BaseDirectory, "ServerConfig.json");
            #endif

            string json = File.ReadAllText(configPath);
            ServerConfig serverConfig = customSerializer.FromJson<ServerConfig>(json);

            Authorization = new AuthorizationService(customSerializer, serverConfig, localDatastore);
            GameServerService = new GameServerService(customSerializer, serverConfig, networkManager);
        }

        public void Initialize()
        {
            Debug.Log("Initialize service");

            GameServerService.Initialize();
        }

        [Serializable]
        public class ServerConfig
        {
            public string urlAdress;
            public string serverToken;

            public ServerConfig(string urlAdress, string serverToken)
            {
                this.urlAdress = urlAdress;
                this.serverToken = serverToken;
            }
        }
    }
}
