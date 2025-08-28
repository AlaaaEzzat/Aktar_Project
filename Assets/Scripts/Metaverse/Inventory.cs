using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject MagnitEffect;
    [SerializeField] private GameObject ClothItem;
    private readonly HashSet<string> _items = new();

    // Event when an item is collected
    public event Action<ItemSO> OnItemCollected;

    public bool HasItem(string itemId) => _items.Contains(itemId);

    private void Start()
    {
        OnItemCollected += OnItemColletecdEffect;
    }

    public void AddItem(ItemSO item)
    {
        if (item == null) return;

        // If already collected, do nothing
        if (_items.Contains(item.itemId)) return;

        _items.Add(item.itemId);

        // Fire event to notify other systems
        OnItemCollected?.Invoke(item);
    }

    public void OnItemColletecdEffect(ItemSO item)
    {
        if(item.itemId == "Magnat")
        {
            MagnitEffect.gameObject.SetActive(true);
        }
        else if (item.itemId == "Cloth")
        {
            ClothItem.GetComponent<SpriteRenderer>().sprite = item.icon;
            ClothItem.gameObject.SetActive(true);
        }
    }
}
