#if uConstruct_ForgeNetworking

using UnityEngine;
using System.Collections;

using BeardedManStudios.Forge;
using BeardedManStudios.Network;

namespace uConstruct.Extensions.ForgeExtension
{
    public class PlayerInstantiater : MonoBehaviour
    {
        public GameObject Player;

        private void Start()
        {
            if(Player == null)
            {
                Debug.LogError("Player not assigned to PlayerInstantiater!!");
                this.enabled = false;

                return;
            }

            Transform randSpawnPoint = spawnPoint;

            if (randSpawnPoint)
            {
                if (NetworkingManager.Socket == null || NetworkingManager.Socket.Connected)
                    Networking.Instantiate(Player, randSpawnPoint.position, randSpawnPoint.rotation, NetworkReceivers.AllBuffered);
                else
                {
                    NetworkingManager.Instance.OwningNetWorker.connected += delegate()
                    {
                        Networking.Instantiate(Player, randSpawnPoint.position, randSpawnPoint.rotation, NetworkReceivers.AllBuffered);
                    };
                }
            }
        }

        Transform spawnPoint
        {
            get
            {
                ForgeDemo_SpawnPoint[] spawnPoints = GameObject.FindObjectsOfType<ForgeDemo_SpawnPoint>();

                if (spawnPoints.Length == 0)
                {
                    Debug.LogError("NO SPAWNPOINTS FOUND!!!");

                    return null;
                }

                return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
            }
        }
    }
}

#endif
