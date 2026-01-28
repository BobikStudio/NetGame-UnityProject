using Unity.Netcode;
using UnityEngine;

namespace GameClient
{
    public class CameraFollow : NetworkBehaviour
    {

        [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 8, -5);
  
        private Camera _followCamera;

        public void Construct(Camera followCamera)
        {
            _followCamera = followCamera;
        }
 
        private void LateUpdate()
        {
            if (IsOwner)
            {           
                _followCamera.transform.position = transform.position + _cameraOffset;
            }
        }
    }
}
