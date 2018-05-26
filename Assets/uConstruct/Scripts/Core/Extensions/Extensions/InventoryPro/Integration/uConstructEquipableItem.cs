#if uConstruct_InventoryPRO

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Devdog.InventoryPro;
using Devdog.General;

//using Devdog.InventorySystem;
//using Devdog.InventorySystem.Models;

public class uConstructEquipableItem : EquippableInventoryItem
{
    public GameObject building;

    public override void NotifyItemEquipped(EquippableSlot equipSlot, uint amountEquipped)
    {
        base.NotifyItemEquipped(equipSlot, amountEquipped);

        uConstructInventoryPlacer placer = PlayerManager.instance.currentPlayer.transform.GetComponentInParent<uConstructInventoryPlacer>();

        if (placer == null)
        {
            Debug.LogError("uConstruct Inventory placer cant be found on the player!");
            return;
        }

        placer.onEquip(building, this);
    }

    public override void NotifyItemUnEquipped(ICharacterCollection equipTo, uint amountUnEquipped)
    {
        if (isEquipped)
        {
            uConstructInventoryPlacer placer = PlayerManager.instance.currentPlayer.transform.GetComponentInParent<uConstructInventoryPlacer>();

            if (placer == null)
            {
                Debug.LogError("uConstruct Inventory placer cant be found on the player!");
                return;
            }

            placer.onDeEquip();
        }

        base.NotifyItemUnEquipped(equipTo, amountUnEquipped);
    }

    public virtual bool Place()
    {
        currentStackSize--;

        for (uint i = 0; i < PlayerManager.instance.currentPlayer.inventoryPlayer.skillbarCollection.collectionSize; i++)
        {
            if (PlayerManager.instance.currentPlayer.inventoryPlayer.skillbarCollection[i].item == this)
            {
                PlayerManager.instance.currentPlayer.inventoryPlayer.skillbarCollection.SetItem(i, this, true);
            }
        }

        if (currentStackSize == 0)
        {
            NotifyItemUnEquipped(PlayerManager.instance.currentPlayer.inventoryPlayer.characterCollection, 0);

            var skillbar = PlayerManager.instance.currentPlayer.inventoryPlayer.skillbarCollection;
            var inventoryCols = PlayerManager.instance.currentPlayer.inventoryPlayer.inventoryCollections;

            if (skillbar != null)
            {
                foreach (var wrapper in skillbar)
                {
                    if (wrapper.item != null && wrapper.item.currentStackSize == 0)
                    {
                        wrapper.item = null;
                        wrapper.Repaint();
                    }
                }
                foreach (var inventory in inventoryCols)
                {
                    foreach (var wrapper in inventory)
                    {
                        if (wrapper.item != null && wrapper.item.currentStackSize == 0)
                        {
                            wrapper.item = null;
                            wrapper.Repaint();
                        }
                    }
                }
            }

            return true;
        }

        return false;
    }
}

#endif
