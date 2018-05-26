#if uConstruct_UNet

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using uConstruct;
using uConstruct.Core.PrefabDatabase;

namespace uConstruct.Extensions.UNetExtension
{
    public class UNetEntitiesManager : NetworkManager
    {
        public static UNetEntitiesManager instance;

        public static List<UNetBuilding> entities = new List<UNetBuilding>();

        /// <summary>
        /// Will the system handle the "Ready" state of the connection? (disable if you are setting it up yourself in your game).
        /// </summary>
        public bool handleClientReady = true;

        /// <summary>
        /// Initialize awake
        /// </summary>
        public void OnEnable()
        {
            instance = this;

            uConstruct.Core.Saving.BuildingGroupSaveData.OnBuildingLoadedEvent += (BaseBuilding building) =>
                {
                    UNetBuilding unetBuilding = building as UNetBuilding;

                    if (unetBuilding != null)
                    {
                        entities.Add(unetBuilding);
                        unetBuilding.networkedID = entities.Count;
                    }
                };
        }

        /// <summary>
        /// Set up callbacks
        /// </summary>
        /// <param name="conn">our socket</param>
        public override void OnStartClient(NetworkClient client)
        {
            if (NetworkServer.active)
            {
                NetworkServer.RegisterHandler(CreateNetworkedBuilding.MSG, CreateNetworkBuildingEvent);
                NetworkServer.RegisterHandler(UpdateNetworkedBuilding.MSG, UpdateNetworkedBuildingEvent);
            }
            else if(NetworkClient.active)
            {
                NetworkClient.allClients[0].RegisterHandler(CreateNetworkedBuilding.MSG, CreateNetworkBuildingEvent);
                NetworkClient.allClients[0].RegisterHandler(UpdateNetworkedBuilding.MSG, UpdateNetworkedBuildingEvent);
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

                UNetBuilding building = instance.GetComponent<UNetBuilding>();

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

                if (NetworkServer.active)
                {
                    data = new UpdateNetworkedBuilding();
                    data.buildingUID = id;
                    data.Send();
                }
            }
        }

        /// <summary>
        /// Find an entity
        /// </summary>
        /// <param name="pos">pos</param>
        /// <returns>found entity</returns>
        public static UNetBuilding GetEntity(Vector3 pos)
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
        public static UNetBuilding GetEntity(int id)
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
        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);

            if (NetworkServer.active)
            {
                UNetBuilding building;

                for (int i = 0; i < entities.Count; i++)
                {
                    building = entities[i];

                    var evnt = building.PackData(null, true);
                    evnt.Send(conn);
                }
            }
        }

        /// <summary>
        /// Called when the CreateNetworkBuilding event is called
        /// </summary>
        /// <param name="msg">event data</param>
        public void CreateNetworkBuildingEvent(NetworkMessage msg)
        {
            var evnt = msg.ReadMessage<CreateNetworkedBuilding>();

            if (NetworkServer.active)
            {
                GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

                if (prefab != null && evnt.requester != null)
                {
                    var instance = GameObject.Instantiate(prefab);

                    UNetBuilding building = instance.GetComponent<UNetBuilding>();

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

                    building.PackData(evnt.requester, false).Send();
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
        public void UpdateNetworkedBuildingEvent(NetworkMessage msg)
        {
            var evnt = msg.ReadMessage<UpdateNetworkedBuilding>();

            UpdateEntity(evnt);
        }

    }

    /// <summary>
    /// The CreateNetworkedBuilding event class
    /// </summary>
    public class CreateNetworkedBuilding : MessageBase
    {
        public const short MSG = 559;

        public Vector3 pos;
        public Quaternion rot;

        public int health;

        public int id;
        public int placedOnID;
        public int prefabID;

        public NetworkIdentity requester;

        public CreateNetworkedBuilding()
        {

        }

        public void Send(NetworkConnection target)
        {
            target.Send(MSG, this);
        }

        public void Send()
        {
            NetworkServer.SendToAll(MSG, this);
        }

        public void SendToServer()
        {
            if (NetworkClient.active)
            {
                NetworkClient.allClients[0].SendUnreliable(MSG, this);
            }
        }

    }

    /// <summary>
    /// The UpdateNetworkedBuilding event class
    /// </summary>
    public class UpdateNetworkedBuilding : MessageBase
    {
        public const short MSG = 560;

        public int buildingUID;
        public int health;

        public UpdateNetworkedBuilding()
        {

        }

        public void Send(NetworkConnection target)
        {
            target.Send(MSG, this);
        }

        public void Send()
        {
            NetworkServer.SendToAll(MSG, this);
        }

        public void SendToServer()
        {
            if(NetworkClient.active)
            {
                NetworkClient.allClients[0].SendUnreliable(MSG, this);
            }
        }
    }

}
#endif