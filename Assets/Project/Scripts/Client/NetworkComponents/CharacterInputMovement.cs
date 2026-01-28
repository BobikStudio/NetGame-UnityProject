using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace GameClient
{
    public class CharacterInputMovement : NetworkBehaviour
    {

        [SerializeField] private NetworkVariable<float> _movementSpeed = new NetworkVariable<float>(0.1f);
        [SerializeField] private Animator _animator;

        [SerializeField] private float _rotateSpeed = 10;
        [SerializeField] private float _fieldSize = 25;

        private IDeviceInput _deviceInput;
        private Vector2 _movement;

        public void Construct(IDeviceInput deviceInput)
        {
            _deviceInput = deviceInput;
        }

        private void Update()
        {
            if (IsOwner)
            {
                Vector2 movement = new Vector2(_deviceInput.GetHorizontal(), _deviceInput.GetVertical());
                SendInputServerRpc(movement);
            }
        }

        private void FixedUpdate()
        {
            if (IsServer)
            {
                Vector2 direction = _movement.normalized * _movementSpeed.Value;
                Vector3 moveVector = new Vector3(direction.x, 0, direction.y);

                Vector3 movePoint = transform.position + moveVector;
                movePoint = new Vector3(Mathf.Clamp(movePoint.x, -_fieldSize, _fieldSize), 0, Mathf.Clamp(movePoint.z, -_fieldSize, _fieldSize));

                transform.position = movePoint;

                if (moveVector.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(moveVector),
                        _rotateSpeed * Time.fixedDeltaTime
                    );
                }

                _animator.SetFloat("MoveSpeed", moveVector.magnitude);
            }
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        private void SendInputServerRpc(Vector2 input)
        {
            _movement = input;
        }
 
    }
}
