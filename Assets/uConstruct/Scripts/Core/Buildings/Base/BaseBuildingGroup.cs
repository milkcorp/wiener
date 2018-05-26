using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using uConstruct.Core.Saving;
using uConstruct.Core.AOI;
using uConstruct.Sockets;

namespace uConstruct
{

    public delegate void BuildingAdded(BaseBuilding building);
    public delegate void BuildingRemoved(BaseBuilding building);
    public delegate void GroupBuildingDestroyed(BaseBuilding building);

    public delegate void BuildingGroupCreated(BaseBuildingGroup group);

    public delegate void OnBatchDone(GameObject go, Mesh mesh);
    public delegate void BatchedGroup(bool value);

    /// <summary>
    /// A base class for groups.
    /// Handles all group management.
    /// incase of doing another use of groups please inherite from this class.
    /// </summary>
    public class BaseBuildingGroup : BuildingGroupAOITarget, UCSavedItem
    {
        static List<BaseBuildingGroup> _groups = new List<BaseBuildingGroup>();
        public static List<BaseBuildingGroup> groups
        {
            get { return _groups; }
        }

        static int _lastID;
        public static int lastID
        {
            get
            {
                _lastID++;
                return _lastID;
            }
            set
            {
                _lastID = value;
            }
        }

        #region Variables
        public List<BaseBuilding> groupBuildings = new List<BaseBuilding>();

        /// <summary>
        /// Get the group buildings
        /// </summary>
        public List<BaseBuilding> buildings
        {
            get { return groupBuildings; }
        }
        #endregion

        #region BatchData
        bool initializedBatch;

        BatchData batchData;
        HashSet<MeshFilter> batchedFilters = new HashSet<MeshFilter>();
        List<Transform> batchInstances = new List<Transform>();
        #endregion

        #region Events

        public event BuildingAdded OnBuildingAddedEvent;
        public event BuildingRemoved OnBuildingRemovedEvent;

        public event GroupBuildingDestroyed OnGroupBuildingDestroyed;
        public event BatchedGroup OnGroupBatchedEvent;

        public static event OnBatchDone OnBatchDoneEvent;
        public static event OnBuildingGroupChanged OnBuildingGroupCreatedEvent;
        #endregion

        /// <summary>
        /// A generic method to create a building group.
        /// </summary>
        /// <typeparam name="T">The class of the group</typeparam>
        /// <param name="pos">The position that the group will be on, so if its the first building in the group just give it the building position.</param>
        /// <returns>Returns the created group instance.</returns>
        public static T CreateGroup<T>(Vector3 pos) where T : BaseBuildingGroup
        {
            return CreateGroup(pos, typeof(T)) as T;
        }

        /// <summary>
        /// A generic method to create a building group.
        /// </summary>
        /// <param name="groupType">The class of the group</param>
        /// <param name="pos">The position that the group will be on, so if its the first building in the group just give it the building position.</param>
        /// <returns>Returns the created group instance.</returns>
        public static BaseBuildingGroup CreateGroup(Vector3 pos, System.Type groupType)
        {
            if(groupType.BaseType != typeof(BaseBuildingGroup) && groupType != typeof(BaseBuildingGroup))
            {
                Debug.LogError(string.Format("Cannot create group with type {0} because it doesnt inherit from BaseBuildingGroup !!!", groupType));
                return null;
            }

            GameObject go = new GameObject();
            int id = lastID;

            go.name = "BuildingGroup : " + id;
            go.transform.position = pos;
            var instance = go.AddComponent(groupType) as BaseBuildingGroup;

            instance.GetBuildings();
            groups.Add(instance);

            if (OnBuildingGroupCreatedEvent != null)
                OnBuildingGroupCreatedEvent(instance);

            return instance;
        }
        
