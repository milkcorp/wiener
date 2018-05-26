#if uConstruct_InventoryPRO

using UnityEngine;
using System.Collections;

public class uConstructInventoryInfiniteAmountPopulator : MonoBehaviour {

    /// <summary>
    /// The target assignable amount.
    /// </summary>
    public uint targetAmount = 999;

    /// <summary>
    /// Assign infinite amount to all lootableItems
    /// </summary>
    protected virtual IEnumerator Start()
    {
        yield return new WaitForSeconds(1);

        var lootObject = GetComponent<Devdog.InventoryPro.LootableObject>();

        if(lootObject != null)
        {
            for(int i = 0; i < lootObject.items.Length; i++)
            {
                lootObject.items[i].currentStackSize = targetAmount;
            }
        }
    }
}

#endif
