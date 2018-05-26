using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using uConstruct.Sockets;
using uConstruct.Conditions;
using uConstruct.Core.Physics;
using uConstruct.Core.Templates;
using uConstruct.Core.Blueprints;


namespace uConstruct
{
    public delegate void OnSnappedToSocket(BaseSocket socket);
    public delegate void OnLostSnapToSocket(BaseSocket socket);

    public delegate void OnPlacedOnChanged(BaseBuilding oldValue, BaseBuilding newValue);
    public delegate void OnBuildingGroupChanged(BaseBuildingGroup group);

    public delegate void OnNetworkInstanceLoaded(BaseBuilding building);
    public delegate void OnNetworkinstancePacked(BaseBuilding building);

    public delegate void OnPlaced();
    public delegate void OnDestroy();
    public delegate void OnDeattach();

    public delegate void OnMaterialColorChanged(BuildingMaterialData color);
    public delegate void OnHealthChanged();

    /// <summary>
    /// A base building script that has all building methods.
    /// Incase of making an another type of use for the building like a mine/ a networked building please inherite from this class.
    /// </summary>
    [ExecuteInEditMode]
    public partial class BaseBuilding : MonoBehaviour, IBuilding
    {
        #region BuildingData
        public BuildingType buildingType;
        public PlacingRestrictionType placingRestrictionType;
        [SerializeField]
        bool _batchBuilding;
        public bool rotateWithSlope = false;

        [System.NonSerialized]
        public BuildingMaterialData latestMaterialData = new BuildingMaterialData(null, Color.white);

        public bool rotateToFit = false;
        public Axis rotateAxis = Axis.X;
        public float rotateThreshold = 90f;
        public int rotationSteps = 4;

        public bool canBeRotated = true;
        public int rotationAmount = 90;

        public Vector3 placingOffset;
        public Vector3 rotationOffset;

        bool isSocketPlaceType
        {
            get { return FlagsHelper.IsBitSet<PlacingRestrictionType>(placingRestrictionType, PlacingRestrictionType.SocketBased); }
        }
        bool isFreePlaceType
        {
            get { return FlagsHelper.IsBitSet<PlacingRestrictionType>(placingRestrictionType, PlacingRestrictionType.FreelyBased); }
        }

        public bool isPlaced
        {
            get { return !isBeingPlaced; }
        }

        public bool batchBuilding
        {
            get { return _batchBuilding; }
            set
            {
                if(_batchBuilding != value)
                {
                    _batchBuilding = value;

                    if(Application.isPlaying)
                    {
                        if(hasGroup)
                        {
                            buildingGroup.PopulateBatchedFilters(this, value);
                            buildingGroup.Batch(true, this, value);

                            EnableRenderings(!value);
                        }
                    }
                }
            }
        }

        MeshRenderer[] meshRenderers;
        Collider[] colliders;
        List<BuildingMaterialData> originalMats = new List<BuildingMaterialData>();

        private bool _isBeingPlaced = true;
        public bool isBeingPlaced
        {
            get { return _isBeingPlaced; }
            set
            {
                bool changed = _isBeingPlaced != value;

                _isBeingPlaced = value;
                InitiateBuildingData();

                if(!isBeingPlaced && changed)
                {
                    uid = globalCount;
                    globalCount++;
                }
            }
        }

        private int _maxHealth = 100;
        public int maxHealth
        {
            get { return _maxHealth; }
            set { _maxHealth = Mathf.Clamp(value, 0, int.MaxValue); }
        }

        public int _health = 100;
        public virtual int health
        {
            get { return _health; }
            set
            {
                value = Mathf.Clamp(value, 0, maxHealth);

                if(value != health)
                {
                    _health = value;

                    if (OnHealthChangedEvent != null)
                    {
                        OnHealthChangedEvent();
                    }

                    if(health == 0)
                    {
                        DestroyBuilding();
                    }
                }
            }
        }
        #endregion

        #region SocketVariables
        BaseSocket[] _sockets = new BaseSocket[0];
        public BaseSocket[] sockets
        {
            get { return _sockets; }
            set
            {
                _sockets = value;

                if(Application.isPlaying)
                    InitiateBuildingData();
            }
        }

        public BaseSnapPoint currentSnapPoint;
        BaseSnapPoint[] _snapPoints = new BaseSnapPoint[0];
        public BaseSnapPoint[] snapPoints
        {
            get { return _snapPoints; }
            set
            {
                _snapPoints = value;

                if (Application.isPlaying)
                    InitiateBuildingData();
            }
        }

        public BaseSocket SnappedTo;

        BaseBuilding _placedOn;
        public BaseBuilding placedOn
        {
            get
            {
                return _placedOn;
            }
            set
            {
                if(_placedOn != value)
                {
                    if(OnPlacedOnChangedEvent != null)
                        OnPlacedOnChangedEvent(_placedOn, value);

                    _placedOn = value;
                }
            }
        }
        #endregion

        #region ConditionsVariables
        BaseCondition[] _conditions;
        BaseCondition[] conditions
        {
            get { return _conditions; }
            set
            {
                _conditions = value;

                if (Application.isPlaying)
                    InitiateBuildingData();
            }
        }
        #endregion

