using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    WEAPON,
    CONSUMABLE,
    KEY
}

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string Name;
    public ItemType Type;
    public Sprite image;
    public GameObject model;

    public void UseItem(PlayerMovement player)
    {
        switch (Name)
        {
            case "Apple":
                player.GetComponent<PlayerMovement>().IncreaseHealth(3);
                break;

            default:
                break;
        }
    }
}
