using UnityEngine;
using System.Collections;
using uConstruct.Conditions;
using uConstruct;
using uConstruct.Core.Physics;
using uConstruct.Core.Templates;
using uConstruct.Core.Blueprints;
using uConstruct.Core.Saving;

using System.Linq;
using System.Collections.Generic;

namespace uConstruct.Sockets
{

    public delegate void OnPreviewObjectChanged(GameObject go);

    /// <summary>
    /// Base class for sockets.
    /// inherite from this class if you want to do any code adjustments.
    /// </summary>
    public partial class BaseSocket : UCPhysicsObject
    {
        #region Runtime-Preview
        public static bool drawSockets = false;

        static Mesh _cubeMesh;
        static Mesh cubeMesh
        {
            get
            {
                if(_cubeMesh == null)
                {
                    GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _cubeMesh = instance.GetComponent<MeshFilter>().mesh;
                    DestroyImmediate(instance);
                }

                return _cubeMesh;
            }
        }

        static Material _socketMat;
        static Material socketMat
        {
            get
            {
                if(_socketMat == null)
                {
                    GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _socketMat = instance.GetComponent<MeshRenderer>().sharedMaterial;
                    _socketMat.SetFloat("_Mode", 2.0f);
                    _socketMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _socketMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    _socketMat.SetInt("_ZWrite", 0);
                    _socketMat.DisableKeyword("_ALPHATEST_ON");
                    _socketMat.EnableKeyword("_ALPHABLEND_ON");
                    _socketMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    _socketMat.renderQueue = 3000;
                    _socketMat.SetColor("_Color", GizmosColor);

                    DestroyImmediate(instance);
                }

                return _socketMat;
            }
        }
        #endregion

        #region Variables
        public BuildingType receiveType;
        public PlacingRestrictionType placingType = PlacingRestrictionType.FreelyBased;

        public static event OnPreviewObjectChanged OnPreviewObjectChangedEvent;

        /// <summary>
        /// Will building be able to hover on this socket ? (mainly used for flat sockets, terrains etc).
        /// </summary>
        public bool isHoverTarget;

        /// <summary>
        /// Draw this specific socket individually.
        /// </summary>
        public bool drawIndividual;

        [SerializeField]
        PreviewBuilding _previewInstance;
        public PreviewBuilding previewInstance
        {
            get
            {
                return _previewInstance;
            }
            set
            {
                _previewInstance = value;
            }
        }

        [SerializeField]
        GameObject _previewObject;
        public GameObject PreviewObject
        {
            get { return _previewObject; }
            set
            {
                if (_previewObject != value)
                {
                    if (previewInstance != null)
                    {
                        DestroyImmediate(previewInstance.gameObject);
                    }

                    _previewObject = value;

                    if (value != null)
                    {
                        GameObject go = GameObject.Instantiate<GameObject>(value);

                        go.transform.parent = this.transform;
                        go.transform.localPosition = Vector3.zero;

                        //remove all relations to buildings.

                        BaseSocket[] sockets = go.GetComponentsInChildren<BaseSocket>(true);
                        BaseCondition[] conditions = go.GetComponentsInChildren<BaseCondition>(true);
                        BaseBuilding building = go.GetComponent<BaseBuilding>();

                        for (int socketIndex = 0; socketIndex < sockets.Length; socketIndex++)
                        {
                            DestroyImmediate(sockets[socketIndex].gameObject);
                        }

                        for (int conditionIndex = 0; conditionIndex < conditions.Length; conditionIndex++)
                        {
                            DestroyImmediate(conditions[conditionIndex].gameObject);
                        }

                        if (building != null)
                        {
                            DestroyImmediate(building);
                        }

                        previewInstance = go.AddComponent<PreviewBuilding>();
                        previewInstance.previewPrefab = PreviewObject;

                        if(OnPreviewObjectChangedEvent != null)
                            OnPreviewObjectChangedEvent(_previewObject);
                    }
                }
            }
        }

        [HideInInspector]
        public BaseBuilding building;

        bool _isOccupied = false;
        bool _isForced = false;

        public bool isActive
        {
            get { return gameObject.activeInHierarchy; }
        }
        public bool isOccupied
        {
            get { return _isOccupied; }
            set
            {
                _isOccupied = value;
                ForceEnable(!value);
            }
        }
        public bool isForced
        {
            get { return _isForced; }
        }
        public bool isSocketPlaceType
        {
            get { return FlagsHelper.IsBitSet<PlacingRestrictionType>(placingType, PlacingRestrictionType.SocketBased); }
        }
        public bool isFreePlaceType
        {
            get { return FlagsHelper.IsBitSet<PlacingRestrictionType>(placingType, PlacingRestrictionType.FreelyBased); }
        }
        #endregion

        /// <summary>
        /// Handle the building that is hovering on the socket.
        /// </summary>
        /// <param name="building">The building that is hovering on the socket now</param>
        /// <param name="buildingPlaceType">What is the building type ?</param>
        /// <returns></returns>
        public virtual bool IsFit(BaseBuilding building, PlacingRestrictionType buildingPlaceType)
        {
            return FlagsHelper.IsBitSet(receiveType, building.buildingType) && FlagsHelper.IsBitSet<PlacingRestrictionType>(placingType, buildingPlaceType) && isActive;
        }

