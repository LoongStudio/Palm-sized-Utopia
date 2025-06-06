using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCInventory
{
    [SerializeField] private Dictionary<ResourceType, int> items;
    [SerializeField] private int maxCapacity;
    [SerializeField] private int currentWeight;

    // 事件
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public event System.Action<ResourceType, int> OnItemAdded;
    // public event System.Action<ResourceType, int> OnItemRemoved;
    // public event System.Action OnInventoryFull;
    // public event System.Action OnInventoryEmpty;
    
    public bool CanAddItem(ResourceType type, int amount) { return false; }
    public bool AddItem(ResourceType type, int amount) { return false; }
    public bool RemoveItem(ResourceType type, int amount) { return false; }
    public int GetItemCount(ResourceType type) { return 0; }
    public bool IsFull() { return false; }
    public bool IsEmpty() { return false; }
    public float GetCapacityPercentage() { return 0f; }
    public Dictionary<ResourceType, int> GetAllItems() { return null; }
    public void Clear() { }
    

}