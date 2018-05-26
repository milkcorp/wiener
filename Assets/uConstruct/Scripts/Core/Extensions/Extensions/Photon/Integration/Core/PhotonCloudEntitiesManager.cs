#if uConstruct_PhotonCloud

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using uConstruct;
using uConstruct.Core.PrefabDatabase;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace uConstruct.Extensions.PCloudExtension
{
    public class PhotonCloudEntitiesManager : Photon.PunBehaviour
    {
        public static PhotonCloudEntitiesManager instance;

        public static List<PhotonCloudBuilding> entities = new List<PhotonCloudBuilding>();

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
                    PhotonCloudBuilding unetBuilding = building as PhotonCloudBuilding;

                    if (unetBuilding != null)
                    {
                        entities.Add(unetBuilding);
                        unetBuilding.networkedID = entities.Count;
                    }
                };
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        public virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Set up callbacks
        /// </summary>
        /// <param name="conn">our socket</param>
        public override void OnJoinedRoom()
        {
            PhotonNetwork.OnEventCall += HandleEvents;
        }

        /// <summary>
        /// Receive and handle events
        /// </summary>
        public void HandleEvents(byte eventCode, object content, int senderid)
        {
            byte[] data;


            if (eventCode == CreateNetworkedBuilding.MSG)
            {
                data = (byte[])content;
                CreateNetworkedBuilding evnt = CreateNetworkedBuilding.Deserialize(data);

                if (evnt != null)
                {
                    CreateNetworkBuildingEvent(evnt);
                }
            }
            else if (eventCode == UpdateNetworkedBuilding.MSG)
            {
                data = (byte[])content;
                UpdateNetworkedBuilding evnt = UpdateNetworkedBuilding.Deserialize(data);

                if (evnt != null)
                {
                    UpdateNetworkedBuildingEvent(evnt);
                }
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

                PhotonCloudBuilding building = instance.GetComponent<PhotonCloudBuilding>();

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

                if (PhotonNetwork.isMasterClient)
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
        public static PhotonCloudBuilding GetEntity(Vector3 pos)
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
        public static PhotonCloudBuilding GetEntity(int id)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].networkedID == id)
                    return entities[i];
            }

            return null;
        }

        /// <summary>
        /// Called when master client is changed, re-assign saving/loading.
        /// </summary>
        /// <param name="newMasterClient">the new master client.</param>
        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = PhotonNetwork.isMasterClient;
        }

        /// <summary>
        /// Called when a client is connected
        /// </summary>
        /// <param name="player">the connected player</param>
        public override void OnPhotonPlayerConnected(PhotonPlayer player)
        {
            if (PhotonNetwork.isMasterClient)
            {
                PhotonCloudBuilding building;

                for (int i = 0; i < entities.Count; i++)
                {
                    building = entities[i];

                    var evnt = building.PackData(null, true);
                    evnt.Send();
                }
            }
        }

        /// <summary>
        /// Called when the CreateNetworkBuilding event is called
        /// </summary>
        /// <param name="msg">event data</param>
        public void CreateNetworkBuildingEvent(CreateNetworkedBuilding evnt)
        {
            if (PhotonNetwork.isMasterClient)
            {
                GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

                if (prefab != null && evnt.requester != null)
                {
                    var instance = GameObject.Instantiate(prefab);

                    PhotonCloudBuilding building = instance.GetComponent<PhotonCloudBuilding>();

                    building.transform.position = (Vector3)evnt.pos;
                    building.transform.rotation = (Quaternion)evnt.rot;

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
        public void UpdateNetworkedBuildingEvent(UpdateNetworkedBuilding evnt)
        {
            UpdateEntity(evnt);
        }

    }

    /// <summary>
    /// The CreateNetworkedBuilding event class
    /// </summary>
    [System.Serializable]
    public class CreateNetworkedBuilding
    {
        public const byte MSG = 152;

        public uConstruct.Core.Saving.SerializeableVector3 pos;
        public uConstruct.Core.Saving.SerializeableQuaternion rot;

        public int health;

        public int id;
        public int placedOnID;
        public int prefabID;

        public int _requesterID;
        public PhotonView requester
        {
            get { return PhotonView.Find(_requesterID); }
        }

        public CreateNetworkedBuilding()
        {

        }

        byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();

            using(var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);

                return ms.ToArray();
            }
        }

        public static CreateNetworkedBuilding Deserialize(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using(var ms = new MemoryStream(data))
            {
                return (CreateNetworkedBuilding)bf.Deserialize(ms);
            }
        }

        public void Send(PhotonPlayer target)
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), true, new RaiseEventOptions() { TargetActors = new int[1] { target.ID } });
        }

        public void Send()
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), true, new RaiseEventOptions() { Receivers = ExitGames.Client.Photon.ReceiverGroup.Others });
        }

        public void SendToServer()
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), false, new RaiseEventOptions() { Receivers = ExitGames.Client.Photon.ReceiverGroup.MasterClient });
        }

    }

    /// <summary>
    /// The UpdateNetworkedBuilding event class
    /// </summary>
    [System.Serializable]
    public class UpdateNetworkedBuilding
    {
        public const byte MSG = 153;

        public int buildingUID;
        public int health;

        public UpdateNetworkedBuilding()
        {

        }

        byte[] Serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, this);

                return ms.ToArray();
            }
        }

        public static UpdateNetworkedBuilding Deserialize(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();

            using (var ms = new MemoryStream(data))
            {
                return (UpdateNetworkedBuilding)bf.Deserialize(ms);
            }
        }

        public void Send(PhotonPlayer target)
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), true, new RaiseEventOptions() { TargetActors = new int[1] { target.ID } });
        }

        public void Send()
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), true, new RaiseEventOptions() { Receivers = ExitGames.Client.Photon.ReceiverGroup.All });
        }

        public void SendToServer()
        {
            PhotonNetwork.RaiseEvent(MSG, Serialize(), false, new RaiseEventOptions() { Receivers = ExitGames.Client.Photon.ReceiverGroup.MasterClient });
        }
    }

}
#endif