        #region BuildingGroupVariables
        protected BaseBuildingGroup _buildingGroup;
        public BaseBuildingGroup buildingGroup
        {
            get { return _buildingGroup; }
            set
            {
                if(_buildingGroup != value)
                {
                    if (_buildingGroup != null)
                    {
                        _buildingGroup.RemoveBuilding(this);
                    }

                    _buildingGroup = value;

                    if(OnGroupChangedEvent != null)
                    {
                        OnGroupChangedEvent.Invoke(value);
                    }
                }
            }
        }

        public bool hasGroup
        {
            get { return buildingGroup != null; }
        }

        /// <summary>
        /// The building group which will be used for this object. IMPORTANT - make sure that it inherits from BaseBuildingGroup or it will throw an error!
        /// </summary>
        public virtual System.Type buildingGroupType
        {
            get
            {
                return typeof(BaseBuildingGroup);
            }
        }
        #endregion

        #region Database
        public int prefabID = -1;

        /// <summary>
        /// The global count of the assignable uid.
        /// </summary>
        private static int globalCount;

        /// <summary>
        /// A uid which is used to locate this building. persists through saving BUT, DOESNT persist through network!. (networkID -> networking persisted id).
        /// </summary>
        private int _uid = -1;
        public int uid
        {
            get { return _uid; }
            set
            {
                if (_uid == -1)
                {
                    _uid = value;
                }
            }
        }
        #endregion

        #region Templates
        public List<Template> templates = new List<Template>();
        #endregion

        #region Events
        public event OnSnappedToSocket OnSnappedToSocketEvent;
        public event OnLostSnapToSocket OnLostSnapToSocketEvent;

        public event OnPlacedOnChanged OnPlacedOnChangedEvent;

        public event OnPlaced OnPlacedEvent;
        public event OnDestroy OnDestroyEvent;
        public event OnDeattach OnDeattachEvent;

        public event OnBuildingGroupChanged OnGroupChangedEvent;

        public event OnMaterialColorChanged OnMaterialColorChangedEvent;

        public event OnHealthChanged OnHealthChangedEvent;

        public static event OnNetworkinstancePacked OnNetworkInstancePackedEvent;
        public static event OnNetworkInstanceLoaded OnNetworkInstanceLoadedEvent;

        #endregion

        /// <summary>
        /// Called on awake, initiates all methods.
        /// Please note this is also called on editor.
        /// </summary>
        protected virtual void Awake()
        {
            _sockets = GetComponentsInChildren<BaseSocket>();
            conditions = GetComponentsInChildren<BaseCondition>();
            _snapPoints = GetComponentsInChildren<BaseSnapPoint>();

            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            if (Application.isPlaying)
            {
                AssignOriginalColors();

                if (!LayersData.BuildingLayers.Contains(LayerMask.LayerToName(gameObject.layer)))
                    gameObject.layer = LayersData.DefaultBuildingLayer;

                OnLostSnapToSocketEvent += LostSnapToSocket;
                OnSnappedToSocketEvent += SnappedToSocket;
                OnPlacedEvent += BuildingPlaced;
                OnDestroyEvent += BuildingDestroyed;
                OnGroupChangedEvent += BuildingGroupChanged;
                OnDeattachEvent += BuildingDeattached;

                isBeingPlaced = true;
            }
        }

        /// <summary>
        /// Calls when creating or placing this building, used to disappear sockets on need or reappear them on need.
        /// </summary>
        protected virtual void InitiateBuildingData()
        {
            ActivateSockets(!isBeingPlaced, true);
            ActivateConditions(isBeingPlaced, false);
            ActivateColliders(!isBeingPlaced);

            ActivateSnapPoints(isBeingPlaced);
        }

        /// <summary>
        /// Change socket's state, disabled or enabled ( used on init, and also can be used for AOI [ disable all sockets when far away from the building ] ).
        /// </summary>
        /// <param name="value"></param>
        public void ActivateSockets(bool value, bool forced)
        {
            BaseSocket socket;

            for (int i = 0; i < sockets.Length; i++)
            {
                socket = sockets[i];

                if (socket.isActive != value)
                {
                    if (forced)
                        socket.ForceEnable(value);
                    else
                        socket.EnableSocket(value);
                }
            }
        }

