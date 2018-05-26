#if uConstruct_UFPS

using UnityEngine;
using System.Collections;
using uConstruct.Demo;

namespace uConstruct.Extensions.UFPSExtension
{
    public class uConstructEquipment : MonoBehaviour
    {
        [SerializeField]
        UFPSBuildingPlacer bPlacer;

        [SerializeField]
        GameObject buildingPrefab;

        public vp_Weapon weaponInstance;

        /// <summary>
        /// Called when the weapon is activated.
        /// </summary>
        public void OnEnable()
        {
            if (bPlacer)
                bPlacer.onEquip(buildingPrefab, this);
        }

        /// <summary>
        /// Called when the weapon is deactivated.
        /// </summary>
        public void OnDisable()
        {
            if (bPlacer)
                bPlacer.onDeEquip();
        }

        /// <summary>
        /// Calls on awake, initialize components
        /// </summary>
        protected void Awake()
        {
            if (bPlacer == null)
            {
                bPlacer = GetComponentInParent<UFPSBuildingPlacer>();

                if(bPlacer == null) // if still equals to null
                {
                    Debug.LogError("UFPSBuildingPlacer isn't assigned on : " + transform.root.name + " uConstruct equipment is disabled.");
                    this.enabled = false;
                }
            }

            if(weaponInstance == null)
            {
                weaponInstance = GetComponent<vp_Weapon>();

                if (weaponInstance == null) // if still equals to null
                {
                    Debug.LogError("vp_weapon isn't assigned on : " + transform.name + " uConstruct equipment is disabled.");
                    this.enabled = false;
                }
            }
        }

    }
}

#endif