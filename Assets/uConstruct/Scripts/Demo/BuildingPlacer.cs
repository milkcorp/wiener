using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uConstruct.Core.Physics;
using uConstruct.Core.Manager;
using uConstruct.Core.Saving;
using uConstruct.Core.AOI;
using uConstruct.Sockets;

namespace uConstruct.Demo
{
    /// <summary>
    /// A demo script that comes with the asset to place buildings.
    /// </summary>
    public class BuildingPlacer : MonoBehaviour
    {
        #region Variables
        [HideInInspector]
        public BaseBuilding currentBuilding;

        BaseBuilding _currentlyInspectedBuilding;
        public BaseBuilding currentlyInspectedBuilding
        {
            get { return _currentlyInspectedBuilding; }
            set
            {
                if (value == null && DemoUI.instance != null)
                    DemoUI.instance.Inspect("");

                if (_currentlyInspectedBuilding != value && DemoUI.instance != null && playerCamera != null)
                {
                    if (value != null)
                    {
                        DemoUI.instance.Inspect("Click Right Mouse Btn In Order To Destroy : " + value.name);
                    }
                }

                _currentlyInspectedBuilding = value;
            }
        }

        int _currentSlot = -1;
        public int currentSlot
        {
            get { return _currentSlot; }
            set
            {
                bool changed = _currentSlot != value;

                _currentSlot = value;

                currentID = changed ? 0 : currentID; // if we didnt switch to a new building so just reset the instance with the same type.
            }
        }

        public BuildingSlot[] buildings = null;
        public int _currentID = 0;
        public int currentID
        {
            get
            {
                return _currentID;
            }
            set
            {
                if (currentSlot == -1) return;

                _currentID = value;

                if (_currentID >= buildings[currentSlot].buildings.Length)
                {
                    currentSlot = -1;
                    _currentID = 0;
                }

                if (currentSlot != -1)
                {
                    if (currentBuilding != null && currentBuilding.isBeingPlaced)
                    {
                        DestroyCurrentBuilding();
                    }

                    CreateBuildingInstance(buildings[currentSlot].buildings[currentID].gameObject);
                }
                else
                {
                    DestroyCurrentBuilding();
                }
            }
        }

        [SerializeField]
        Camera _playerCamera;
        public Camera playerCamera
        {
            get { return _playerCamera; }
            set { _playerCamera = value; }
        }

        [SerializeField]
        float _placingDistance = 20;
        public float placingDistance
        {
            get { return _placingDistance; }
            set { _placingDistance = value; }
        }

        [SerializeField]
        float _destroyDistance = 50;
        public float destroyDistance
        {
            get { return _destroyDistance; }
            set { _destroyDistance = value; }
        }

        [SerializeField]
        BuildingMaterialData _canBePlacedMat = new BuildingMaterialData(null, Color.green);
        public BuildingMaterialData canBePlacedMat
        {
            get { return _canBePlacedMat; }
            set { _canBePlacedMat = value; }
        }

        [SerializeField]
        BuildingMaterialData _cantBePlacedMat = new BuildingMaterialData(null, Color.red);
        public BuildingMaterialData cantBePlacedMat
        {
            get { return _cantBePlacedMat; }
            set { _cantBePlacedMat = value; }
        }

        /// <summary>
        /// Whether the cursor will be locked on default.
        /// </summary>
        [SerializeField]
        private bool _defaultLockCursor = true;
        public bool defaultLockCursor
        {
            get { return _defaultLockCursor; }
            set
            {
                _defaultLockCursor = value;
            }
        }

        [SerializeField]
        private bool _LockCursor;
        public bool LockCursor
        {
            get { return _LockCursor; }
            set
            {
                if(defaultLockCursor && value != LockCursor)
                {
                    _LockCursor = value;

                    Cursor.visible = !value;
                    Cursor.lockState = value ? CursorLockMode.Locked:CursorLockMode.None;
                }
            }
        }

        /// <summary>
        /// Rotate the placed buildings according to the player rotation
        /// Kind of like fallout 4 building style.
        /// </summary>
        [SerializeField]
        private bool _rotatedWithPlayer = false;
        public bool rotateWithPlayer
        {
            get
            {
                return _rotatedWithPlayer;
            }
            set
            {
                _rotatedWithPlayer = value;
            }
        }

