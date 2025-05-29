using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : SingletonManager<ResourceManager>
{
    [Header("资源配置")]
    [SerializeField] private ResourceData[] resourceDataArray;
    [SerializeField] private int initialGold = 100;
    
    [Header("当前资源")]
    [SerializeField] private Dictionary<ResourceType, int> resources;
    private Dictionary<ResourceType, ResourceData> resourceDataDict;
    
    [Header("资源转化队列")]
    private List<ResourceConversionTask> conversionTasks;

    // 资源变化事件，供报告系统监听
    // 事件
    public event System.Action<ResourceType, int, int> OnResourceChanged;
    public event System.Action<ResourceType, int> OnResourceGained;
    public event System.Action<ResourceType, int> OnResourceSpent;
    public event System.Action<ResourceType, int> OnResourceCapacityChanged;

    protected override void Awake()
    {
        base.Awake();
        InitializeResources();
    }
    
    private void Update()
    {
        UpdateConversionTasks();
    }

    public void Initialize()
    {
        LoadResourceData();
        SetInitialResources();
        Debug.Log("资源系统初始化完成");
    }

    private void InitializeResources()
    {
        resources = new Dictionary<ResourceType, int>();
        resourceDataDict = new Dictionary<ResourceType, ResourceData>();
        conversionTasks = new List<ResourceConversionTask>();
    }

    private void LoadResourceData()
    {
        resourceDataDict.Clear();
        
        foreach (var data in resourceDataArray)
        {
            if (data != null)
            {
                resourceDataDict[data.resourceType] = data;
            }
        }
        
        Debug.Log($"加载了 {resourceDataDict.Count} 种资源配置");
    }

    private void SetInitialResources()
    {
        // 初始化所有资源为0
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 0;
        }
        
        // 设置初始金币
        resources[ResourceType.Gold] = initialGold;
        
        // 给一些初始资源用于测试
        resources[ResourceType.Seed] = 10;
    }

    #region 资源操作

    public bool SpendResource(ResourceType type, int amount) 
    { 
        if (amount <= 0) return false;
        
        if (GetResourceAmount(type) >= amount)
        {
            int oldAmount = resources[type];
            resources[type] -= amount;
            
            // 触发事件
            OnResourceChanged?.Invoke(type, oldAmount, resources[type]);
            OnResourceSpent?.Invoke(type, amount);
            
            Debug.Log($"消耗资源: {type} -{amount}, 剩余: {resources[type]}");
            return true;
        }

        Debug.LogWarning($"资源不足: {type}, 需要: {amount}, 拥有: {GetResourceAmount(type)}");
        return false;
    }

    public void AddResource(ResourceType type, int amount) 
    { 
        if (amount <= 0) return;

        int oldAmount = GetResourceAmount(type);
        
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
        }
        else
        {
            resources[type] = amount;
        }
        
        // 检查堆叠限制
        int stackLimit = GetResourceData(type)?.stackLimit ?? 999;
        if (resources[type] > stackLimit)
        {
            resources[type] = stackLimit;
        }

        // 触发事件
        OnResourceChanged?.Invoke(type, oldAmount, resources[type]);
        OnResourceGained?.Invoke(type, amount);

        Debug.Log($"获得资源: {type} +{amount}, 总计: {resources[type]}");
    }
    public int GetResourceAmount(ResourceType type) 
    { 
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    public bool HasEnoughResources(ResourceCost[] costs)
    {
        foreach (var cost in costs)
        {
            if (GetResourceAmount(cost.resourceType) < cost.amount)
            {
                return false;
            }
        }
        return true;
    }

    public bool SpendResources(ResourceCost[] costs)
    {
        if (!HasEnoughResources(costs))
        {
            return false;
        }
        
        foreach (var cost in costs)
        {
            SpendResource(cost.resourceType, cost.amount);
        }
        
        return true;
    }

    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(resources);
    }
    
    public ResourceData GetResourceData(ResourceType type)
    {
        return resourceDataDict.ContainsKey(type) ? resourceDataDict[type] : null;
    }

    #endregion

    #region 资源转化系统
    
    public bool StartResourceConversion(ResourceType inputType, ResourceConversion conversion)
    {
        ResourceData inputData = GetResourceData(inputType);
        if (inputData == null || !inputData.canBeConverted)
        {
            Debug.LogWarning($"资源 {inputType} 无法进行转化");
            return false;
        }
        
        // 检查输入资源是否足够
        if (!HasEnoughResources(conversion.inputCosts))
        {
            Debug.LogWarning("转化所需资源不足");
            return false;
        }
        
        // 消耗输入资源
        if (!SpendResources(conversion.inputCosts))
        {
            return false;
        }
        
        // 创建转化任务
        ResourceConversionTask task = new ResourceConversionTask
        {
            outputType = conversion.outputType,
            outputAmount = conversion.outputAmount,
            remainingTime = conversion.conversionTime,
            totalTime = conversion.conversionTime
        };
        
        conversionTasks.Add(task);
        
        Debug.Log($"开始资源转化: {inputType} -> {conversion.outputType}, 预计 {conversion.conversionTime} 秒完成");
        return true;
    }
    
    private void UpdateConversionTasks()
    {
        for (int i = conversionTasks.Count - 1; i >= 0; i--)
        {
            ResourceConversionTask task = conversionTasks[i];
            task.remainingTime -= Time.deltaTime;
            
            if (task.remainingTime <= 0)
            {
                // 转化完成，添加产出资源
                AddResource(task.outputType, task.outputAmount);
                conversionTasks.RemoveAt(i);
                
                Debug.Log($"资源转化完成: 获得 {task.outputType} x{task.outputAmount}");
            }
        }
    }
    
    public List<ResourceConversionTask> GetActiveConversions()
    {
        return new List<ResourceConversionTask>(conversionTasks);
    }
    
    #endregion

    #region 经济系统（简化版固定价格）
    
    public int GetResourcePrice(ResourceType type, bool isBuying = true)
    {
        ResourceData data = GetResourceData(type);
        if (data == null) return 1;
        
        return data.basePrice; // 固定价格
    }
    
    public bool CanBuyResource(ResourceType type)
    {
        ResourceData data = GetResourceData(type);
        return data != null && data.canBeBought;
    }
    
    public bool CanSellResource(ResourceType type)
    {
        ResourceData data = GetResourceData(type);
        return data != null && data.canBeSold;
    }
    
    public bool BuyResource(ResourceType type, int amount)
    {
        if (!CanBuyResource(type))
        {
            Debug.LogWarning($"无法购买资源: {type}");
            return false;
        }
        
        int totalCost = GetResourcePrice(type, true) * amount;
        
        if (SpendResource(ResourceType.Gold, totalCost))
        {
            AddResource(type, amount);
            Debug.Log($"购买成功: {type} x{amount}, 花费: {totalCost} 金币");
            return true;
        }
        
        return false;
    }
    
    public bool SellResource(ResourceType type, int amount)
    {
        if (!CanSellResource(type))
        {
            Debug.LogWarning($"无法出售资源: {type}");
            return false;
        }
        
        if (SpendResource(type, amount))
        {
            int totalIncome = GetResourcePrice(type, false) * amount;
            AddResource(ResourceType.Gold, totalIncome);
            Debug.Log($"出售成功: {type} x{amount}, 获得: {totalIncome} 金币");
            return true;
        }
        
        return false;
    }
    
    #endregion
}

[System.Serializable]
public class ResourceConversionTask
{
    public ResourceType outputType;
    public int outputAmount;
    public float remainingTime;
    public float totalTime;
    
    public float Progress => 1f - (remainingTime / totalTime);
}

