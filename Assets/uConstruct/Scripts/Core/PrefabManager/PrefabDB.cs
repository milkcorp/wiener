using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.PrefabDatabase
{
    /// <summary>
    /// This class handles all prefab database in the system.
    /// </summary>
    public class PrefabDB : ScriptableObject
    {
        static PrefabDB _instance;
        public static PrefabDB instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Resources.Load<PrefabDB>("UConstructPrefabsDatabase");

                    #if UNITY_EDITOR
                    if (_instance == null) // if its still null then we have to recreate it...
                    {
                        _instance = CreateInstance<PrefabDB>();

                        UnityEditor.AssetDatabase.CreateAsset(_instance, uConstruct.Core.Manager.UCCallbacksManager.ProjectPath + "Resources/UConstructPrefabsDatabase.asset");
                        UnityEditor.EditorUtility.SetDirty(_instance);
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
                    #endif
                }

                return _instance;
            }
        }

        [SerializeField]
        private List<PrefabData> _prefabs = new List<PrefabData>();
        public List<PrefabData> prefabs
        {
            get { return _prefabs; }
        }

        /// <summary>
        /// Add an item to the database
        /// </summary>
        /// <param name="go">The GameObject you want to add</param>
        /// <param name="UID">The prefabID you want to assign to it</param>
        public void AddToDB(GameObject go, int UID)
        {
            for (int i = 0; i < _prefabs.Count; i++ )
            {
                if (_prefabs[i].go.Equals(go))
                    return;
            }

                _prefabs.Add(new PrefabData(UID, go));
        }

        /// <summary>
        /// Remove an prefab from the database
        /// </summary>
        /// <param name="go">what prefab to remove</param>
        public void RemoveFromDB(GameObject go)
        {
            for(int i = 0; i < _prefabs.Count; i++)
            {
                if(_prefabs[i].go.Equals(go))
                {
                    _prefabs.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Reset the prefabs on the database
        /// </summary>
        public void ResetDB()
        {
            _prefabs.Clear();
        }

        /// <summary>
        /// Does the prefab contains this prefab id?
        /// </summary>
        /// <param name="uid">the prefab uid to check</param>
        /// <returns>is it used?</returns>
        public bool Contains(int uid)
        {
            for(int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].ID == uid)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Add an item to the database
        /// </summary>
        /// <param name="go">The gameObject</param>
        /// <returns>Random prefabID</returns>
        public int AddToDB(GameObject go)
        {
            int uniqueID = ReturnUID(prefabs.Count + 1);
            AddToDB(go, uniqueID);
            return uniqueID;
        }
         
        /// <summary>
        /// Get a gameobject thats attached to this prefabID
        /// </summary>
        /// <param name="prefabID">The prefab id you want to get an game object off</param>
        /// <returns>The game object this prefabID belongs to</returns>
        public GameObject GetGO(int prefabID)
        {
            PrefabData data;
            for (int i = 0; i < _prefabs.Count; i++)
            {
                data = _prefabs[i];

                if (data.ID == prefabID)
                    return data.go;
            }

            return null;
        }

        /// <summary>
        /// Returns a building that has that specific type in it.
        /// </summary>
        /// <param name="type">type of the building</param>
        /// <returns></returns>
        public GameObject GetGO(BuildingType type)
        {
            BaseBuilding buildingScript;

            for(int i = 0; i < prefabs.Count; i++)
            {
                buildingScript = prefabs[i].go.GetComponent<BaseBuilding>();

                if (buildingScript != null && buildingScript.buildingType == type)
                    return buildingScript.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Return a random id that isnt used
        /// </summary>
        /// <returns>an random id</returns>
        public int ReturnUID()
        {
            return ReturnUID(_prefabs.Count);
        }

        /// <summary>
        /// Get ID that isnt in use
        /// </summary>
        /// <param name="initial">the initial value, leave 0 if called first time</param>
        /// <returns>unique id</returns>
        int ReturnUID(int initial)
        {
            int UID = initial + 1;

            if (Contains(UID)) return ReturnUID(UID);

            return UID;
        }
    }


    /// <summary>
    /// Holds all the data for a prefab
    /// </summary>
    [System.Serializable]
    public class PrefabData
    {
        public int ID = -1;
        public GameObject go = null;

        public PrefabData()
        {
            ID = -1;
            go = null;
        }

        public PrefabData(int _ID, GameObject _go)
        {
            this.ID = _ID;
            this.go = _go;
        }
    }
}