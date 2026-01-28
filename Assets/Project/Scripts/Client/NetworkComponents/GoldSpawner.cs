using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameClient
{
    public class GoldSpawner : NetworkBehaviour
    {

        [SerializeField] private int _goldOnField = 8;
        [SerializeField] private float _goldSpawnArea = 13;
        [SerializeField] private GoldBar _goldBarPrefab;

        private List<GoldBar> _spawnedBars;

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
                return;

             _spawnedBars = new List<GoldBar>();
             
            for (int i = 0; i < _goldOnField; i++)
            {
                GoldBar bar = Instantiate(_goldBarPrefab);

                bar.SetPosition(GetRandomPositionOnField());
                bar.OnTake += OnGoldBarTaken;

                _spawnedBars.Add(bar);

                bar.NetworkObject.Spawn();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer)
                return;

            for (int i = 0; i < _spawnedBars.Count; i++)
            {
                GoldBar bar = _spawnedBars[i];

                if (bar != null)
                {
                    bar.OnTake -= OnGoldBarTaken;
                    bar.NetworkObject.Despawn();
                }
            }

            _spawnedBars.Clear();
        }

        private void OnGoldBarTaken(GoldBar bar, PlayerCharacter playerCharacter)
        {
            bar.SetPosition(GetRandomPositionOnField());
        }

        private Vector3 GetRandomPositionOnField()
        {
            return new Vector3(Random.Range(-_goldSpawnArea, _goldSpawnArea), 0, Random.Range(-_goldSpawnArea, _goldSpawnArea));
        }
    }
}
