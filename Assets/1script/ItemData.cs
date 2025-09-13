using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public Sprite icon;
    public string description;

    public ItemData(string name, Sprite iconSprite, string desc)
    {
        itemName = name;
        icon = iconSprite;
        description = desc;
    }
}
