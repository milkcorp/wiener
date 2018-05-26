#if uConstruct_UFPS

using UnityEngine;
using System.Collections;

namespace uConstruct.Extensions.UFPSExtension
{
    public class UFPSBuildingPlacer
#if uConstruct_PhotonCloud && uConstruct_UFPS
 : uConstruct.Extensions.PCloudExtension.PhotonCloudBuildingPlacer
#else
        : uConstruct.Demo.BuildingPlacer
#endif
    {
        protected GameObject currentPrefab;
        protected uConstructEquipment item;

        /// <summary>
        /// On Item Equipped.
        /// </summary>
        public virtual void onEquip(GameObject prefab, uConstructEquipment item)
        {
            this.currentPrefab = prefab;
            this.item = item;

            CreateBuildingInstance(prefab);
        }

        /// <summary>
        /// On Item UnEquipped.
        /// </summary>
        public virtual void onDeEquip()
        {
            currentPrefab = null;
            item = null;

            DestroyCurrentBuilding();
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
                base.PlaceBuilding();

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
    }
}

#endif
