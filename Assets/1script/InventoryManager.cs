using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private List<ItemData> items = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InventorySlot.InitRecipes();
    }


    public void AddItem(string name, Sprite icon, string desc)
    {
        ItemData newItem = new ItemData(name, icon, desc);
        items.Add(newItem);
        InventoryUI.Instance.AddSlot(newItem);
    }

    public bool HasItem(string name)
    {
        return items.Exists(i => i.itemName == name);
    }

    public ItemData GetItem(string name)
    {
        return items.Find(i => i.itemName == name);
    }
}
