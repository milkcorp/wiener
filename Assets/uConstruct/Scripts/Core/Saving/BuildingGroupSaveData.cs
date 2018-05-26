using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uConstruct.Sockets;
using uConstruct.Core.PrefabDatabase;

using System.Linq;

namespace uConstruct.Core.Saving
{

    public delegate void OnBuildingLoaded(BaseBuilding building);
    public delegate void OnBuildingSaving(BuildingSaveData data);

    /// <summary>
    /// This is a class that holds data for all the group save data.
    /// used for saving groups.
    /// </summary>
    [System.Serializable]
    public class BuildingGroupSaveData : BaseUCSaveData
    {
        /// <summary>
        /// Run this action on the first loaded building on each group.
        /// </summary>
        public static System.Action<GameObject> initialBuildingAction;

        public static event OnBuildingLoaded OnBuildingLoadedEvent;

        public List<BuildingSaveData> buildingsData = new List<BuildingSaveData>();

        [System.NonSerialized]
        BaseBuildingGroup instanceGroup = null;

        public override void Load(BaseUCSaveData _data)
        {
            BuildingSaveData currentBuilding;
            GameObject go;

            // place the buildings, dont load them yet.
            for (int i = 0; i < buildingsData.Count; i++)
            {
                currentBuilding = buildingsData[i];

                go = LoadSpecificData(currentBuilding);

                if (i == 0 && initialBuildingAction != null)
                    initialBuildingAction(go);
            }
        }

        GameObject LoadSpecificData(BuildingSaveData building)
        {
            GameObject buildingPrefab = PrefabDB.instance.GetGO(building.prefabID);

            if (buildingPrefab != null)
            {
                GameObject go = GameObject.Instantiate(buildingPrefab);
                BaseBuilding buildingScript = go.GetComponent<BaseBuilding>();

                if (instanceGroup == null)
                {
                    instanceGroup = BaseBuildingGroup.CreateGroup((Vector3)building.pos, buildingScript.buildingGroupType);
                }

                buildingScript.buildingGroup = instanceGroup;

                go.transform.position = (Vector3)building.pos;
                go.transform.rotation = (Quaternion)building.rot;

                buildingScript.health = building.health;
                buildingScript.uid = building.uniqueID;

                BaseBuilding currentBuilding;
                BaseSocket snappedTo;

                for (int i = 0; i < instanceGroup.buildings.Count; i++)
                {
                    currentBuilding = instanceGroup.buildings[i];

                    if (currentBuilding.uid == building.placedOnUID)
                    {
                        snappedTo = currentBuilding.ReturnSocket((Vector3)building.pos, buildingScript.buildingType);

                        buildingScript.SnappedTo = snappedTo;
                        buildingScript.placedOn = currentBuilding;

                        break;
                    }
                }

                buildingScript.PlaceBuilding();

                for (int i = 0; i < instanceGroup.buildings.Count; i++) // handle occoupied sockets to make sure there's no sockets on each other.
                {
                    currentBuilding = instanceGroup.buildings[i];
                    instanceGroup.HandleOccupiedSockets(currentBuilding);
                }

                CallOnLoad(buildingScript);

                return go;
            }

            return null;
        }

        public static void CallOnLoad(BaseBuilding Building)
        {
            if (OnBuildingLoadedEvent != null)
            {
                OnBuildingLoadedEvent(Building);
            }
        }
    }

    /// <summary>
    /// Save data class for the group
    /// </summary>
    [System.Serializable]
    public class BuildingSaveData
    {
        public static event OnBuildingSaving OnBuildingSavingEvent;

        public SerializeableVector3 pos;
        public SerializeableQuaternion rot;

        public int health;

        public int prefabID;
        public int uniqueID;
        public int placedOnUID;

        public BuildingSaveData(Vector3 _pos, Quaternion _rot, int _placedOnUID, int _health, int _prefabID, int _uniqueID)
        {
            pos = _pos;
            rot = _rot;
            placedOnUID = _placedOnUID;
            this.health = _health;

            prefabID = _prefabID;
            uniqueID = _uniqueID;

            if (OnBuildingSavingEvent != null)
                OnBuildingSavingEvent(this);
        }
    }
}