        /// <summary>
        /// Change condition's state, disabled or enabled ( used on init, and also can be used for AOI [ disable all conditions when far away from the building to stay away physics limit ] ).
        /// </summary>
        /// <param name="value"></param>
        public void ActivateConditions(bool value, bool force)
        {
            BaseCondition condition;

            for (int i = 0; i < conditions.Length; i++)
            {
                condition = conditions[i];

                if ((!force && condition.gameObject.activeInHierarchy != value && condition.DisableOnPlace) || (force && !condition.DisableOnPlace && condition.gameObject.activeInHierarchy != value)) // if isnt forced, disable/enable all objects that needs to be disabled at init, if its forced then disable and enable only the ones that arent disabled after init.
                    condition.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Activate the colliders in the building.
        /// </summary>
        /// <param name="value">Enable the colliders or disable them ?</param>
        public void ActivateColliders(bool value)
        {
            if(colliders == null) // initiate initial data
            {
                List<Collider> temp = new List<Collider>();
                colliders = GetComponentsInChildren<Collider>();
                Collider collider;

                for(int i = 0; i < colliders.Length; i++)
                {
                    collider = colliders[i];

                    if (collider.gameObject.GetComponent<BaseSocket>() != null || collider.gameObject.GetComponent<BaseCondition>() != null) continue;

                    temp.Add(collider);
                }

                colliders = temp.ToArray();
            }

            for(int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = value;
            }

        }

        /// <summary>
        /// Activate the snap points in the building.
        /// </summary>
        /// <param name="value">Enable the snappoints or disable them ?</param>
        public void ActivateSnapPoints(bool value)
        {
            if (snapPoints == null) // initiate initial data
            {
                snapPoints = GetComponentsInChildren<BaseSnapPoint>();
            }

            for (int i = 0; i < snapPoints.Length; i++)
            {
                snapPoints[i].enabled = value;
            }

        }

        /// <summary>
        /// Called from the player controller, called to check the placing of this building and see if it fits the target or not, receives a raycastHit.
        /// </summary>
        /// <param name="hits">A raycast hit generated from a raycast that is being called on the player/ from where this script is called from.</param>
        /// <param name="offset">The offset of the placed building.</param>
        /// <returns>is this building placeable ?, can be used to change the material to a certain color/ what ever.</returns>
        public virtual bool HandlePlacing(UCPhysicsHitsArray hits)
        {
            if (!isBeingPlaced) return false;

            UCPhysicsHit hit;
            BaseSocket socket;
            bool isFit = false;

            for (int i = 0; i < hits.Count; i++)
            {
                hit = hits[i];
                
                socket = hit.transform.GetComponent<BaseSocket>();

                if (socket == null)
                    socket = SnappedTo;

                if (socket)
                {
                    placedOn = socket.building;

                    if (isSocketPlaceType) // socket based placing
                    {
                        isFit = socket.IsFit(this, PlacingRestrictionType.SocketBased);

                        if (isFit)
                        {
                            HandleSnapPlace(hit, socket);
                        }
                    }

                    if (isFreePlaceType && !isFit) // check if our placing type is also/ just freely based. and if its also freely based then make sure we didnt find an appropriate target in the sockets search.
                    {
                        currentSnapPoint = null;
                        SnappedTo = null;

                        isFit = socket.IsFit(this, PlacingRestrictionType.FreelyBased);

                        if (isFit)
                        {
                            HandleFreePlace(hit, socket);
                        }
                    }

                    if (isFit)
                    {
                        isFit = CheckConditions();

                        if (isFit) break;
                        else continue;
                    }
                    else
                    {
                        if (socket.isHoverTarget && !FlagsHelper.IsBitSet<BuildingType>(socket.receiveType, buildingType))
                        {
                            transform.position = hit.point + placingOffset;
                        }
                        continue;
                    }
                }
                else
                {
                    transform.position = hit.point + placingOffset;
                }
            }

            return isFit;
        }

        /// <summary>
        /// Handle the snap placement of the building.
        /// </summary>
        /// <param name="hit">hit data</param>
        /// <param name="socket">Our socket</param>
        /// <param name="offset">The offset of the placed building.</param>
        public virtual void HandleSnapPlace(UCPhysicsHit hit, BaseSocket socket)
        {
            BaseSnapPoint snap = BaseSnapPoint.ReturnClosest(socket.building == null ? new BaseSnapPoint[0] : socket.building.snapPoints, hit.point, buildingType);

            if (snap != null && currentSnapPoint == null)
            {
                currentSnapPoint = snap;

                transform.position = socket.transform.position + placingOffset;

                currentSnapPoint.Snap(this.transform);
            }
            else if (currentSnapPoint != null && snap != null)
            {
                currentSnapPoint = snap;

                transform.position = socket.transform.position + placingOffset;
                currentSnapPoint.Snap(this.transform);
            }
            else if (snap == null)
            {
                transform.position = socket.transform.position + placingOffset;
            }

            if (socket != SnappedTo)
            {
                currentSnapPoint = null;
                SnappedTo = socket;

                transform.rotation = hit.transform.rotation;
                transform.eulerAngles += rotationOffset;

                if (rotateToFit && !CheckConditions())
                {
                    for (int b = 1; b <= rotationSteps; b++)
                    {
                        transform.Rotate(rotateAxis == Axis.X ? (rotateThreshold) : 0,
                                                                rotateAxis == Axis.Y ? (rotateThreshold) : 0,
                                                                rotateAxis == Axis.Z ? (rotateThreshold) : 0);

                        if (CheckConditions())
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle the free placement of the building.
        /// </summary>
        /// <param name="hit">hit data</param>
        /// <param name="socket">Our socket</param>
        /// <param name="offset">The offset of the placed building.</param>
        public virtual void HandleFreePlace(UCPhysicsHit hit, BaseSocket socket)
        {
            transform.position = hit.point + placingOffset;

            if (rotateWithSlope)
            {
                if (hit.normal != -Vector3.one)
                {
                    transform.up = hit.normal;
                    transform.eulerAngles += rotationOffset;
                }
                else
                {
                    transform.eulerAngles = socket.transform.eulerAngles + rotationOffset;
                }
            }
        }

        /// <summary>
        /// Another way to run Handle placing method but with raycast hits instead of the custom physics library.
        /// </summary>
        /// <param name="offset">The offset of the placed building.</param>
        /// <param name="hits">hits array.</param>
        /// <returns>did we place it correctly ?</returns>
        public virtual bool HandlePlacing(params RaycastHit[] hits)
        {
            UCPhysicsHitsArray _hits = new UCPhysicsHitsArray();
            RaycastHit hit;

            for(int i = 0; i < hits.Length; i++)
            {
                hit = hits[i];

                _hits.AddToList(new UCPhysicsHit() { distance = hit.distance, normal = hit.normal, point = hit.point, transform = hit.transform });
            }

            return HandlePlacing(_hits);
        }

        /// <summary>
        /// A method that handles the placing of the building, this is a virtual method so you can implement your 3d party libraries implemention here.
        /// </summary>
        public virtual void PlaceBuilding()
        {
            isBeingPlaced = false;
            ResetMaterialColors();

            if (SnappedTo != null)
            {
                if (OnSnappedToSocketEvent != null)
                {
                    OnSnappedToSocketEvent(SnappedTo);
                }
            }

            if (placedOn != null && placedOn.hasGroup && !hasGroup)
            {
                placedOn.buildingGroup.AddBuilding(this);
            }
            else
            {
                if (!hasGroup)
                {
                    buildingGroup = BaseBuildingGroup.CreateGroup(transform.position, buildingGroupType);
                    buildingGroup.AddBuilding(this);
                }
                else if(hasGroup && !buildingGroup.buildings.Contains(this))
                {
                    buildingGroup.AddBuilding(this);
                }
            }

            if (OnPlacedEvent != null)
                OnPlacedEvent();

        }

        /// <summary>
        /// Handles the building deattaching,
        /// removes it from the group and updates the sockets that it is no longer snapped.
        /// </summary>
        public virtual void DeAttachBuilding()
        {
            EnableRenderings(true);
            if (SnappedTo != null)
            {
                if (SnappedTo != null && OnLostSnapToSocketEvent != null)
                {
                    OnLostSnapToSocketEvent(SnappedTo);
                }
            }

            if (hasGroup)
            {
                buildingGroup.RemoveBuilding(this);
                buildingGroup = null;
            }

            if (OnDeattachEvent != null)
                OnDeattachEvent();
        }

        /// <summary>
        /// Assign the original colors of the materials
        /// </summary>
        public virtual void AssignOriginalColors()
        {
            MeshRenderer renderer;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                renderer = meshRenderers[i];

                for (int b = 0; b < renderer.sharedMaterials.Length; b++)
                {
                    originalMats.Add(new BuildingMaterialData(renderer.sharedMaterials[b], renderer.sharedMaterials[b].color));
                }
            }
        }

        /// <summary>
        /// Reset the materials color to initial colors, used to reset preview material after placement.
        /// </summary>
        public virtual void ResetMaterialColors()
        {
            MeshRenderer currentRenderer;
            Material[] materialsInstances;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                currentRenderer = meshRenderers[i];

                materialsInstances = currentRenderer.materials;

                for (int b = 0; b < materialsInstances.Length; b++)
                {
                    materialsInstances[b] = originalMats[b + i].material;
                    materialsInstances[b].color = originalMats[b + i].color;
                }

                currentRenderer.materials = materialsInstances;
            }
        }

        /// <summary>
        /// Changing all the materials into a chosen mat.
        /// </summary>
        /// <param name="color"> the material you want to change to. </param>
        public virtual void HandleMaterial(BuildingMaterialData mat)
        {
            latestMaterialData = mat;

            MeshRenderer currentRenderer;
            Material[] materialsInstances;

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                currentRenderer = meshRenderers[i];

                materialsInstances = currentRenderer.materials;

                // please note : we have to create a new material instances as unity doesnt allow direct editing of materials.

                for (int b = 0; b < materialsInstances.Length; b++)
                {
                    if(mat.material !=  null)
                        materialsInstances[b] = mat.material;

                    materialsInstances[b].color = mat.color;
                }

                currentRenderer.materials = materialsInstances;
            }

            if (OnMaterialColorChangedEvent != null)
                OnMaterialColorChangedEvent(mat);
        }

        /// <summary>
        /// Check for all the conditions in the object and make sure they all return true.
        /// </summary>
        /// <returns>Are all conditions meet ?</returns>
        public virtual bool CheckConditions()
        {
            BaseCondition condition;

            for (int i = 0; i < conditions.Length; i++)
            {
                condition = conditions[i];

                if (!condition.CheckCondition())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a socket from the building by position.
        /// </summary>
        /// <param name="pos">The position of the socket</param>
        /// <param name="targetType">What is the targeted building type? this will make sure you only check the right sockets and save perf.</param>
        /// <returns>The socket that is placed on this position</returns>
        public virtual BaseSocket ReturnSocket(Vector3 pos, BuildingType? targetType)
        {
            BaseSocket socket;

            for (int i = 0; i < sockets.Length; i++)
            {
                socket = sockets[i];

                if (!targetType.HasValue || FlagsHelper.IsBitSet<BuildingType>(socket.receiveType, targetType.Value))
                {
                    //if (pos.FloatPercisionEquals(socket.transform.position)) // float percised check

                    if(pos == socket.transform.position)
                    {
                        return socket;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Destroy this building
        /// </summary>
        /// <param name="hit">The hit data of the raycast</param>
        public virtual void DestroyBuilding()
        {
            DeAttachBuilding();

            if (OnDestroyEvent != null)
                OnDestroyEvent();

            this.enabled = false;

            Destroy(this.gameObject);
        }

        /// <summary>
        /// Called when the building group of the building changes
        /// </summary>
        /// <param name="group">the new building group</param>
        public virtual void BuildingGroupChanged(BaseBuildingGroup group)
        {
            if(group != null)
            {
                group.HandleOccupiedSockets(this);
            }
        }

        /// <summary>
        /// Called when ever a building was created in the group/ added to the group.
        /// </summary>
        /// <param name="building">the building</param>
        public virtual void GroupBuildingAdded(BaseBuilding building)
        {
            BaseSocket socket;

            socket = ReturnSocket(building.transform.position, building.buildingType);

            if (socket != null)
            {
                socket.isOccupied = true;
            }
        }
        
        /// <summary>
        /// Called when ever a building was created in the group/ added to the group.
        /// </summary>
        /// <param name="building">the building</param>
        public virtual void GroupBuildingRemoved(BaseBuilding building)
        {
            BaseSocket socket;

            socket = ReturnSocket(building.transform.position, building.buildingType);
            
            if(socket != null)
            {
                socket.isOccupied = false;
            }
        }

        /// <summary>
        /// Enable all building's renderings, used mostly to restore to its old state before being batched.
        /// <param name="value">Enable?</param>
        /// </summary>
        public virtual void EnableRenderings(bool value)
        {
            MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>(true);

            for(int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = value;
            }

        }

        #region Events

        /// <summary>
        /// An event callback thats called when the building is snapped to a socket
        /// </summary>
        /// <param name="socket">the socket we snapped to</param>
        protected virtual void SnappedToSocket(BaseSocket socket)
        {
            socket.BuildingSnapped(true);
        }
        /// <summary>
        /// An event callback thats called when the building lost snap to a socket.
        /// </summary>
        /// <param name="socket">the socket we lost snap to</param>
        protected virtual void LostSnapToSocket(BaseSocket socket)
        {
            socket.BuildingSnapped(false);
        }
        /// <summary>
        /// An event callback thats called when building is placed
        /// </summary>
        protected virtual void BuildingPlaced()
        {
            //SOverlapThreshold.DetectOverlap(transform.position, sockets);
        }
        /// <summary>
        /// An event callback thats called when the building is destroyed
        /// </summary>
        protected virtual void BuildingDestroyed()
        {
        }

        /// <summary>
        /// Called when the building is deattached.
        /// </summary>
        public virtual void BuildingDeattached()
        {
        }

        #endregion

        #region Templates

        /// <summary>
        /// Add a template
        /// </summary>
        /// <param name="template">What template to add</param>
        public void AddTemplate(GameObject template)
        {
            var templateInstance = Instantiate<GameObject>(template.gameObject);
            templateInstance.name = template.name; //remove clone...

            templates.Add(templateInstance.GetComponent<Template>());

            templateInstance.transform.parent = this.transform;
            templateInstance.transform.localPosition = Vector3.zero;
        }

        /// <summary>
        /// Remove a template
        /// </summary>
        /// <param name="template">what template to remove</param>
        public void RemoveTemplate(Template template)
        {
            if(templates.Contains(template))
            {
                templates.Remove(template);
                DestroyImmediate(template.gameObject, true);
            }
        }

        #endregion
    }

    /// <summary>
    /// A base building script that has all building methods.
    /// This class will handle the creation of sockets, conditions and snap points runtime/editor wise.
    /// </summary>
    public partial class BaseBuilding : MonoBehaviour, IBlueprintItem, IUTCPhysicsIgnored
    {
        #region Static Data
        public const string SocketParentName = "Sockets";
        public const string ConditionsParentName = "Conditions";
        public const string SnapPointParentName = "Snap Points";
        #endregion

        #region CreationMethods
        /// <summary>
        /// Create a socket, can be used on both runtime and editor.
        /// </summary>
        /// <param name="name">The name of the socket, can be changed later on.</param>
        /// <param name="socketAnchor">What anchor will this socket be created on, used for mostly editor.</param>
        /// <param name="previewGameObject">Use a preview game object for the socket creation, used mostly for editor.</param>
        /// <param name="receive">what kind of buildings will this socket receive?, can be changed later on.</param>
        /// <param name="restriction">what placing type will this socket have?, can be changed later on.</param>
        /// <returns>Returns the created socket.</returns>
        public BaseSocket CreateSocket(string name, SocketPositionAnchor socketAnchor, GameObject previewGameObject, BuildingType receive, PlacingRestrictionType restriction)
        {
            GameObject go = new GameObject();
            BaseSocket socket = go.AddComponent<BaseSocket>();
            socket.InitiateComponents(previewGameObject, transform.lossyScale);
            socket.receiveType = receive;
            socket.placingType = restriction;

            go.name = name;

            #if UNITY_EDITOR
            UnityEditor.Selection.activeObject = go;
            #endif

            //first parent to the building and use its local position cords and then parent to the socket parent.
            go.transform.parent = transform;
            go.transform.position = ReturnPosition(socketAnchor, transform);
            go.transform.parent = ReturnParent(SocketParentName, true);

            AddSocket(socket);

            return socket;
        }
        /// <summary>
        /// Create a condition, can be used on both runtime and editor.
        /// </summary>
        /// <param name="name">The name of the condition, can be changed later on.</param>
        /// <param name="anchor">What anchor will this condition be created on, used for mostly editor.</param>
        /// <param name="condition">What condition this condition will have? cant use an abstract type.</param>
        /// <returns></returns>
        public BaseCondition CreateCondition(string name, SocketPositionAnchor anchor, System.Type condition)
        {
            GameObject go = new GameObject();
            BaseCondition conditionScript;

            if (condition.GetType().IsAbstract)
            {
                Debug.LogError("Cannot create condition because the condition class is abstract.");
                return null;
            }

            go.AddComponent(condition);
            go.name = name;

            conditionScript = go.GetComponent<BaseCondition>();
            conditionScript.rootBuilding = this;

            #if UNITY_EDITOR
            UnityEditor.Selection.activeObject = go;
            #endif

            //first parent to the building and use its local position cords and then parent to the condition parent.
            go.transform.parent = transform;
            go.transform.position = ReturnPosition(anchor, transform);
            go.transform.parent = ReturnParent(ConditionsParentName, true);

            AddCondition(conditionScript);

            return conditionScript;
        }

        /// <summary>
        /// Create a condition, can be used on both runtime and editor.
        /// </summary>
        /// <param name="name">The name of the condition, can be changed later on.</param>
        /// <param name="anchor">What anchor will this condition be created on, used for mostly editor.</param>
        /// <param name="type">What buildings will this snap point receive?</param>
        /// <returns></returns>
        public BaseSnapPoint CreateSnapPoint(string name, SocketPositionAnchor anchor, BuildingType type)
        {
            GameObject go = new GameObject();
            var snapScript = go.AddComponent<BaseSnapPoint>();
            go.name = name;

            snapScript.receiveType = type;
            snapScript.building = this;

            #if UNITY_EDITOR
            UnityEditor.Selection.activeObject = go;
            #endif

            //first parent to the building and use its local position cords and then parent to the condition parent.
            go.transform.parent = transform;
            go.transform.position = ReturnPosition(anchor, transform);
            go.transform.parent = ReturnParent(SnapPointParentName, true);

            return snapScript;
        }
        #endregion

        #region HandleMethods
        /// <summary>
        /// Add a socket to the building.
        /// </summary>
        /// <param name="socket"></param>
        public void AddSocket(BaseSocket socket)
        {
            var temp = sockets.ToList();
            temp.Add(socket);
            this._sockets = temp.ToArray();
        }
        /// <summary>
        /// Add a condition to the building.
        /// </summary>
        /// <param name="condition"></param>
        public void AddCondition(BaseCondition condition)
        {
            var temp = conditions.ToList();
            temp.Add(condition);
            this.conditions = temp.ToArray();
        }
        #endregion

        #region CreationHelpers
        /// <summary>
        /// Return a position based upon an anchor
        /// </summary>
        /// <param name="anchor">what anchor to use</param>
        /// <returns>A position based upon the anchor.</returns>
        public Vector3 ReturnPosition(SocketPositionAnchor anchor, Transform transform)
        {
            switch (anchor)
            {
                case SocketPositionAnchor.Center:
                    return transform.position;

                case SocketPositionAnchor.Back:
                case SocketPositionAnchor.Forward:
                    return (anchor == SocketPositionAnchor.Forward ? transform.GetForward() : transform.GetBackwards());

                case SocketPositionAnchor.ForwardRight:
                case SocketPositionAnchor.ForwardLeft:
                case SocketPositionAnchor.BackLeft:
                case SocketPositionAnchor.BackRight:
                    return new Vector3((anchor == SocketPositionAnchor.ForwardRight || anchor == SocketPositionAnchor.BackRight ? transform.GetRight().x : transform.GetLeft().x),
                        transform.position.y,
                        (anchor == SocketPositionAnchor.ForwardRight || anchor == SocketPositionAnchor.ForwardLeft ? transform.GetForward().z : transform.GetBackwards().z));

                case SocketPositionAnchor.Right:
                case SocketPositionAnchor.Left:
                    return anchor == SocketPositionAnchor.Right ? transform.GetRight() : transform.GetLeft();

                case SocketPositionAnchor.Up:
                case SocketPositionAnchor.Down:
                    return anchor == SocketPositionAnchor.Up ? transform.GetUp() : transform.GetDown();

                default:
                    return transform.position;
            }
        }
        /// <summary>
        /// Return the parent of the specific condition/socket.
        /// </summary>
        /// <param name="isSocket">Is this a socket?.</param>
        /// <param name="createIfNotFound">Create an instance if parent cannot be found</param>
        /// <returns>The parent of the socket/ condition.</returns>
        public Transform ReturnParent(string parentName, bool createIfNotFound)
        {
            var Child = transform.Find(parentName);

            if (Child == null && createIfNotFound)
            {
                GameObject go = new GameObject();

                go.transform.parent = transform;
                go.name = parentName;
                go.transform.localPosition = Vector3.zero;
                Child = go.transform;
            }

            return Child;
        }
        #endregion

        #region BlueprintItemInterface

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public virtual BlueprintData Pack()
        {
            return new BuildingBlueprintData(this);
        }

        /// <summary>
        /// Our priority.
        /// </summary>
        public virtual int priority
        {
            get
            {
                return 1;
            }
        }

        #endregion

        #region Physics
        /// <summary>
        /// Will our building be ignored?
        /// </summary>
        public bool ignore
        {
            get { return isBeingPlaced; }
        }
        #endregion

        #region Networking

        public static void CallPack(BaseBuilding building)
        {
            if (OnNetworkInstancePackedEvent != null)
                OnNetworkInstancePackedEvent(building);
        }

        public static void CallLoad(BaseBuilding building)
        {
            if (OnNetworkInstanceLoadedEvent != null)
                OnNetworkInstanceLoadedEvent(building);
        }

        #endregion

        #region Static-Methods
        /// <summary>
        /// Safely removes all of the buildings in the scene making all of the errors that will be caused if you normally remove the buildings disappear.
        /// </summary>
        public static void ResetBuildingsInScene()
        {
            BaseBuilding[] buildings = GameObject.FindObjectsOfType<BaseBuilding>();
            BaseBuilding currentBuilding;
            
            for(int i = 0; i < buildings.Length; i++)
            {
                currentBuilding = buildings[i];

                currentBuilding.DestroyBuilding();
            }

            Debug.Log("Reseting scene-buildings done.");
        }
        #endregion
    }

    [System.Serializable]
    public class BuildingBlueprintData : BlueprintData
    {
        public BuildingType buildingType;
        public PlacingRestrictionType placingRestrictionType;
        public bool batchBuilding;
        public bool rotateWithSlope;

        public bool rotateToFit;
        public Axis rotateAxis;
        public float rotateThreshold;
        public int rotationSteps;

        public BuildingBlueprintData(BaseBuilding building)
        {
            building.transform.localRotation = Quaternion.identity; // reset rotation in order to avoid wrong scale.

            this.rotation = building.transform.rotation;

            this.scale = building.transform.GetRendererSize();

            building.transform.localRotation = (Quaternion)this.rotation; // apply old rotation after reset.

            buildingType = building.buildingType;
            placingRestrictionType = building.placingRestrictionType;
            batchBuilding = building.batchBuilding;
            rotateWithSlope = building.rotateWithSlope;

            rotateToFit = building.rotateToFit;
            rotateAxis = building.rotateAxis;
            rotateThreshold = building.rotateThreshold;
            rotationSteps = building.rotationSteps;
        }

        public override void UnPack(GameObject target)
        {
            var renderersSum = target.transform.GetRendererSize();
            Vector3 localSum = (Vector3)this.scale;

            target.transform.localScale = new Vector3(localSum.x / renderersSum.x, localSum.y / renderersSum.y, localSum.z / renderersSum.z);
            target.transform.rotation = (Quaternion)this.rotation;

            BaseBuilding building = target.GetComponent<BaseBuilding>();

            if (building == null) building = target.AddComponent<BaseBuilding>();

            building.buildingType = this.buildingType;
            building.placingRestrictionType = this.placingRestrictionType;
            building.batchBuilding = this.batchBuilding;
            building.rotateWithSlope = this.rotateWithSlope;

            building.rotateToFit = this.rotateToFit;
            building.rotateAxis = this.rotateAxis;
            building.rotateThreshold = this.rotateThreshold;
            building.rotationSteps = this.rotationSteps;
        }
    }

    /// <summary>
    /// An enum that containes the 2 placing types that are allowed for a building/ socket.
    /// </summary>
    public enum PlacingRestrictionType
    {
        SocketBased = 1, // Restricted to sockets ( for example a wall).
        FreelyBased = 2 // Can be placed anywhere ( for example a foundation ).
    }

    /// <summary>
    /// Some helper classes for bitmasks
    /// </summary>
    public class FlagsHelper
    {
        
        /// <summary>
        /// Is the value contained inside the enum values?
        /// </summary>
        /// <typeparam name="T">the type of the enum</typeparam>
        /// <param name="values">values of the enum</param>
        /// <param name="value">the specific value you want to check if is included in the enum values</param>
        /// <returns>is it assigned or not ?</returns>
        public static bool IsBitSet<T>(T values, T value) where T : struct
        {
            var a = (int)(object)values;
            var b = (int)(object)value;

            return (a & b) != 0;
        }
         

        /// <summary>
        /// Is the value contained inside the enum values?
        /// </summary>
        /// <typeparam name="T">the type of the enum</typeparam>
        /// <param name="values">values of the enum</param>
        /// <param name="value">the specific value you want to check if is included in the enum values</param>
        /// <returns>is it assigned or not ?</returns>
        public static bool IsBitSet(BuildingType values, BuildingType value)
        {
            return (values & value) != 0;
        }

        /// <summary>
        /// Is this layer inside the layer bitmask?
        /// </summary>
        /// <param name="GameObjectLayer">the layer you want to check</param>
        /// <param name="mask">the mask</param>
        /// <returns>is the layer inside the mask?</returns>
        public static bool isInsideMask(int GameObjectLayer, LayerMask mask)
        {
            return (mask.value & (1 << GameObjectLayer)) > 0;
        }

    }

    /// <summary>
    /// An interface for all buildings.
    /// </summary>
    public interface IBuilding
    {
        /// <summary>
        /// Destroy the building
        /// </summary>
        void DestroyBuilding();
    }

    /// <summary>
    /// Extension methods for the transform
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// The Up corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>UP corner</returns>
        public static Vector3 GetUp(this Transform transform)
        {
            return new Vector3(transform.position.x, transform.position.y + (transform.GetRenderersSum().y / 2) * +1, transform.position.z);
        }
        /// <summary>
        /// The Down corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>DOWN corner</returns>
        public static Vector3 GetDown(this Transform transform)
        {
            return new Vector3(transform.position.x, transform.position.y + (transform.GetRenderersSum().y / 2) * -1, transform.position.z);
        }
        /// <summary>
        /// The Right corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>RIGHT corner</returns>
        public static Vector3 GetRight(this Transform transform)
        {
            return new Vector3(transform.position.x + (transform.GetRenderersSum().x / 2) * +1, transform.position.y, transform.position.z);
        }
        /// <summary>
        /// The Left corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>LEFT corner</returns>
        public static Vector3 GetLeft(this Transform transform)
        {
            return new Vector3(transform.position.x + (transform.GetRenderersSum().x / 2) * -1, transform.position.y, transform.position.z);
        }
        /// <summary>
        /// The Forward corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>FORWARD corner</returns>
        public static Vector3 GetForward(this Transform transform)
        {
            return new Vector3(transform.position.x, transform.position.y, transform.position.z + (transform.GetRenderersSum().z / 2) * 1);
        }

        /// <summary>
        /// The Back corner of the transform
        /// </summary>
        /// <param name="transform">our transform</param>
        /// <returns>BACK corner</returns>
        public static Vector3 GetBackwards(this Transform transform)
        {
            return new Vector3(transform.position.x, transform.position.y, transform.position.z + (transform.GetRenderersSum().z / 2) * -1);
        }
        
        /// <summary>
        /// A comperasion extension method that checks for equality with hard float percision in account
        /// </summary>
        /// <param name="a">the Vector3 instance</param>
        /// <param name="b">the second Vector3 you want to compare to</param>
        /// <returns>Are they equal ?</returns>
        public static bool FloatPercisionEquals(this Vector3 a, Vector3 b)
        {
            return Vector3.SqrMagnitude(a - b) <= 0.1f;
        }

        /// <summary>
        /// Returns the sum size of all the renderers in the transform.
        /// </summary>
        /// <param name="transform">transform instance</param>
        /// <returns>size sum of all renderers in the transform.</returns>
        public static Vector3 GetRenderersSum(this Transform transform)
        {
            MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>(true);

            Bounds bounds = new Bounds();

            for(int i = 0; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.size;
        }

        /// <summary>
        /// Returns solo renderers center, not encapsulated.
        /// </summary>
        /// <param name="transform">transform instance</param>
        /// <returns>center of the chosen target.</returns>
        public static Vector3 GetRendererCenter(this Transform transform)
        {
            MeshRenderer renderer = transform.GetComponentInChildren<MeshRenderer>();
            return renderer == null ? Vector3.zero : renderer.bounds.center;
        }

        /// <summary>
        /// Returns solo renderers size, not encapsulated.
        /// </summary>
        /// <param name="transform">transform instance</param>
        /// <returns>size of the chosen target.</returns>
        public static Vector3 GetRendererSize(this Transform transform)
        {
            MeshRenderer renderer = transform.GetComponentInChildren<MeshRenderer>();
            if (renderer == null)
                renderer = transform.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = transform.GetComponentInParent<MeshRenderer>();

            return renderer == null ? Vector3.zero : renderer.bounds.size;
        }

        /// <summary>
        /// Subside 2 quaternions.
        /// </summary>
        /// <param name="a">our quaternion instance</param>
        /// <param name="b">subside from</param>
        /// <returns>our subsided result</returns>
        public static Quaternion Subside(this Quaternion a, Quaternion b)
        {
            return Quaternion.Euler(a.x - b.x, a.y - b.y, a.z - b.z);
        }
    }

    public enum SocketPositionAnchor
    {
        Right,
        Left,
        Forward,
        ForwardRight,
        ForwardLeft,
        Back,
        BackRight,
        BackLeft,
        Up,
        Down,
        Center
    }

    public enum Axis
    {
        X,
        Y,
        Z
    }

    [System.Serializable]
    public struct BuildingMaterialData
    {
        public Material material;
        public Color color;

        public BuildingMaterialData(Material mat, Color col)
        {
            this.material = mat;
            this.color = col;
        }
    }

}