        /// <summary>
        /// Destroys the group.
        /// </summary>
        public virtual void DestroyGroup()
        {
            for(int i = 0; i < groupBuildings.Count; i++)
            {
                RemoveBuilding(groupBuildings.ElementAt(i));
            }

            groups.Remove(this);
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Assign all the buildings in your group ( get components in childrens ) and assign them.
        /// </summary>
        protected virtual void GetBuildings()
        {
            groupBuildings = GetComponentsInChildren<BaseBuilding>().ToList();
        }

        /// <summary>
        /// Awake.
        /// </summary>
        void Awake()
        {
            UCSavingManager.OnLoadingProcessComplete += () => Batch(true, null, false);
        }



        /// <summary>
        /// Add a building to the group.
        /// </summary>
        /// <param name="building"> the building </param>
        public virtual void AddBuilding(BaseBuilding building)
        {
            if (!groupBuildings.Contains(building))
            {
                groupBuildings.Add(building);
            }

            OnBuildingAddedEvent += building.GroupBuildingAdded;
            OnBuildingRemovedEvent += building.GroupBuildingRemoved;

            building.transform.parent = this.transform;
            building.buildingGroup = this;

            PopulateBatchedFilters(building, true);

            ///Add this building to the event count
            building.OnDestroyEvent += () =>
            {
                if (OnGroupBuildingDestroyed != null)
                {
                    OnGroupBuildingDestroyed(building);
                }
            };

            if (OnBuildingAddedEvent != null)
                OnBuildingAddedEvent(building);

            Batch(true, building, true);
        }

        /// <summary>
        /// Remove a building from the group.
        /// </summary>
        /// <param name="building">The building</param>
        public virtual void RemoveBuilding(BaseBuilding building)
        {
            if (groupBuildings.Contains(building))
            {
                groupBuildings.Remove(building);

                OnBuildingAddedEvent -= building.GroupBuildingAdded;
                OnBuildingRemovedEvent -= building.GroupBuildingRemoved;

                if (OnBuildingRemovedEvent != null)  // call this now before the un-parenting is applied so it doesn't mess up the local position.
                    OnBuildingRemovedEvent(building);

                building.transform.parent = null;

                PopulateBatchedFilters(building, false);

                if (groupBuildings.Count == 0)
                {
                    Batch(false, building, false);

                    DestroyGroup();
                    return;
                }

                Batch(true, building, false);
            }
        }

        /// <summary>
        /// 
        /// Toggle AOI state on the group, if value is true, it will disable all sockets in the group to save performance and avoid the physics limit.
        /// 
        /// Use case :
        /// when a player is more than 20 meters from the group enable AOI cause he wont build in this area so no need to keep sockets alive.
        /// 
        /// </summary>
        /// <param name="value">Should it enable AOI or not ? if set to true then all sockets on all buildings in the group will be disabled if set to false then they will be enabled.</param>
        public virtual void AOIGroup(bool value, Vector3 position, float radius)
        {
            BaseBuilding building;
            bool perBuilding = UCSettings.instance.UCAOICalculationMethod == UCAOIMethod.PerBuilding;

            for (int i = 0; i < groupBuildings.Count; i++)
            {
                building = groupBuildings.ElementAt(i);

                if (perBuilding)
                {
                    value = Vector3.Distance(building.transform.position, position) <= radius;
                }

                building.ActivateSockets(value, false);
                building.ActivateConditions(value, true);
                building.ActivateColliders(value);
            }
        }

        /// <summary>
        /// This method will handle all the building sockets and check if one of the buildings in the group are on that socket.
        /// </summary>
        /// <param name="building">the building you want to apply sockets check for.</param>
        public virtual void HandleOccupiedSockets(BaseBuilding building)
        {
            BaseBuilding groupBuilding;
            BaseSocket socket;

            for (int i = 0; i < groupBuildings.Count; i++)
            {
                groupBuilding = groupBuildings.ElementAt(i);

                socket = building.ReturnSocket(groupBuilding.transform.position, groupBuilding.buildingType);

                if(socket != null)
                {
                    socket.isOccupied = true;
                }
            }
        }

        /// <summary>
        /// Is the socket occopied in the group buildings?
        /// </summary>
        /// <param name="socketInstance">the instance of the socket you are checking.</param>
        /// <returns></returns>
        public virtual bool IsGroupSocketOccoupied(BaseSocket socketInstance)
        {
            BaseSocket socket;

            for (int i = 0; i < buildings.Count; i++)
            {
                socket = buildings.ElementAt(i).ReturnSocket(socketInstance.transform.position, socketInstance.receiveType);

                if (socket != null && socket.building != socketInstance.building && !socket.isActive)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Enable all of the sockets in the group.
        /// </summary>
        /// <param name="value">Disable or enable?</param>
        /// <param name="force">Force?</param>
        public void EnableGroupSockets(bool value, bool force)
        {
            for(int i = 0; i < buildings.Count; i++)
            {
                buildings.ElementAt(i).ActivateSockets(value, force);
            }
        }

        #region Batching

        /// <summary>
        /// Populate the batch filters.
        /// </summary>
        /// <param name="building">the building</param>
        /// <param name="Add">are we adding ? or removing ?</param>
        public virtual void PopulateBatchedFilters(BaseBuilding building, bool Add)
        {
            if (!building.batchBuilding && Add) return;

            MeshFilter[] filters = building.GetComponentsInChildren<MeshFilter>(true);

            for(int i = 0; i < filters.Length; i++)
            {
                if(Add)
                {
                    batchedFilters.Add(filters[i]);
                }
                else
                {
                    if(batchedFilters.Contains(filters[i]))
                    {
                        batchedFilters.Remove(filters[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Get the batch data
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        protected virtual MeshFilter[] GetUpdatedBatchData(BaseBuilding building, bool add)
        {
            if (building.batchBuilding == false && add) return new MeshFilter[0];

            return building.GetComponentsInChildren<MeshFilter>();
        }

        /// <summary>
        /// Batch buildings
        /// </summary>
        /// <param name="value">Batch the building ?</param>
        public virtual void Batch(bool value, BaseBuilding updatedBuilding, bool added)
        {
            if (UCSavingManager.IsLoading || !UCSettings.instance.UCBatchingEnabled) return;

            if (value)
            {
                Batch(false, updatedBuilding, added); // clear the old data

                if (initializedBatch && updatedBuilding != null)
                {
                    BatchUtility.UpdateBatchData(GetUpdatedBatchData(updatedBuilding, added), added, ref batchData);
                }
                else if (!initializedBatch)
                {
                    batchData = BatchUtility.CompileInitialBatchData(batchedFilters.ToArray(), true);
                }

                Mesh mesh;

                for (int i = 0; i < batchData.Count; i++)
                {
                    var go = new GameObject();
                    go.name = "Group Batched Collider";
                    go.gameObject.layer = LayersData.DefaultBuildingLayer;
                    go.transform.parent = transform;
                    go.transform.position = transform.position;

                    var buildingBatch = go.AddComponent<BaseBuildingBatcher>();
                    buildingBatch.group = this;

                    mesh = new Mesh();
                    
                    mesh.CombineMeshes(batchData[i].combineInstances.ToArray());

                    /* COLLIDER BATCHING CURRENTLY DISABLED.
                    var meshCollider = go.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                    */

                    if (batchData[i].Materials != null)
                    {
                        var renderer = go.AddComponent<MeshRenderer>();
                        var filter = go.AddComponent<MeshFilter>();

                        filter.mesh = mesh;
                        renderer.materials = batchData[i].Materials;
                    }

                    batchInstances.Add(go.transform);

                    if(OnBatchDoneEvent != null)
                    {
                        OnBatchDoneEvent(go, mesh);
                    }
                }

                initializedBatch = true;
            }
            else
            {
                for(int i = 0; i < batchInstances.Count; i++)
                {
                    if(batchInstances[i] != null)
                        Destroy(batchInstances[i].gameObject);
                }
                batchInstances.Clear();

            }

            if(OnGroupBatchedEvent != null)
            {
                OnGroupBatchedEvent(value);
            }
        }

        /// <summary>
        /// Return a building from a point inside the group.
        /// </summary>
        /// <param name="pos">The hit point</param>
        /// <returns>The building that containes this point, used for batching.</returns>
        public virtual BaseBuilding ReturnBatchedBuilding(Vector3 pos)
        {
            BaseBuilding building;
            MeshRenderer collider;
            MeshRenderer[] colliders;

            for (int i = 0; i < groupBuildings.Count; i++)
            {
                building = groupBuildings.ElementAt(i);
                colliders = building.GetComponentsInChildren<MeshRenderer>(true);

                for (int b = 0; b < colliders.Length; b++)
                {
                    collider = colliders[b];

                    if (collider.bounds.Contains(pos))
                    {
                        return building;
                    }
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Save all group data into a binary file.
        /// </summary>
        /// <returns>The saved data</returns>
        public virtual BaseUCSaveData Save()
        {
            BuildingGroupSaveData data = new BuildingGroupSaveData();
            data.GUID = "";
            data.buildingsData = new List<BuildingSaveData>();
            BaseBuilding building;

            for(int i = 0; i < groupBuildings.Count; i++)
            {
                building = groupBuildings.ElementAt(i);

                data.buildingsData.Add(new BuildingSaveData(building.transform.position, building.transform.rotation,
                    building.placedOn == null ? -1 : building.placedOn.uid, building.health, building.prefabID, building.uid));
            }

            return data;
        }
    }

    /// <summary>
    /// A class that handles all batch data
    /// </summary>
    public class BatchClass
    {
        public Material[] Materials;
        public List<MeshFilter> Filters = new List<MeshFilter>();
        public List<CombineInstance> combineInstances = new List<CombineInstance>();
        public int totalVertexAmount = 0;

        public BatchClass(Material[] _materials, List<MeshFilter> _filters)
        {
            this.Materials = _materials;
            this.Filters = _filters;
        }
        public BatchClass(Material[] _materials)
        {
            this.Materials = _materials;
            this.Filters = new List<MeshFilter>();
        }

        public void AddFilter(MeshFilter filter, CombineInstance instance)
        {
            Filters.Add(filter);

            totalVertexAmount += filter.mesh.vertexCount;
            combineInstances.Add(instance);
        }

        public void RemoveFilter(int index)
        {
            MeshFilter filter;

            if (Filters.Count > index)
            {
                filter = Filters[index];

                Filters.RemoveAt(index);
                combineInstances.RemoveAt(index);
                totalVertexAmount -= filter.mesh.vertexCount;
            }
        }

        /// <summary>
        /// Are the materials contained?
        /// </summary>
        /// <param name="materials">the materials</param>
        /// <returns>are the materials contained?</returns>
        public bool Containes(Material[] materials)
        {
            if(this.Materials.Length == materials.Length)
            {
                for (int i = 0; i < materials.Length ; i++)
                {
                    if (materials[i].name != this.Materials[i].name)
                        return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

    }

}