        /// <summary>
        /// Calls on awake, sets up values and adds the socket to the global sockets collection.
        /// </summary>
        public virtual void Awake()
        {
            building = GetComponentInParent<BaseBuilding>();

            if (!LayersData.SocketLayers.Contains(LayerMask.LayerToName(gameObject.layer)))
                gameObject.layer = LayersData.DefaultSocketLayer;

            if(previewInstance != null) // get rid of preview on init
            {
                DestroyImmediate(previewInstance.gameObject);
                previewInstance = null;
                PreviewObject = null;
            }
        }

        /// <summary>
        /// Calls when building was snapped into this socket, if true then it will disable this socket. if false it will re-enable it.
        /// </summary>
        /// <param name="value">Is a building snapped to the socket ?</param>
        public virtual void BuildingSnapped(bool value)
        {
            isOccupied = value;
        }

        /// <summary>
        /// This will take 2 initial parameters and scale up the socket, and use the parameters to determine its values.
        /// </summary>
        /// <param name="previewObject">The preview object, will be used in order to scale the socket. can be left null.</param>
        /// <param name="colliderScale">The collider scale.</param>
        public virtual void InitiateComponents(GameObject previewObject, Vector3 colliderScale)
        {
            if (previewObject != null)
            {
                this.PreviewObject = previewObject;

                transform.localScale = previewObject.transform.lossyScale;
                transform.rotation = previewObject.transform.rotation;
                previewInstance.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// Occupy a socket.
        /// </summary>
        /// <param name="value">occupy or unoccupy ?</param>
        [System.Obsolete("This method is obsolete, change the isOccupied property instead.")]
        public void OccupySocket(bool value)
        {
        }
         

        /// <summary>
        /// Force enable or disable the socket, this will block the ability to normally enable sockets.
        /// </summary>
        /// <param name="enable">enable or disable?</param>
        public virtual void ForceEnable(bool enable)
        {
            _isForced = !enable;
            EnableSocket(enable);
        }

        /// <summary>
        /// Enable the socket
        /// </summary>
        /// <param name="value">disable or enable</param>
        public virtual void EnableSocket(bool value)
        {
            if (value && isForced) return; // return if we are trying to active an occoupied socket.
                
            gameObject.SetActive(value);
        }

        /// <summary>
        /// Globally enable all of the sockets in the scene
        /// </summary>
        /// <param name="value">Enable the sockets, or disable them.</param>
        public static void GloballyEnableSockets(bool value)
        {
            BaseBuildingGroup group;

            for(int i = 0; i < BaseBuildingGroup.groups.Count; i++)
            {
                group = BaseBuildingGroup.groups[i];

                if (group != null)
                {
                    group.EnableGroupSockets(value, true);
                }
            }
        }

        /// <summary>
        /// Initialize runtime socket preview.
        /// </summary>
        public virtual void Update()
        {
            if((drawSockets || drawIndividual) && isActive)
            {
                Graphics.DrawMesh(cubeMesh, transform.localToWorldMatrix, socketMat, 0);
            }
        }

    }

    /// <summary>
    /// Handles socket's template and blueprints section
    /// </summary>
    public partial class BaseSocket : UCPhysicsObject, ITemplateObject, IBlueprintItem, IPlacingModifier
    {
        /// <summary>
        /// Get our transform
        /// </summary>
        /// <returns>our transform</returns>
        public Transform GetTransform()
        {
            return transform;
        }

        /// <summary>
        /// Pack our building data
        /// </summary>
        /// <returns>our building data</returns>
        public virtual BlueprintData Pack()
        {
            return new SocketBuildingData(this);
        }

        /// <summary>
        /// Our priority.
        /// </summary>
        public virtual int priority
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Render our editor. (EDITOR ONLY).
        /// </summary>
        public void RenderEditor()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Create our instance.
        /// </summary>
        public void Create()
        {
            throw new System.NotImplementedException();
        }
    }

    [System.Serializable]
    public class SocketBuildingData : BlueprintData
    {
        public BuildingType receiveType = BuildingType.Foundation;
        public PlacingRestrictionType placingType = PlacingRestrictionType.SocketBased;

        public SerializeableVector3 center;
        public SerializeableVector3 size;

        public bool isHoverTarget;

        public SocketBuildingData(BaseSocket socket)
        {
            this.name = socket.transform.name;

            this.position = socket.transform.localPosition;
            this.rotation = socket.transform.localRotation;
            this.scale = socket.transform.localScale;

            this.receiveType = socket.receiveType;
            this.placingType = socket.placingType;

            this.center = socket.center;
            this.size = socket.size;

            this.isHoverTarget = socket.isHoverTarget;
        }

        public override void UnPack(GameObject target)
        {
            BaseBuilding building = target.GetComponentInParent<BaseBuilding>();

            if(building != null)
            {
                BaseSocket socket = building.CreateSocket(name, SocketPositionAnchor.Center, null, receiveType, placingType);
                socket.transform.localPosition = (Vector3)position;
                socket.transform.localScale = (Vector3)scale;
                socket.transform.localRotation = (Quaternion)rotation;

                socket.center = (Vector3)this.center;
                socket.size = (Vector3)this.size;

                socket.isHoverTarget = isHoverTarget;
            }
        }
    }

}