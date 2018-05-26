#if uConstruct_UNet

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using uConstruct.Demo;

namespace uConstruct.Extensions.UNetExtension
{
    public class UNetBuildingPlacer : BuildingPlacer
    {
        public static List<UNetBuildingPlacer> instances = new List<UNetBuildingPlacer>();

        public NetworkIdentity entity;

        public override void Awake()
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = NetworkServer.active;

            base.Awake();

            if(entity == null)
                entity = GetComponent<NetworkIdentity>();

            instances.Add(this);
        }

        public virtual void OnEnable()
        {
            if (!instances.Contains(this))
            {
                instances.Add(this);
            }
        }

        public virtual void OnDisable()
        {
            if (Demo.DemoUI.instance != null)
            {
                Demo.DemoUI.instance.ResetControl();

                for (int i = 0; i < UNetBuildingPlacer.instances.Count; )
                {
                    UNetBuildingPlacer.instances[i].ApplyControlsToDemoUI();
                    return;
                }
            }

            instances.Remove(this);
        }

        public static void LocalNetworkedBuildingPlaced(NetworkIdentity target)
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
            UNetBuilding building = currentBuilding as UNetBuilding;

            if (building != null)
            {
                var evnt = building.PackData(entity, true);
                evnt.SendToServer();
            }

            DestroyCurrentBuilding();
            ResetBuildingInstance();
        }

        public override void DestroyBuilding(BaseBuilding _building, RaycastHit hit)
        {
            UNetBuilding building = (UNetBuilding)_building;

            if (building == null)
            {
                base.DestroyBuilding(building,hit);
                return;
            }

            var evnt = new UpdateNetworkedBuilding();
            evnt.buildingUID = building.networkedID;
            evnt.health = 0;
            evnt.SendToServer();
        }

    }
}

#endif