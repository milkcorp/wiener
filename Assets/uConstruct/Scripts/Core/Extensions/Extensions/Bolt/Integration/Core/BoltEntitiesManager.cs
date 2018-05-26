#if uConstruct_PhotonBolt

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct;
using uConstruct.Core.PrefabDatabase;

namespace uConstruct.Extensions.BoltExtension
{
    [BoltGlobalBehaviour(BoltNetworkModes.Host)]
    public class BoltEntitiesManager : Bolt.GlobalEventListener
    {
        public static List<BoltBuilding> entities = new List<BoltBuilding>();

        void Awake()
        {
            uConstruct.Core.Saving.BuildingGroupSaveData.OnBuildingLoadedEvent += (BaseBuilding building) =>
                {
                    BoltBuilding boltBuilding = (BoltBuilding)building;

                    if(boltBuilding != null)
                    {
                        entities.Add(boltBuilding);
                        boltBuilding.networkedID = entities.Count;
                    }
                };
        }

        public static void LoadEntity(CreateNetworkedBuilding evnt)
        {
            GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

            if (prefab != null)
            {
                var instance = GameObject.Instantiate(prefab);

                BoltBuilding building = instance.GetComponent<BoltBuilding>();

                building.LoadData(evnt);
                entities.Add(building);
            }
        }

        public static void UpdateEntity(UpdateNetworkedBuilding evnt)
        {
            int id = evnt.buildingUID;
            var building = GetEntity(id);

            if (building != null)
            {
                building.AssingNetworkedHealth(evnt.health);

                if (BoltNetwork.isServer)
                {
                    evnt = UpdateNetworkedBuilding.Create(Bolt.GlobalTargets.Others, Bolt.ReliabilityModes.ReliableOrdered);
                    evnt.buildingUID = id;
                    evnt.Send();
                }
            }
        }

        public static BoltBuilding GetEntity(Vector3 pos)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].transform.position == pos)
                    return entities[i];
            }

            return null;
        }

        public static BoltBuilding GetEntity(int id)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].networkedID == id)
                    return entities[i];
            }

            return null;
        }

        public override void SceneLoadRemoteDone(BoltConnection connection)
        {
            BoltBuilding building;

            for (int i = 0; i < entities.Count; i++)
            {
                building = entities[i];

                BaseBuilding.CallPack(building);

                var evnt = CreateNetworkedBuilding.Create(connection, Bolt.ReliabilityModes.ReliableOrdered);
                evnt.pos = building.transform.position;
                evnt.rot = building.transform.rotation;
                evnt.health = building.health;

                evnt.id = building.networkedID;
                evnt.placedOnID = building.networkedPlacedOnID;
                evnt.prefabID = building.prefabID;
                evnt.Send();
            }
        }

        public override void OnEvent(CreateNetworkedBuilding evnt)
        {
            GameObject prefab = PrefabDB.instance.GetGO(evnt.prefabID);

            if (prefab != null && evnt.requester != null)
            {
                var instance = GameObject.Instantiate(prefab);

                BoltBuilding building = instance.GetComponent<BoltBuilding>();

                building.transform.position = evnt.pos;
                building.transform.rotation = evnt.rot;

                if (!building.CheckConditions()) // check conditions serverside
                {
                    Destroy(instance);
                    return;
                }

                entities.Add(building);

                evnt.health = building.maxHealth;
                evnt.id = entities.Count;

                building.LoadData(evnt);

                building.PackData(evnt.requester, false).Send();
            }
        }

        public override void OnEvent(UpdateNetworkedBuilding evnt)
        {
            UpdateEntity(evnt);
        }

    }
}
#endif