using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : SingletonManager<ResourceManager>
{
    private Dictionary<ResourceType, Dictionary<int, int>> resources;
    private Dictionary<ResourceType, int> storageLimit;
    
    // 事件
    public static event System.Action<ResourceType, int, int> OnResourceChanged;
    protected override void Awake()
    {
        base.Awake();
    }
    // 初始化
    public void Initialize() { }
    
    // 资源操作API
    // 类型安全的资源操作API - 推荐使用
    public bool AddResource<T>(ResourceType type, T subType, int amount) where T : System.Enum
    {
        int subTypeInt = ResourceSubTypeHelper.ToInt(subType);
        return AddResource(type, subTypeInt, amount);
    }

    public bool RemoveResource<T>(ResourceType type, T subType, int amount) where T : System.Enum
    {
        int subTypeInt = ResourceSubTypeHelper.ToInt(subType);
        return RemoveResource(type, subTypeInt, amount);
    }
    public int GetResourceAmount<T>(ResourceType type, T subType) where T : System.Enum
    {
        int subTypeInt = ResourceSubTypeHelper.ToInt(subType);
        return GetResourceAmount(type, subTypeInt);
    }
    public bool HasEnoughResource<T>(ResourceType type, T subType, int amount) where T : System.Enum
    {
        int subTypeInt = ResourceSubTypeHelper.ToInt(subType);
        return HasEnoughResource(type, subTypeInt, amount);
    }

    // 内部使用的int版本API
    public bool AddResource(ResourceType type, int subType, int amount) 
    { 
        ValidateSubType(type, subType);
        
        if (!resources.ContainsKey(type))
            resources[type] = new Dictionary<int, int>();
        
        if (!resources[type].ContainsKey(subType))
            resources[type][subType] = 0;
            
        int oldAmount = resources[type][subType];
        resources[type][subType] += amount;
        
        OnResourceChanged?.Invoke(type, oldAmount, resources[type][subType]);
        return true; 
    
    }
    public bool RemoveResource(ResourceType type, int subType, int amount) 
    { 
        ValidateSubType(type, subType);
        
        if (!HasEnoughResource(type, subType, amount))
            return false;
            
        int oldAmount = resources[type][subType];
        resources[type][subType] -= amount;
        
        OnResourceChanged?.Invoke(type, oldAmount, resources[type][subType]);
        return true; 
    
    }
    public int GetResourceAmount(ResourceType type, int subType) 
    { 
        ValidateSubType(type, subType);
        
        if (!resources.ContainsKey(type) || !resources[type].ContainsKey(subType))
            return 0;
            
        return resources[type][subType];
    }
    public bool HasEnoughResource(ResourceType type, int subType, int amount) 
    {
        ValidateSubType(type, subType);
        return GetResourceAmount(type, subType) >= amount;
    }
    
    // 验证子类型是否有效
    private void ValidateSubType(ResourceType type, int subType)
    {
        if (!ResourceSubTypeHelper.IsValidSubType(type, subType))
        {
            throw new System.ArgumentException(
                $"Invalid subType {subType} for ResourceType {type}. " +
                $"Valid range: {GetValidSubTypeRange(type)}");
        }
    }
    
    private string GetValidSubTypeRange(ResourceType type)
    {
        var enumType = ResourceSubTypeHelper.GetSubTypeEnum(type);
        var values = System.Enum.GetValues(enumType);
        return $"0-{values.Length - 1} ({string.Join(", ", values.Cast<object>())})";
    }
    // 存储管理
    public void SetStorageLimit(ResourceType type, int limit) { }
    public int GetStorageLimit(ResourceType type) { return 0; }
    public bool IsStorageFull(ResourceType type) { return false; }
    
    // 数据访问
    public Dictionary<ResourceType, Dictionary<int, int>> GetAllResources() { return null; }
}