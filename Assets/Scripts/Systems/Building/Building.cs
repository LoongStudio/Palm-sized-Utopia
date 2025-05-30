// Building.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("建筑基本信息")]
    public BuildingType type;
    public int level = 1;
    public Vector2Int gridPosition;
    public BuildingData data;
    
    [Header("运行状态")]
    public bool isActive = true;
    public float efficiency = 1f;
    
    [Header("NPC管理")]
    public List<NPC> assignedNPCs;
    public int maxNPCs;
    
    [Header("生产相关")]
    public Dictionary<ResourceType, int> storedResources;
    public float productionTimer = 0f;
    public float productionInterval = 10f; // 10秒生产一次
    
    // 事件
    public event System.Action<Building> OnBuildingUpgraded;
    public event System.Action<Building, ResourceType, int> OnResourceProduced;
    
    protected virtual void Awake()
    {
        assignedNPCs = new List<NPC>();
        storedResources = new Dictionary<ResourceType, int>();
        
        if (data != null)
        {
            InitializeFromData();
        }
    }
    
    protected virtual void Start()
    {
        RegisterWithManager();
    }
    
    protected virtual void Update()
    {
        if (isActive && data.isProductionBuilding)
        {
            UpdateProduction();
        }
    }
    
    private void InitializeFromData()
    {
        maxNPCs = data.npcCapacity;
        productionInterval = 60f / data.baseProductionRate; // 每分钟生产次数转换为间隔
        
        // 初始化存储
        foreach (var resourceType in data.producedResources)
        {
            storedResources[resourceType] = 0;
        }
    }
    
    private void RegisterWithManager()
    {
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.RegisterBuilding(this);
        }
    }
    
    #region 生产系统
    
    protected virtual void UpdateProduction()
    {
        productionTimer += Time.deltaTime;
        
        if (productionTimer >= productionInterval)
        {
            ProduceResources();
            productionTimer = 0f;
        }
    }
    
    protected virtual void ProduceResources()
    {
        if (data.producedResources.Length == 0) return;
        
        // 计算生产效率
        float totalEfficiency = CalculateProductionEfficiency();
        
        foreach (var resourceType in data.producedResources)
        {
            int productionAmount = Mathf.RoundToInt(GetBaseProductionAmount(resourceType) * totalEfficiency);
            
            if (productionAmount > 0)
            {
                // 检查存储容量
                int currentStored = storedResources.ContainsKey(resourceType) ? storedResources[resourceType] : 0;
                int maxStorage = GetStorageCapacity();
                
                if (currentStored < maxStorage)
                {
                    int actualProduced = Mathf.Min(productionAmount, maxStorage - currentStored);
                    storedResources[resourceType] = currentStored + actualProduced;
                    
                    OnResourceProduced?.Invoke(this, resourceType, actualProduced);
                    Debug.Log($"{data.buildingName} 生产了 {resourceType} x{actualProduced}");
                }
            }
        }
    }
    
    protected virtual float CalculateProductionEfficiency()
    {
        float efficiency = 1f;
        
        // NPC效率加成
        float npcEfficiency = 0f;
        foreach (var npc in assignedNPCs)
        {
            if (npc != null)
            {
                npcEfficiency += npc.GetWorkEfficiency();
            }
        }
        
        if (assignedNPCs.Count > 0)
        {
            efficiency *= (1f + npcEfficiency / assignedNPCs.Count);
        }
        
        // 建筑等级加成
        BuildingUpgradeData upgradeData = data.GetUpgradeData(level);
        if (upgradeData != null)
        {
            efficiency *= upgradeData.productionRateMultiplier;
        }
        
        return efficiency;
    }
    
    protected virtual int GetBaseProductionAmount(ResourceType type)
    {
        // 基础产量，可以在子类中重写
        return 1;
    }
    
    public virtual int GetStorageCapacity()
    {
        int capacity = data.storageCapacity;
        
        BuildingUpgradeData upgradeData = data.GetUpgradeData(level);
        if (upgradeData != null)
        {
            capacity += upgradeData.storageCapacityBonus;
        }
        
        return capacity;
    }
    
    #endregion
    
    #region NPC管理
    
    public virtual bool CanAssignNPC(NPC npc)
    {
        return assignedNPCs.Count < GetMaxNPCCapacity() && !assignedNPCs.Contains(npc);
    }
    
    public virtual bool AssignNPC(NPC npc)
    {
        if (CanAssignNPC(npc))
        {
            assignedNPCs.Add(npc);
            npc.AssignToBuilding(this);
            Debug.Log($"NPC {npc.npcName} 被分配到 {data.buildingName}");
            return true;
        }
        return false;
    }
    
    public virtual bool RemoveNPC(NPC npc)
    {
        if (assignedNPCs.Remove(npc))
        {
            npc.UnassignFromBuilding();
            Debug.Log($"NPC {npc.npcName} 离开了 {data.buildingName}");
            return true;
        }
        return false;
    }
    
    public virtual int GetMaxNPCCapacity()
    {
        int capacity = data.npcCapacity;
        
        BuildingUpgradeData upgradeData = data.GetUpgradeData(level);
        if (upgradeData != null)
        {
            capacity += upgradeData.npcCapacityBonus;
        }
        
        return capacity;
    }
    
    #endregion
    
    #region 升级系统
    
    public virtual bool CanUpgrade()
    {
        if (!data.canBeUpgraded || level >= data.maxLevel)
            return false;
        
        ResourceCost[] upgradeCosts = data.GetUpgradeCost(level);
        return ResourceManager.Instance.HasEnoughResources(upgradeCosts);
    }
    
    public virtual bool Upgrade()
    {
        if (!CanUpgrade()) return false;
        
        ResourceCost[] upgradeCosts = data.GetUpgradeCost(level);
        
        if (ResourceManager.Instance.SpendResources(upgradeCosts))
        {
            level++;
            OnUpgradeCompleted();
            OnBuildingUpgraded?.Invoke(this);
            
            Debug.Log($"{data.buildingName} 升级到 {level} 级");
            return true;
        }
        
        return false;
    }
    
    protected virtual void OnUpgradeCompleted()
    {
        // 升级后的处理，子类可以重写
        UpdateMaxNPCCapacity();
    }
    
    private void UpdateMaxNPCCapacity()
    {
        maxNPCs = GetMaxNPCCapacity();
    }
    
    #endregion
    
    #region 资源收集
    
    public virtual Dictionary<ResourceType, int> CollectResources()
    {
        Dictionary<ResourceType, int> collected = new Dictionary<ResourceType, int>(storedResources);
        
        // 将收集的资源转移到资源管理器
        foreach (var kvp in collected)
        {
            if (kvp.Value > 0)
            {
                ResourceManager.Instance.AddResource(kvp.Key, kvp.Value);
            }
        }
        
        // 清空建筑存储
        foreach (var key in storedResources.Keys.ToArray())
        {
            storedResources[key] = 0;
        }
        
        return collected;
    }
    
    public virtual int GetStoredAmount(ResourceType type)
    {
        return storedResources.ContainsKey(type) ? storedResources[type] : 0;
    }
    
    #endregion
    
    #region 建筑信息
    
    public virtual BuildingInfo GetBuildingInfo()
    {
        return new BuildingInfo
        {
            buildingType = type,
            buildingName = data.buildingName,
            level = level,
            assignedNPCCount = assignedNPCs.Count,
            maxNPCCapacity = GetMaxNPCCapacity(),
            efficiency = CalculateProductionEfficiency(),
            storedResources = new Dictionary<ResourceType, int>(storedResources),
            storageCapacity = GetStorageCapacity()
        };
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 清理分配的NPC
        foreach (var npc in assignedNPCs)
        {
            if (npc != null)
            {
                npc.UnassignFromBuilding();
            }
        }
        
        // 从管理器中注销
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterBuilding(this);
        }
    }
}

[System.Serializable]
public class BuildingInfo
{
    public BuildingType buildingType;
    public string buildingName;
    public int level;
    public int assignedNPCCount;
    public int maxNPCCapacity;
    public float efficiency;
    public Dictionary<ResourceType, int> storedResources;
    public int storageCapacity;
}