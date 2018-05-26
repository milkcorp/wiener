#if uConstruct_ForgeNetworking

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct;
using uConstruct.Core.PrefabDatabase;

using BeardedManStudios.Network;
using BeardedManStudios.Forge;

namespace uConstruct.Extensions.ForgeExtension
{
    public class ForgeEntitiesManager : NetworkedMonoBehavior
    {
        public static List<ForgeBuilding> entities = new List<ForgeBuilding>();

        /// <summary>
        /// Will the system handle the "Ready" state of the connection? (disable if you are setting it up yourself in your game).
        /// </summary>
        public bool handleClientReady = true;

        /// <summary>
        /// Initialize awake
        /// </summary>
        void Awake()
        {
            uConstruct.Core.Saving.BuildingGroupSaveData.OnBuildingLoadedEvent += (BaseBuilding building) =>
                {
                    ForgeBuilding forgeBuilding = building as ForgeBuilding;

                    if (forgeBuilding != null)
                    {
                        entities.Add(forgeBuilding);
                        forgeBuilding.networkedID = entities.Count;
                    }
                };

            if (!Networking.PrimarySocket.Connected)
            {
                Networking.connected += Networking_Setup;
            }
            else
            {
                Networking_Setup(Networking.PrimarySocket);
            }
        }

        /// <summary>
        /// Set up callbacks
        /// </summary>
        /// <param name="socket">our socket</param>
        void Networking_Setup(NetWorker socket)
        {
            Networking.PrimarySocket.AddCustomDataReadEvent(CreateNetworkedBuilding.UID, (NetworkingPlayer player, NetworkingStream stream) =>
                {
                    CreateNetworkedBuilding data = new CreateNetworkedBuilding().Deserialize(stream);

                    BeardedManStudios.Network.Unity.MainThreadManager.Run(() => CreateNetworkBuildingEvent(data));
                });

            Networking.PrimarySocket.AddCustomDataReadEvent(UpdateNetworkedBuilding.UID, (NetworkingPlayer player, NetworkingStream stream) =>
                {
                    UpdateNetworkedBuilding data = new UpdateNetworkedBuilding().Deserialize(stream);

                    BeardedManStudios.Network.Unity.MainThreadManager.Run(() => UpdateNetworkedBuildingEvent(data));
                });

            if (Networking.PrimarySocket.IsServer)
            {
                Networking.PrimarySocket.playerConnected += (NetworkingPlayer player) => StartCoroutine(ClientConnected(player));
            }
        }

        /// <summary>
        /// Load an entity
        /// </summary>
        /// <param name="evnt">our event</param>
        public static void LoadEntity(CreateNetworkedBuilding evnt)
        {
            GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

            if (prefab != null)
            {
                var instance = GameObject.Instantiate(prefab);

                ForgeBuilding building = instance.GetComponent<ForgeBuilding>();

                building.LoadData(evnt);
                entities.Add(building);
            }
        }

        /// <summary>
        /// Update the entity
        /// </summary>
        /// <param name="data">data</param>
        public static void UpdateEntity(UpdateNetworkedBuilding data)
        {
            int id = data.buildingUID;
            var building = GetEntity(id);

            if (building != null)
            {
                building.AssingNetworkedHealth(data.health);

                if (Networking.PrimarySocket.IsServer)
                {
                    data = new UpdateNetworkedBuilding();
                    data.buildingUID = id;
                    data.Send(NetworkReceivers.Others);
                }
            }
        }

