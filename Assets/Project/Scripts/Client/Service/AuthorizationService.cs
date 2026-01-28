using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Collections;

namespace GameClient
{
    public class AuthorizationService
    {
        private readonly ICustomSerializer _serializer;
        private readonly ServerProvider.ServerConfig _config;
        private readonly LocalDatastore _localDatastore;

        const string DataTokenName = "loginToken";

        public AuthorizationService(ICustomSerializer serializer, ServerProvider.ServerConfig serverConfig, LocalDatastore localDatastore)
        {
            _localDatastore = localDatastore;
            _serializer = serializer;
            _config = serverConfig;
        }
   
        public async Task<LoginResult> LoginAsync(string login, string password, bool createToken)
        {
            string enterToken = GetEnterToken();

            LoginRequest loginRequest = new LoginRequest(login, password, enterToken, createToken);
            byte[] serialize = _serializer.ToByteArray(loginRequest);

            string url = _config.urlAdress + "authorization/login";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(serialize);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                Debug.Log("Login result: " + request.result.ToString());

                if (request.result == UnityWebRequest.Result.Success)
                {
                    LoginResponce loginResult = _serializer.FromByteArray<LoginResponce>(request.downloadHandler.data);
                    
                    if (!string.IsNullOrEmpty(loginResult.loginToken))
                    {
                        FixedString512Bytes tokenFixedString = new FixedString512Bytes(loginResult.loginToken);
                        _localDatastore.SaveData<FixedString512Bytes>(DataTokenName, tokenFixedString);
                    }

                    return new LoginResult(true, loginResult.message, loginResult.loginToken, loginResult.connectToken);
                }
                else
                {
                    return new LoginResult();
                }
            }
        }

        public async Task<SignUpResult> SignUpAsync(string login, string password, string nickname)
        {
            RegisterRequest registerRequest = new RegisterRequest(login, password, nickname);

            byte[] serialize = _serializer.ToByteArray(registerRequest);

            string url = _config.urlAdress + "authorization/register";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(serialize);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                await request.SendWebRequest();

                Debug.Log("SignUp result: " + request.result.ToString() + "/" + request.responseCode);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return SignUpResult.Success;
                }
                else
                {
                    return SignUpResult.Failure;
                }
            }
        }

        public string GetEnterToken()
        {
            if (_localDatastore.DataExist(DataTokenName))
            {
               return _localDatastore.LoadData<FixedString512Bytes>(DataTokenName).ToString();
            }

            return "";
        }

        public void DeleteEnterToken()
        {
            _localDatastore.DeleteData(DataTokenName);
        }

        public struct LoginResult
        {
            public readonly bool Success;
            public readonly string Messange;
            public readonly string LoginToken;
            public readonly string ConnectToken;

            public LoginResult (bool success, string messange, string loginToken, string connectToken)
            {
                Success = success;
                Messange = messange;
                LoginToken = loginToken;
                ConnectToken = connectToken;
            }
        }

        public enum SignUpResult
        {
            Success,
            Failure
        }

        [Serializable]
        public class LoginResponce
        {
            public string message;
            public string loginToken;
            public string connectToken;
        }

        [Serializable]
        public class RegisterRequest 
        {
            public string login;
            public string password;
            public string nickname;

            public RegisterRequest(string login, string password, string nickname)
            {
                this.login = login;
                this.password = password;
                this.nickname = nickname;
            }
        }

        [Serializable]
        public class LoginRequest
        {
            public string login;
            public string password;
            public string loginToken;
            public bool   createToken;

            public LoginRequest(string login, string password, string loginToken, bool createToken)
            {
                this.login = login;
                this.password = password;
                this.loginToken = loginToken;
                this.createToken = createToken;
            }
        }
    }
}
