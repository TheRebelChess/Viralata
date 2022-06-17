using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    
    private GameObject itemIcon;
    private Inventory inventoryScript;

    private PlayerMovement player;

    public void UseItem()
    {
        if (item == null)
            return;

        Debug.Log("Item " + item.name + " consumed");
        player = GetComponentInParent<Inventory>().player;
        item.UseItem(player);
        itemIcon = transform.GetChild(0).gameObject;
        inventoryScript = GetComponentInParent<Inventory>();

        this.item = null;
        itemIcon.SetActive(false);
        itemIcon.GetComponent<Image>().sprite = null;

        inventoryScript.RemoveItem(this.gameObject);
    }

    public void SetItem(Item item)
    {
        itemIcon = transform.GetChild(0).gameObject;
        inventoryScript = GetComponentInParent<Inventory>();

        this.item = item;

        itemIcon.SetActive(true);
        itemIcon.GetComponent<Image>().sprite = item.image;
    }
}
