using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : SingletonManager<ResourceManager>
{
    private Dictionary<ResourceType, Dictionary<int, int>> resources;
    private Dictionary<ResourceType, int> storageLimit;
    
    // 事件
    public static event System.Action<ResourceType, int, int> OnResourceChanged;
    
    // 初始化
    public void Initialize() { }
    
    // 资源操作API
    public bool AddResource(ResourceType type, int subType, int amount) { return false; }
    public bool RemoveResource(ResourceType type, int subType, int amount) { return false; }
    public int GetResourceAmount(ResourceType type, int subType) { return 0; }
    public bool HasEnoughResource(ResourceType type, int subType, int amount) { return false; }
    
    // 存储管理
    public void SetStorageLimit(ResourceType type, int limit) { }
    public int GetStorageLimit(ResourceType type) { return 0; }
    public bool IsStorageFull(ResourceType type) { return false; }
    
    // 数据访问
    public Dictionary<ResourceType, Dictionary<int, int>> GetAllResources() { return null; }
}