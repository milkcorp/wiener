#if uConstruct_UFPS && uConstruct_PhotonCloud

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using uConstruct.Extensions.PCloudExtension;

namespace uConstruct.Extensions.UFPSExtension
{
    public class UFPS_Photon_BuildingPlacer : UFPSBuildingPlacer
    {
        PhotonView _entity;
        public PhotonView entity
        {
            get
            {
                if (_entity == null)
                {
                    _entity = GetComponent<PhotonView>();
                }

                return _entity;
            }
        }

        /// <summary>
        /// Call some methods that initiate the networking calls.
        /// </summary>
        public override void Awake()
        {
            uConstruct.Core.Saving.UCSavingManager.enabled = PhotonNetwork.isMasterClient;

            base.Awake();

            instances.Add(this);
        }

        /// <summary>
        /// Get keys inputs, leave empty if you dont want to get any key inputs from the keyboard.
        /// </summary>
        public override void GetInputs()
        {
        }

        /// <summary>
        /// Create a new building instance
        /// </summary>
        /// <param name="building">building game object</param>
        public override void CreateBuildingInstance(GameObject building)
        {
            DestroyCurrentBuilding();

            base.CreateBuildingInstance(building);
        }

        /// <summary>
        /// Place our building
        /// </summary>
        public override void PlaceBuilding()
        {
            if (item != null)
            {
                vp_PlayerInventory pInventory = GetComponentInParent<vp_PlayerInventory>();
                vp_UnitBankInstance instance;

                if (pInventory != null)
                {
                    instance = pInventory.GetUnitBankInstanceOfWeapon(item.weaponInstance);

                    if (instance != null)
                    {
                        instance.TryRemoveUnits(1);

                        if (instance.Count == 0)
                            pInventory.TryRemoveItem(instance);
                    }
                }

                PhotonCloudBuilding building = currentBuilding as PhotonCloudBuilding;

                if (building != null)
                {
                    var evnt = building.PackData(entity, true);
                    evnt.SendToServer();
                }

                DestroyCurrentBuilding();
                ResetBuildingInstance();
            }
        }

        /// <summary>
        /// Reset our building instance
        /// </summary>
        public override void ResetBuildingInstance()
        {
            if (currentPrefab != null)
            {
                base.CreateBuildingInstance(currentPrefab);
            }
        }

        /// <summary>
        /// Apply the local placed building information (Disable if the built building is the one we are pointing on)
        /// </summary>
        /// <param name="target"></param>
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

        /// <summary>
        /// Called when the building is destoryed, apply networked actions.
        /// </summary>
        /// <param name="_building"></param>
        /// <param name="hit"></param>
        public override void DestroyBuilding(BaseBuilding _building, RaycastHit hit)
        {
            PhotonCloudBuilding building = (PhotonCloudBuilding)_building;

            if (building == null)
            {
                base.DestroyBuilding(building, hit);
                return;
            }

            var evnt = new uConstruct.Extensions.PCloudExtension.UpdateNetworkedBuilding();
            evnt.buildingUID = building.networkedID;
            evnt.health = 0;
            evnt.SendToServer();
        }
    }
}

#endif