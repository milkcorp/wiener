#if uConstruct_PhotonBolt

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Demo;

namespace uConstruct.Extensions.BoltExtension
{
    public class BoltBuildingPlacer : BuildingPlacer
    {
        public static List<BoltBuildingPlacer> instances = new List<BoltBuildingPlacer>();

        public BoltEntity entity;

        public override void Awake()
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = BoltNetwork.isServer;

            base.Awake();

            if(entity == null)
                entity = GetComponent<BoltEntity>();

            instances.Add(this);
        }

        void OnEnable()
        {
            instances.Add(this);
        }

        public virtual void OnDisable()
        {
            if (Demo.DemoUI.instance != null)
            {
                Demo.DemoUI.instance.ResetControl();

                for (int i = 0; i < BoltBuildingPlacer.instances.Count; i++)
                {
                    BoltBuildingPlacer.instances[i].ApplyControlsToDemoUI();
                }
            }

            instances.Remove(this);
        }

        public static void LocalNetworkedBuildingPlaced(BoltEntity target)
        {
            if (instances != null)
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    if (instances[i].entity == target)
                    {
                        instances[i].DestroyCurrentBuilding();
                        instances[i].ResetBuildingInstance();
                    }
                }
            }
        }

        public override void PlaceBuilding()
        {
            BaseBuilding.CallPack(currentBuilding);

            var evnt = CreateNetworkedBuilding.Create(Bolt.GlobalTargets.OnlyServer, Bolt.ReliabilityModes.Unreliable);
            evnt.pos = currentBuilding.transform.position;
            evnt.rot = currentBuilding.transform.rotation;

            evnt.placedOnID = ((BoltBuilding)(currentBuilding)).networkedPlacedOnID;
            evnt.prefabID = currentBuilding.prefabID;

            evnt.requester = entity;

            evnt.Send();

            DestroyCurrentBuilding();
            ResetBuildingInstance();
        }

        public override void DestroyBuilding(BaseBuilding building, RaycastHit hit)
        {
            BoltBuilding boltBuilding = (BoltBuilding)building;

            if(boltBuilding == null)
            {
                base.DestroyBuilding(building,hit);
                return;
            }

            var evnt = UpdateNetworkedBuilding.Create(Bolt.GlobalTargets.OnlyServer, Bolt.ReliabilityModes.ReliableOrdered);
            evnt.buildingUID = boltBuilding.networkedID;
            evnt.health = 0;
            evnt.Send();
        }

    }
}

#endif