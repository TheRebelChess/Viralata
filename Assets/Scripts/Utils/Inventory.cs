using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySpace
{
    public GameObject InventorySlot;
    public bool InUse;
    public Item item;

    public InventorySpace(GameObject slot, bool use)
    {
        InventorySlot = slot;
        InUse = use;
        item = null;
    }
};

public class Inventory : MonoBehaviour
{
    private List<InventorySpace> inventorySpaces = new List<InventorySpace>();

    //public Database itemsDatabase;
    public int numSlots = 16;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize()
    {
        // Cria referencias para os slots do inventário na UI
        for (int i = 0; i < numSlots; i++)
        {
            GameObject slot = transform.GetChild(i).gameObject;
            inventorySpaces.Add(new InventorySpace(slot, false));
            //Debug.Log("Registered slot  " + slot);
        }
    }

    public void AddItem(Item item)
    {
        for (int i = 0; i < inventorySpaces.Count; i++)
        {
            if (!inventorySpaces[i].InUse)
            {
                inventorySpaces[i].item = item;
                inventorySpaces[i].InUse = true;
                inventorySpaces[i].InventorySlot.GetComponent<InventorySlot>().SetItem(item);
                return;
            }
        }
    }

    public void RemoveItem(GameObject slot)
    {
        for (int i = 0; i < inventorySpaces.Count; i++)
        {
            if (inventorySpaces[i].InventorySlot == slot)
            {
                inventorySpaces[i].InUse = false;
                inventorySpaces[i].item = null;
                return;
            }
        }
    }

}
