#if uConstruct_ForgeNetworking

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Demo;
using BeardedManStudios.Network;

namespace uConstruct.Extensions.ForgeExtension
{
    public class ForgeBuildingPlacer : BuildingPlacer
    {
        public static List<ForgeBuildingPlacer> instances = new List<ForgeBuildingPlacer>();

        public SimpleNetworkedMonoBehavior entity;

        public override void Awake()
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = Networking.PrimarySocket.IsServer;

            base.Awake();

            if(entity == null)
                entity = GetComponent<SimpleNetworkedMonoBehavior>();

            instances.Add(this);
        }

        public virtual void OnEnable()
        {
            instances.Add(this);
        }

        public virtual void OnDisable()
        {
            if (Demo.DemoUI.instance != null)
            {
                Demo.DemoUI.instance.ResetControl();

                for (int i = 0; i < ForgeBuildingPlacer.instances.Count; i++)
                {
                    ForgeBuildingPlacer.instances[i].ApplyControlsToDemoUI();
                }
            }

            instances.Remove(this);
        }

        public static void LocalNetworkedBuildingPlaced(SimpleNetworkedMonoBehavior target)
        {
            if (instances != null)
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    if (instances[i].entity.NetworkedId == target.NetworkedId)
                    {
                        instances[i].DestroyCurrentBuilding();
                        instances[i].ResetBuildingInstance();
                    }
                }
            }
        }

        public override void PlaceBuilding()
        {
            ForgeBuilding building = currentBuilding as ForgeBuilding;

            if (building != null)
            {
                var evnt = building.PackData(entity, true);
                evnt.Send(NetworkReceivers.Server);
            }

            DestroyCurrentBuilding();
            ResetBuildingInstance();
        }

        public override void DestroyBuilding(BaseBuilding building, RaycastHit hit)
        {
            ForgeBuilding boltBuilding = (ForgeBuilding)building;

            if(boltBuilding == null)
            {
                base.DestroyBuilding(building,hit);
                return;
            }

            var evnt = new UpdateNetworkedBuilding();
            evnt.buildingUID = boltBuilding.networkedID;
            evnt.health = 0;
            evnt.Send(NetworkReceivers.Server);
        }

    }
}

#endif