        /// <summary>
        /// Destroy buildings with right mouse click.
        /// </summary>
        [SerializeField]
        private bool _destroyBuildings = true;
        public bool destroyBuildings
        {
            get { return _destroyBuildings; }
            set { _destroyBuildings = value; }
        }

        [SerializeField]
        float _rayOffset = 0f;
        public float rayOffset
        {
            get { return _rayOffset; }
            set { _rayOffset = value; }
        }

        [SerializeField]
        RayOrigin _rayOrigin = RayOrigin.MidScreen;
        public RayOrigin rayOrigin
        {
            get { return _rayOrigin; }
            set
            {
                _rayOrigin = value;
            }
        }

        /// <summary>
        /// Get the ray which will be used for the raycast checks.
        /// </summary>
        public virtual Ray ray
        {
            get
            {
                return playerCamera.ScreenPointToRay(rayOrigin == RayOrigin.MidScreen ? new Vector3(Screen.width / 2, Screen.height / 2) : Input.mousePosition);
            }
        }

        protected virtual bool isPlaceButtonPressed
        {
            get { return Input.GetMouseButtonDown(0); }
        }

        protected virtual bool isDestroyButtonPressed
        {
            get { return Input.GetMouseButtonDown(1); }
        }

        protected virtual bool isRotatingRight
        {
            get { return Input.GetAxis("Mouse ScrollWheel") > 0; }
        }

        protected virtual bool isRotatingLeft
        {
            get { return Input.GetAxis("Mouse ScrollWheel") < 0; }
        }

        public AudioClip placeBuildingSound;
        public AudioClip destroyBuildingSound;

        public AudioSource audioSource;

        Vector3 currentRotation;
        #endregion

        /// <summary>
        /// Create and initialize the callbacks manager.
        /// </summary>
        public virtual void Awake()
        {
            UCCallbacksManager.CreateAndInitialize();
            LockCursor = defaultLockCursor;
        }

        /// <summary>
        /// Initiaite the demo ui
        /// </summary>
        public virtual void Start()
        {
            ApplyControlsToDemoUI();
        }

        /// <summary>
        /// This method will apply our controls to the demo ui, if available.
        /// </summary>
        public virtual void ApplyControlsToDemoUI()
        {
            //Update demo controls
            if (DemoUI.instance != null)
            {
                for (int i = 0; i < buildings.Length; i++)
                {
                    for (int b = 0; b < buildings[i].buildings.Length; b++)
                    {
                        DemoUI.instance.AddControl(buildings[i].buildings[b].name);
                    }
                }
            }
        }

        /// <summary>
        /// Handle the rotation of the current building.
        /// </summary>
        public virtual void HandleRotation()
        {
            if (!currentBuilding.canBeRotated) return;

            float scrollValue = isRotatingRight ? 1 : isRotatingLeft ? -1 : 0;

            if (scrollValue != 0)
            {
                currentRotation.y += (currentBuilding.rotationAmount * scrollValue);

                currentBuilding.transform.rotation = Quaternion.Euler(currentBuilding.transform.eulerAngles.x, currentRotation.y, currentBuilding.transform.eulerAngles.z);
            }
        }

        /// <summary>
        /// Update the building cycle
        /// </summary>
        public virtual void Update()
        {
            GetInputs();

            if (currentBuilding != null)
            {
                HandleRotation();

                UCPhysicsHitsArray hits = UCPhysics.RaycastAll(ray, placingDistance, LayersData.SocketMask, rayOffset);

                if (hits.Count > 0)
                {
                    hits.Sort();

                    bool isFit = currentBuilding.HandlePlacing(hits);

                    if (currentBuilding.SnappedTo == null && rotateWithPlayer)
                    {
                        currentBuilding.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + currentRotation.y, 0);
                    }

                    HandlePlacingResults(currentBuilding, isFit);

                    if (isPlaceButtonPressed && isFit)
                    {
                        PlaceBuilding();
                    }
                }
            }

