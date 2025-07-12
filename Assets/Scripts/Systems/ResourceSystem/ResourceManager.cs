using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Utopia.Systems.ResourceSystem_Legacy{
public class ResourceManager : SingletonManager<ResourceManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [SerializeField] public List<ResourceStack> resourcesData;      // 当前资源信息, 以类的形式存储
    [SerializeField] private List<ResourceConfig> resourceConfigs;  // 资源配置列表
    private Dictionary<(ResourceType, int), ResourceConfig> configCache; // 配置缓存
    [ShowInInspector]
    [DictionaryDrawerSettings(KeyLabel = "键", ValueLabel = "值")]
    private Dictionary<ResourceType, Dictionary<int, int>> resources;      // 当前资源数量
    private Dictionary<ResourceType, int> storageLimit;                   // 存储上限, 同类资源共用一个存储上限

    // 事件
    public static event System.Action<ResourceType, int, int> OnResourceChanged;
    protected override void Awake()
    {
        base.Awake();
        configCache = new Dictionary<(ResourceType, int), ResourceConfig>();
        foreach (var config in resourceConfigs)
        {
            configCache[(config.type, config.subType)] = config;
        }
    }
    // 初始化
    public void Initialize()
    {

        resources = new Dictionary<ResourceType, Dictionary<int, int>>();
        storageLimit = new Dictionary<ResourceType, int>();
        // 设置初始资源量和存储上限
        InitializeResources();
    }

    private void InitializeResources()
    {
        // 设置初始资源量和存储上限
        foreach (var resource in resourcesData)
        {
            resources[resource.type] = new Dictionary<int, int>();
            resources[resource.type][resource.subType] = resource.amount;
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 名为 {resource.type} 的资源被设置为 {resource.amount}");
        }

        foreach (var resource in resourcesData)
        {
            storageLimit[resource.type] = resource.storageLimit;
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 名为 {resource.type} 的存储上限被设置为 {resource.storageLimit}");
        }
    }

    #region 资源操作API
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
    // TODO: 处理这个问题
    // [Obsolete("使用int为subType的AddResource 已过时，请使用泛型版本替代。")]
    public bool AddResource(ResourceType type, int subType, int amount)
    {
        ValidateSubType(type, subType);
        EnsureResourceEntry(type, subType);
        // 边缘条件判断
        if (amount <= 0)
        {
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 添加的资源数量不能小于等于0");
            return false;
        }
        // 存储空间判断
        if (IsStorageFull(type, subType))
        {
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 名为 {type} 的资源存储已满, 无法添加 {amount} 个");
            return false;
        }

        // 存储空间不足判断
        if (resources[type][subType] + amount > storageLimit[type])
        {
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 名为 {type} 的资源存储空间不足, 原本数量为 {resources[type][subType]}, 无法添加 {amount} 个, 已将资源数量设置为存储上限 {storageLimit[type]}");
            resources[type][subType] = storageLimit[type];
            return true;
        }

        // 更新资源数量
        int oldAmount = resources[type][subType];
        resources[type][subType] += amount;

        if(showDebugInfo)
            Debug.Log($"[ResourceManager] 名为 {type} 的资源被添加了 {amount} 个, 原本数量为 {oldAmount}, 当前数量为 {resources[type][subType]}");

        OnResourceChanged?.Invoke(type, oldAmount, resources[type][subType]);
        GameEvents.TriggerResourceChanged(new ResourceEventArgs(){
            resourceType = type,
            subType = subType,
            oldAmount = oldAmount,
            newAmount = resources[type][subType],
            changeAmount = amount,
        });
        return true;

    }
    // TODO: 处理这个问题
    // [Obsolete("使用int为subType的RemoveResource 已过时，请使用泛型版本替代。")]
    public bool RemoveResource(ResourceType type, int subType, int amount)
    {
        ValidateSubType(type, subType);
        EnsureResourceEntry(type, subType);

        if (amount <= 0)
        {
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 移除的资源数量不能小于等于0");
            return false;
        }

        if (!HasEnoughResource(type, subType, amount))
        {
            if(showDebugInfo)
                Debug.Log($"[ResourceManager] 名为 {type} 的资源数量不足, 原本为 {resources[type][subType]}, 无法移除 {amount} 个, 已将资源数量设置为0");
            resources[type][subType] = 0;
            return true;
        }
            
        int oldAmount = resources[type][subType];
        resources[type][subType] -= amount;

        if(showDebugInfo)
            Debug.Log($"[ResourceManager] 名为 {type} 的资源被移除了 {amount} 个, 原本数量为 {oldAmount}, 当前数量为 {resources[type][subType]}");

        OnResourceChanged?.Invoke(type, oldAmount, resources[type][subType]);
        GameEvents.TriggerResourceChanged(new ResourceEventArgs(){
            resourceType = type,
            subType = subType,
            oldAmount = oldAmount,
            newAmount = resources[type][subType],
            changeAmount = -amount,
        });
        return true;

    }
    
    // TODO: 处理这个问题
    // [Obsolete("使用int为subType的GetResourceAmount 已过时，请使用泛型版本替代。")]
    public int GetResourceAmount(ResourceType type, int subType)
    {
        ValidateSubType(type, subType);

        if (!resources.ContainsKey(type) || !resources[type].ContainsKey(subType))
            return 0;

        return resources[type][subType];
    }
    // TODO: 处理这个问题
    // [Obsolete("使用int为subType的HasEnoughResource 已过时，请使用泛型版本替代。")]
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
                $"[ResourceManager]Invalid subType {subType} for ResourceType {type}. " +
                $"Valid range: {GetValidSubTypeRange(type)}");
        }
    }
    
    private string GetValidSubTypeRange(ResourceType type)
    {
        var enumType = ResourceSubTypeHelper.GetSubTypeEnum(type);
        var values = System.Enum.GetValues(enumType);
        return $"0-{values.Length - 1} ({string.Join(", ", values.Cast<object>())})";
    }

    private void EnsureResourceEntry(ResourceType type, int subType)
    {
        if (!resources.ContainsKey(type))
            resources[type] = new Dictionary<int, int>();

        if (!resources[type].ContainsKey(subType))
            resources[type][subType] = 0;
    }

    #endregion

    #region 存储管理
    public void SetStorageLimit(ResourceType type, int limit)
    {
        storageLimit[type] = limit;
        if(showDebugInfo)
            Debug.Log($"[ResourceManager]名为 {type} 的存储上限被设置为{limit}");
    }
    public int GetStorageLimit(ResourceType type)
    {
        return storageLimit[type];
    }
    public bool IsStorageFull<T>(ResourceType type, T subType) where T : System.Enum
    {
        return GetResourceAmount(type, subType) >= GetStorageLimit(type);
    }
    // TODO: 处理这个问题
    // [Obsolete("使用int为subType的IsStorageFull 已过时，请使用泛型版本替代。")]
    public bool IsStorageFull(ResourceType type, int subType)
    {
        return GetResourceAmount(type, subType) >= GetStorageLimit(type);
    }
    #endregion

    #region 数据访问
    public Dictionary<ResourceType, Dictionary<int, int>> GetAllResources()
    {
        return resources;
    }
    public Dictionary<ResourceType, int> GetAllStorageLimits()
    {
        return storageLimit;
    }
    #endregion

    /// <summary>
    /// 获取资源配置
    /// </summary>
    public ResourceConfig GetConfig(ResourceType type, int subType)
    {
        if (configCache.TryGetValue((type, subType), out var config))
        {
            return config;
        }
        Debug.LogError($"[ResourceManager] 找不到资源配置：type={type}, subType={subType}");
        return null;
    }

    /// <summary>
    /// 通过ResourceType和Enum子类型查找ResourceConfig
    /// </summary>
    public ResourceConfig GetConfigByEnum<T>(ResourceType type, T subTypeEnum) where T : System.Enum
    {
        int subTypeInt = ResourceSubTypeHelper.ToInt(subTypeEnum);
        return GetConfig(type, subTypeInt);
    }
}

}