using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace GameClient
{
    public class AuthorizationWindow : MonoBehaviour
    {
        [SerializeField] private RectTransform _window;

        [SerializeField] private TMP_InputField _loginInputField;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _nicknameInputField;

        [SerializeField] private Toggle _rememberToggle;

        [SerializeField] private Button _enterButton;
        [SerializeField] private Button _signUpButton;

        [SerializeField] private TMP_Text _signUpText;
        [SerializeField] private TMP_Text _feedbackText;

        private bool _signUp = false;
        private bool _waitFeedback = false;

        private ServerProvider _serverProvider;

        [Inject]
        public void Construct(ServerProvider serverProvider)
        {
            _serverProvider = serverProvider;

            _feedbackText.text = "";

            _enterButton.onClick.AddListener(OnEnterButton);
            _signUpButton.onClick.AddListener(OnSignUpButton);

            _signUp = false;
            _waitFeedback = false;

            RefreshWindow();
        }

        private void OnEnterButton()
        {
            EnterGame();
        }

        private async void EnterGame()
        {
            if (_waitFeedback)
                return;

            _waitFeedback = true;

            _feedbackText.text = "";

            if (_signUp)
            {
                string login = _loginInputField.text;
                string password = _passwordInputField.text;
                string nickname = _nicknameInputField.text;

                AuthorizationService.SignUpResult signUpResult = await _serverProvider.Authorization.SignUpAsync(login, password, nickname);

                _feedbackText.text = signUpResult.ToString();
            }
            else
            {
                string login = _loginInputField.text;
                string password = _passwordInputField.text;
                bool createToken = _rememberToggle.isOn;

                AuthorizationService.LoginResult loginResult = await _serverProvider.Authorization.LoginAsync(login, password, createToken);

                _feedbackText.text = loginResult.ToString();

                if (loginResult.Success)
                {
                    _serverProvider.GameServerService.ConnectGameServer(loginResult.ConnectToken);
                }
            }

            _waitFeedback = false;
        }

        private void OnSignUpButton()
        {
            _signUp = !_signUp;
            RefreshWindow();
        }

        private void RefreshWindow()
        {
            _nicknameInputField.gameObject.SetActive(_signUp);
            _rememberToggle.gameObject.SetActive(!_signUp);

            if (_signUp)
            {
                _signUpText.text = "Already have an account? To Login";
            }
            else
            {
                _signUpText.text = "Dont have an account? To Sign Up";
            }
        }
    }
}
