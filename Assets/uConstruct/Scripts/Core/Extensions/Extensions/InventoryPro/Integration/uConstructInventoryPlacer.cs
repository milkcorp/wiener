#if uConstruct_InventoryPRO

using UnityEngine;
using System.Collections;

using uConstruct;
using uConstruct.Demo;

using Devdog.InventoryPro;
using Devdog.General.UI;
//using Devdog.InventorySystem;
//using Devdog.InventorySystem.UI;


public class uConstructInventoryPlacer : BuildingPlacer
{
    GameObject currentPrefab;

    [HideInInspector]
    public uConstructEquipableItem item;

    public uConstruct_FirstPersonController controller;
    public InventoryPlayer player;

    bool isOpened;

    /// <summary>
    /// Initiate awake initializations
    /// </summary>
    public override void Awake()
    {
        base.Awake();

        if (player == null)
        {
            Debug.LogError("Inventory player not assigned to building placer.");
            this.enabled = false;

            return;
        }

        ItemCollectionBase collection;
        UIWindow window;

        for (int i = 0; i < player.inventoryCollections.Length; i++)
        {
            collection = player.inventoryCollections[i];

            window = collection.GetComponent<UIWindow>();

            if (window != null)
            {
                window.OnShow += () => HandleInventory(true);
                window.OnHide += () => HandleInventory(false);
            }
        }

    }

    /// <summary>
    /// On Item Equipped.
    /// </summary>
    public virtual void onEquip(GameObject prefab, uConstructEquipableItem item)
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
    /// Disable update if inventory is opened.
    /// </summary>
    public override void Update()
    {
        if (isOpened)
            return;

        base.Update();
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

            if (item.Place())
            {
                this.item = null;
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

    /// <summary>
    /// Handle mouse look and mouse movement when inventory is opened/closed.
    /// </summary>
    /// <param name="value">is opened?</param>
    public virtual void HandleInventory(bool value)
    {
        LockCursor = !value;
        isOpened = value;

        if (controller != null)
            controller.getInputsMouse = !value;
    }

}

#endif