#if uConstruct_PhotonCloud

using UnityEngine;
using UnityEngine.Networking;

using System.Collections;
using System.Collections.Generic;

using uConstruct.Demo;

namespace uConstruct.Extensions.PCloudExtension
{
    public class PhotonCloudBuildingPlacer : BuildingPlacer
    {
        public static List<PhotonCloudBuildingPlacer> instances = new List<PhotonCloudBuildingPlacer>();

        public PhotonView entity;

        /// <summary>
        /// Call some methods that initiate the networking calls.
        /// </summary>
        public override void Awake()
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = PhotonNetwork.isMasterClient;

            base.Awake();

            if(entity == null)
                entity = GetComponent<PhotonView>();

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

                for (int i = 0; i < PhotonCloudBuildingPlacer.instances.Count; )
                {
                    PhotonCloudBuildingPlacer.instances[i].ApplyControlsToDemoUI();
                    return;
                }
            }

            instances.Remove(this);
        }

        public static void LocalNetworkedBuildingPlaced(PhotonView target)
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
            PhotonCloudBuilding building = currentBuilding as PhotonCloudBuilding;

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
            PhotonCloudBuilding building = (PhotonCloudBuilding)_building;

            if(building == null)
            {
                base.DestroyBuilding(building, hit);
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