            if (currentBuilding == null)
            {
                var hits = Physics.RaycastAll(ray, _destroyDistance).OrderBy(x => x.distance).ToArray();
                RaycastHit hit;
                BaseBuilding building;

                if (hits.Length > 0)
                {
                    hit = hits[0];
                    building = hit.transform.GetComponentInParent<BaseBuilding>();

                    if (building != null)
                    {
                        currentlyInspectedBuilding = building;

                        if (isDestroyButtonPressed)
                        {
                            DestroyBuilding(building, hit);
                        }
                    }
                    else
                    {
                        currentlyInspectedBuilding = null;
                    }
                }
                else
                {
                    currentlyInspectedBuilding = null;
                }
            }
            else
            {
                currentlyInspectedBuilding = null;
            }

            if (LockCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        /// <summary>
        /// Get switch slot inputs, in order to use 3d parties like inventories etc you will want to inherite this method and make it empty.
        /// </summary>
        public virtual void GetInputs()
        {
            if (buildings == null) return;

            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(ReturnAlphaKey(i + 1)))
                {
                    if (i <= buildings.Length - 1) // if included in the buildings list
                    {
                        if (currentSlot != i)
                        {
                            if (currentBuilding)
                            {
                                DestroyCurrentBuilding();
                            }

                            currentSlot = i;
                        }
                        else
                        {
                            if (currentBuilding)
                            {
                                currentID++;
                            }
                            else
                            {
                                ResetBuildingInstance();
                            }
                        }
                    }
                    else
                    {
                        currentSlot = -1;
                    }
                }
            }
        }

        /// <summary>
        /// Create a new building instance.
        /// </summary>
        /// <param name="building"></param>
        public virtual void CreateBuildingInstance(GameObject building)
        {
            var go = GameObject.Instantiate<GameObject>(building);
            go.transform.position = new Vector3(-999, -999, -999);
            go.name = building.gameObject.name;
            currentBuilding = go.transform.GetComponent<BaseBuilding>();
            currentBuilding.isBeingPlaced = true;
        }

        /// <summary>
        /// Reset the current building instance = recreate.
        /// </summary>
        public virtual void ResetBuildingInstance()
        {
            if (!this.enabled || currentSlot == -1 || buildings[currentSlot] == null) return;

            currentSlot = currentSlot;
        }

        /// <summary>
        /// Destroy the currently created building instance.
        /// </summary>
        public virtual void DestroyCurrentBuilding()
        {
            if (currentBuilding != null)
            {
                Destroy(currentBuilding.gameObject);

                currentBuilding = null;
            }
        }

        /// <summary>
        /// Place the building
        /// </summary>
        public virtual void PlaceBuilding()
        {
            if (currentBuilding != null)
            {
                currentBuilding.PlaceBuilding();
                ResetBuildingInstance();

                if (audioSource != null && placeBuildingSound != null)
                    audioSource.PlayOneShot(placeBuildingSound);
            }
        }

        /// <summary>
        /// Destroy the current building
        /// </summary>
        /// <param name="building">building instance</param>
        /// <param name="hit">our hit information</param>
        public virtual void DestroyBuilding(BaseBuilding building, RaycastHit hit)
        {
            building.health = 0;

            if (audioSource != null && destroyBuildingSound != null)
                audioSource.PlayOneShot(destroyBuildingSound);
        }

        /// <summary>
        /// Handle the placing results, so for example switch the building material color to Red/Green.
        /// </summary>
        /// <param name="building">our building</param>
        /// <param name="results">the results</param>
        public virtual void HandlePlacingResults(BaseBuilding building, bool results)
        {
            building.HandleMaterial(results ? canBePlacedMat : cantBePlacedMat);
        }

        /// <summary>
        /// Return an KeyCode between 1-9.
        /// </summary>
        /// <param name="key">our targeted key index</param>
        /// <returns></returns>
        public virtual KeyCode ReturnAlphaKey(int key)
        {
            key = Mathf.Clamp(key, 1, 9);

            return (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + key);
        }
    }

    /// <summary>
    /// A building slot that will be used in the object selection.
    /// </summary>
    [System.Serializable]
    public class BuildingSlot
    {
        public GameObject[] buildings = null;
    }
}

public enum RayOrigin
{
    MidScreen,
    MousePosition
}