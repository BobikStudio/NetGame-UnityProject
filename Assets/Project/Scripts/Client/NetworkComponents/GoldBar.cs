using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace GameClient
{
    public class GoldBar : NetworkBehaviour
    {
        public Action<GoldBar, PlayerCharacter> OnTake;

        [SerializeField] private NetworkTransform _networkTransform;

        public void SetPosition(Vector3 position)
        {
            if (IsServer)
            {
                _networkTransform.Teleport(position, Quaternion.identity, transform.localScale);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer)
            {
                if (other.attachedRigidbody != null)
                {
                    PlayerCharacter playerCharacter = other.attachedRigidbody.GetComponent<PlayerCharacter>();

                    if (playerCharacter != null)
                    {
                        OnTake.Invoke(this, playerCharacter);
                    }
                }
            }
        }
    }
}
