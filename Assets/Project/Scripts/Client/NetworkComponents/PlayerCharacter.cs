using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameClient
{
    public class PlayerCharacter : NetworkBehaviour
    {
 
        private CharacterInputMovement _characterInputMovement;
        private CameraFollow _cameraFollow;

        private IDeviceInput _deviceInput;

        [SerializeField] private TMP_Text _nicknameDisplay;

        public NetworkVariable<FixedString128Bytes> PlayerName = new NetworkVariable<FixedString128Bytes>();
        public NetworkVariable<int> CoinCollected = new NetworkVariable<int>();

        private void Initialization()
        {
            _deviceInput = new DesktopDeviceInput();

            _characterInputMovement = GetComponent<CharacterInputMovement>();
            _cameraFollow = GetComponent<CameraFollow>();

            _characterInputMovement.Construct(_deviceInput);
            _cameraFollow.Construct(Camera.main);

            PlayerName.OnValueChanged += delegate { RefreshDisplay(); };
            CoinCollected.OnValueChanged += delegate { RefreshDisplay(); };
        }

        public override void OnNetworkSpawn()
        {
            Initialization();
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            _nicknameDisplay.text = PlayerName.Value.ToString();
        }
 
    }
}