        /// <summary>
        /// Find an entity
        /// </summary>
        /// <param name="pos">pos</param>
        /// <returns>found entity</returns>
        public static ForgeBuilding GetEntity(Vector3 pos)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].transform.position == pos)
                    return entities[i];
            }

            return null;
        }

        /// <summary>
        /// Get an entity
        /// </summary>
        /// <param name="id">entity id</param>
        /// <returns>the found entity</returns>
        public static ForgeBuilding GetEntity(int id)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].networkedID == id)
                    return entities[i];
            }

            return null;
        }

        /// <summary>
        /// Called when a client is connected
        /// </summary>
        /// <param name="player">the connected player</param>
        public IEnumerator ClientConnected(NetworkingPlayer player)
        {
            yield return new WaitForSeconds(1);

            ForgeBuilding building;

            for (int i = 0; i < entities.Count; i++)
            {
                building = entities[i];

                var evnt = building.PackData(null, true);
                evnt.Send(player);
            }
        }

        /// <summary>
        /// Called when the CreateNetworkBuilding event is called
        /// </summary>
        /// <param name="evnt">event data</param>
        public void CreateNetworkBuildingEvent(CreateNetworkedBuilding evnt)
        {
            if (Networking.PrimarySocket.IsServer)
            {
                GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

                if (prefab != null && evnt.requester != null)
                {
                    var instance = GameObject.Instantiate(prefab);

                    ForgeBuilding building = instance.GetComponent<ForgeBuilding>();

                    building.transform.position = evnt.pos;
                    building.transform.rotation = evnt.rot;

                    if (!building.CheckConditions()) // check conditions serverside
                    {
                        Destroy(instance);
                        return;
                    }

                    entities.Add(building);

                    evnt.id = entities.Count;
                    evnt.health = building.maxHealth;

                    building.LoadData(evnt);

                    building.PackData(evnt.requester, false).Send(NetworkReceivers.Others);
                }
            }
            else
            {
                LoadEntity(evnt);
            }
        }

        /// <summary>
        /// Called when the UpdateNetworkedBuildingEvent is called
        /// </summary>
        /// <param name="evnt">the event data</param>
        public void UpdateNetworkedBuildingEvent(UpdateNetworkedBuilding evnt)
        {
            UpdateEntity(evnt);
        }

    }

    /// <summary>
    /// The CreateNetworkedBuilding event class
    /// </summary>
    public class CreateNetworkedBuilding
    {
        public const uint UID = 9502478;

        public Vector3 pos;
        public Quaternion rot;

        public int health;

        public int id;
        public int placedOnID;
        public int prefabID;

        public ulong requesterID;
        public SimpleNetworkedMonoBehavior requester
        {
            get
            {
                if (requesterID == 99999) return null;

                SimpleNetworkedMonoBehavior behaviour;

                SimpleNetworkedMonoBehavior.networkedBehaviors.TryGetValue(requesterID, out behaviour);

                return behaviour;
            }
        }

        private BMSByte cachedData = new BMSByte();

        public void Send(NetworkingPlayer target)
        {
            cachedData.Clone(Serialize());
            Networking.WriteCustom(UID, Networking.PrimarySocket, cachedData, target, true);
        }

        public void Send(NetworkReceivers targets)
        {
            cachedData.Clone(Serialize());
            Networking.WriteCustom(UID, Networking.PrimarySocket, cachedData, true, targets);
        }

        private BMSByte Serialize()
        {
            return ObjectMapper.MapBytes(cachedData, pos, rot, health, id, placedOnID, prefabID, requesterID);
        }

        public CreateNetworkedBuilding Deserialize(NetworkingStream stream)
        {
            this.pos = ObjectMapper.Map<Vector3>(stream);
            this.rot = ObjectMapper.Map<Quaternion>(stream);
            this.health = ObjectMapper.Map<int>(stream);
            this.id = ObjectMapper.Map<int>(stream);

            this.placedOnID = ObjectMapper.Map<int>(stream);
            this.prefabID = ObjectMapper.Map<int>(stream);

            this.requesterID = ObjectMapper.Map<ulong>(stream);

            return this;
        }
    }

    /// <summary>
    /// The UpdateNetworkedBuilding event class
    /// </summary>
    public class UpdateNetworkedBuilding
    {
        public const uint UID = 9502477;

        public int buildingUID;
        public int health;

        private BMSByte cachedData = new BMSByte();

        public void Send(NetworkingPlayer target)
        {
            cachedData.Clone(Serialize());
            Networking.WriteCustom(UID, Networking.PrimarySocket, cachedData, target, true);
        }

        public void Send(NetworkReceivers targets)
        {
            cachedData.Clone(Serialize());
            Networking.WriteCustom(UID, Networking.PrimarySocket, cachedData, true, targets);
        }

        private BMSByte Serialize()
        {
            return ObjectMapper.MapBytes(cachedData, buildingUID, health);
        }

        public UpdateNetworkedBuilding Deserialize(NetworkingStream stream)
        {
            this.buildingUID = ObjectMapper.Map<int>(stream);
            this.health = ObjectMapper.Map<int>(stream);

            return this;
        }
    }

}
#endif