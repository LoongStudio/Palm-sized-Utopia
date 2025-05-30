using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : SingletonManager<ResourceManager>
{
    [Header("资源配置")]
    [SerializeField] private ResourceData[] resourceDataArray; // 资源配置
    [SerializeField] private int initialGold = 100; // 初始金币
    
    [Header("当前资源")]
    [SerializeField] private Dictionary<ResourceType, int> resources; // 当前资源
    private Dictionary<ResourceType, ResourceData> resourceDataDict; // 资源数据字典
    
    [Header("资源转化队列")]
    private List<ResourceConversionTask> conversionTasks; // 资源转化队列   
    private Dictionary<string, ResourceConversionTask> taskDict; // 任务ID映射

    // 转化相关事件
    public event System.Action<ResourceConversionTask> OnConversionStarted;
    public event System.Action<ResourceConversionTask> OnConversionCompleted;
    public event System.Action<ResourceConversionTask> OnConversionPaused;
    public event System.Action<ResourceConversionTask> OnConversionResumed;
    public event System.Action<ResourceConversionTask> OnConversionCanceled;

    // 资源变化事件，供报告系统监听
    public event System.Action<ResourceType, int, int> OnResourceChanged;
    public event System.Action<ResourceType, int> OnResourceGained;
    public event System.Action<ResourceType, int> OnResourceSpent;
    public event System.Action<ResourceType, int> OnResourceCapacityChanged;

    protected override void Awake()
    {
        base.Awake();
        InitializeResources();
        InitializeConversionSystem();
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

    /// <summary>
    /// 初始化资源系统所需的数据结构
    /// </summary>
    private void InitializeResources()
    {
        resources = new Dictionary<ResourceType, int>(); // 当前资源
        resourceDataDict = new Dictionary<ResourceType, ResourceData>(); // 资源数据字典
        conversionTasks = new List<ResourceConversionTask>(); // 资源转化队列
    }

    /// <summary>
    /// 初始化资源转化系统所需的数据结构
    /// </summary> 
    private void InitializeConversionSystem()
    {
        if (conversionTasks == null)
            conversionTasks = new List<ResourceConversionTask>();
        
        if (taskDict == null)
            taskDict = new Dictionary<string, ResourceConversionTask>();
    }

    /// <summary>
    /// 加载资源配置数据
    /// </summary>
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

    /// <summary>
    /// 设置初始资源
    /// </summary>
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

    /// <summary>
    /// 消耗资源
    /// </summary>
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

    /// <summary>
    /// 添加资源
    /// </summary>
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

    /// <summary>
    /// 获取资源数量
    /// </summary>
    public int GetResourceAmount(ResourceType type) 
    { 
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    /// <summary>
    /// 检查是否拥有足够资源
    /// </summary>
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

    /// <summary>
    /// 消耗资源
    /// </summary>
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

    /// <summary>
    /// 获取所有资源
    /// </summary>
    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(resources);
    }

    /// <summary>
    /// 获取资源数据
    /// </summary>
    public ResourceData GetResourceData(ResourceType type)
    {
        return resourceDataDict.ContainsKey(type) ? resourceDataDict[type] : null;
    }

    #endregion

    #region 增强的资源转化系统

    /// <summary>
    /// 开始资源转化
    /// </summary>
    public ResourceConversionTask StartResourceConversion(ResourceType inputType, ResourceConversion conversion, Building sourceBuilding = null)
    {
        ResourceData inputData = GetResourceData(inputType);
        if (inputData == null || !inputData.canBeConverted)
        {
            Debug.LogWarning($"资源 {inputType} 无法进行转化");
            return null;
        }
        
        // 检查输入资源是否足够
        if (!HasEnoughResources(conversion.inputCosts))
        {
            Debug.LogWarning("转化所需资源不足");
            return null;
        }
        
        // 消耗输入资源
        if (!SpendResources(conversion.inputCosts))
        {
            return null;
        }
        
        // 创建转化任务
        ResourceConversionTask task = new ResourceConversionTask(conversion, sourceBuilding);
        
        // 注册任务
        conversionTasks.Add(task);
        taskDict[task.taskId] = task;
        
        // 触发事件
        OnConversionStarted?.Invoke(task);
        
        Debug.Log($"开始资源转化: {inputType} -> {conversion.outputType}, 预计 {conversion.conversionTime} 秒完成");
        Debug.Log($"任务ID: {task.taskId}, 来源建筑: {task.sourceBuildingName}");
        
        return task;
    }

    /// <summary>
    /// 更新资源转化任务
    /// </summary>
    private void UpdateConversionTasks()
    {
        for (int i = conversionTasks.Count - 1; i >= 0; i--)
        {
            ResourceConversionTask task = conversionTasks[i];
            
            // 只更新活跃状态的任务
            if (task.state == ConversionState.Active)
            {
                task.remainingTime -= Time.deltaTime;
                
                if (task.remainingTime <= 0)
                {
                    CompleteConversion(task);
                }
            }
            
            // 清理已完成或已取消的任务
            if (task.state == ConversionState.Completed || task.state == ConversionState.Canceled)
            {
                RemoveConversionTask(task);
                i--; // 调整索引
            }
        }
    }

    /// <summary>
    /// 完成资源转化
    /// </summary>
    private void CompleteConversion(ResourceConversionTask task)
    {
        task.Complete();
        
        // 添加产出资源
        AddResource(task.outputType, task.outputAmount);
        
        // 触发事件
        OnConversionCompleted?.Invoke(task);
        
        Debug.Log($"资源转化完成: 获得 {task.outputType} x{task.outputAmount} (任务ID: {task.taskId})");
    }

    /// <summary>
    /// 移除资源转化任务
    /// </summary>
    private void RemoveConversionTask(ResourceConversionTask task)
    {
        conversionTasks.Remove(task);
        taskDict.Remove(task.taskId);
    }
    
    #endregion
    
    #region 转化任务控制

    /// <summary>
    /// 暂停转化任务
    /// </summary>
    public bool PauseConversion(string taskId)
    {
        if (taskDict.TryGetValue(taskId, out ResourceConversionTask task))
        {
            if (task.Pause())
            {
                OnConversionPaused?.Invoke(task);
                Debug.Log($"暂停转化任务: {taskId}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 恢复转化任务
    /// </summary>
    public bool ResumeConversion(string taskId)
    {
        if (taskDict.TryGetValue(taskId, out ResourceConversionTask task))
        {
            if (task.Resume())
            {
                OnConversionResumed?.Invoke(task);
                Debug.Log($"恢复转化任务: {taskId}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 取消转化任务
    /// </summary>
    public bool CancelConversion(string taskId, bool refundResources = true)
    {
        if (taskDict.TryGetValue(taskId, out ResourceConversionTask task))
        {
            if (task.Cancel())
            {
                // 回退资源
                if (refundResources)
                {
                    RefundConversionResources(task);
                }
                
                OnConversionCanceled?.Invoke(task);
                Debug.Log($"取消转化任务: {taskId}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 回退转化资源
    /// </summary>
    private void RefundConversionResources(ResourceConversionTask task)
    {
        foreach (var cost in task.inputCosts)
        {
            int refundAmount = Mathf.RoundToInt(cost.amount * task.cancelRefundRate);
            if (refundAmount > 0)
            {
                AddResource(cost.resourceType, refundAmount);
                Debug.Log($"回退资源: {cost.resourceType} x{refundAmount}");
            }
        }
    }

    /// <summary>
    /// 暂停所有转化任务
    /// </summary>
    public void PauseAllConversions()
    {
        foreach (var task in conversionTasks)
        {
            if (task.state == ConversionState.Active)
            {
                task.Pause();
                OnConversionPaused?.Invoke(task);
            }
        }
        Debug.Log("已暂停所有转化任务");
    }

    /// <summary>
    /// 恢复所有转化任务
    /// </summary>
    public void ResumeAllConversions()
    {
        foreach (var task in conversionTasks)
        {
            if (task.state == ConversionState.Paused)
            {
                task.Resume();
                OnConversionResumed?.Invoke(task);
            }
        }
        Debug.Log("已恢复所有转化任务");
    }
    
    #endregion
    
    #region 查询方法

    /// <summary>
    /// 获取所有活跃的转化任务
    /// </summary>
    public List<ResourceConversionTask> GetActiveConversions()
    {
        return conversionTasks.FindAll(t => t.state == ConversionState.Active);
    }

    /// <summary>
    /// 获取所有暂停的转化任务
    /// </summary>
    public List<ResourceConversionTask> GetPausedConversions()
    {
        return conversionTasks.FindAll(t => t.state == ConversionState.Paused);
    }

    /// <summary>
    /// 获取所有转化任务
    /// </summary>
    public List<ResourceConversionTask> GetAllConversions()
    {
        return new List<ResourceConversionTask>(conversionTasks);
    }

    /// <summary>
    /// 获取指定建筑的转化任务
    /// </summary>
    public List<ResourceConversionTask> GetConversionsByBuilding(int buildingInstanceId)
    {
        return conversionTasks.FindAll(t => t.sourceInstanceId == buildingInstanceId);
    }

    /// <summary>
    /// 获取指定建筑类型的转化任务
    /// </summary>
    public List<ResourceConversionTask> GetConversionsByBuildingType(BuildingType buildingType)
    {
        return conversionTasks.FindAll(t => t.sourceBuildingType == buildingType);
    }

    /// <summary>
    /// 获取指定任务ID的转化任务
    /// </summary>
    public ResourceConversionTask GetConversionTask(string taskId)
    {
        return taskDict.TryGetValue(taskId, out ResourceConversionTask task) ? task : null;
    }
    
    #endregion
    
    #region 建筑相关处理

    /// <summary>
    /// 处理建筑被拆除
    /// </summary>
    public void HandleBuildingDestroyed(Building building)
    {
        var buildingTasks = GetConversionsByBuilding(building.GetInstanceID());
        
        foreach (var task in buildingTasks)
        {
            Debug.Log($"建筑 {building.data.buildingName} 被拆除，处理转化任务 {task.taskId}");
            
            // 根据策略处理：暂停任务或取消任务
            if (task.canBePaused)
            {
                PauseConversion(task.taskId);
                Debug.Log($"任务 {task.taskId} 已暂停，等待重新分配");
            }
            else
            {
                CancelConversion(task.taskId, true);
                Debug.Log($"任务 {task.taskId} 已取消并回退资源");
            }
        }
    }

    /// <summary>
    /// 处理建筑升级
    /// </summary>
    public void HandleBuildingUpgraded(Building building)
    {
        var buildingTasks = GetConversionsByBuilding(building.GetInstanceID());
        
        foreach (var task in buildingTasks)
        {
            // 升级可能会影响转化速度，这里可以添加相应逻辑
            Debug.Log($"建筑 {building.data.buildingName} 升级，转化任务 {task.taskId} 继续进行");
            
            // 可以根据升级后的建筑属性调整转化速度
            // task.remainingTime *= speedModifier; // 例如
        }
    }

    /// <summary>
    /// 重新分配转化任务到新建筑
    /// </summary>
    public bool ReassignConversionToBuilding(string taskId, Building newBuilding)
    {
        if (taskDict.TryGetValue(taskId, out ResourceConversionTask task))
        {
            if (task.state == ConversionState.Paused && newBuilding != null)
            {
                // 重新分配到新建筑
                task.sourceInstanceId = newBuilding.GetInstanceID();
                task.sourceBuildingType = newBuilding.type;
                task.sourceBuildingName = newBuilding.data.buildingName;
                
                // 恢复任务
                task.Resume();
                OnConversionResumed?.Invoke(task);
                
                Debug.Log($"转化任务 {taskId} 重新分配到 {newBuilding.data.buildingName}");
                return true;
            }
        }
        return false;
    }
    
    #endregion
    
    #region 存档系统

    /// <summary>
    /// 获取转化任务存档数据
    /// </summary>
    public ConversionSaveData GetConversionSaveData()
    {
        ConversionSaveData saveData = new ConversionSaveData();
        
        foreach (var task in conversionTasks)
        {
            ConversionTaskSaveInfo taskInfo = new ConversionTaskSaveInfo
            {
                taskId = task.taskId,
                outputType = task.outputType,
                outputAmount = task.outputAmount,
                inputCosts = task.inputCosts,
                totalTime = task.totalTime,
                remainingTime = task.remainingTime,
                startTime = task.startTime,
                pauseTime = task.pauseTime,
                state = task.state,
                sourceInstanceId = task.sourceInstanceId,
                sourceBuildingType = task.sourceBuildingType,
                sourceBuildingName = task.sourceBuildingName,
                canBePaused = task.canBePaused,
                canBeCanceled = task.canBeCanceled,
                cancelRefundRate = task.cancelRefundRate
            };
            
            saveData.conversionTasks.Add(taskInfo);
        }
        
        return saveData;
    }

    /// <summary>
    /// 加载转化任务存档数据
    /// </summary>
    public void LoadConversionSaveData(ConversionSaveData saveData)
    {
        // 清空现有任务
        conversionTasks.Clear();
        taskDict.Clear();
        
        // 恢复任务
        foreach (var taskInfo in saveData.conversionTasks)
        {
            ResourceConversionTask task = new ResourceConversionTask
            {
                taskId = taskInfo.taskId,
                outputType = taskInfo.outputType,
                outputAmount = taskInfo.outputAmount,
                inputCosts = taskInfo.inputCosts,
                totalTime = taskInfo.totalTime,
                remainingTime = taskInfo.remainingTime,
                startTime = taskInfo.startTime,
                pauseTime = taskInfo.pauseTime,
                state = taskInfo.state,
                sourceInstanceId = taskInfo.sourceInstanceId,
                sourceBuildingType = taskInfo.sourceBuildingType,
                sourceBuildingName = taskInfo.sourceBuildingName,
                canBePaused = taskInfo.canBePaused,
                canBeCanceled = taskInfo.canBeCanceled,
                cancelRefundRate = taskInfo.cancelRefundRate
            };
            
            conversionTasks.Add(task);
            taskDict[task.taskId] = task;
        }
        
        Debug.Log($"加载了 {conversionTasks.Count} 个转化任务");
    }
    
    #endregion

    #region 经济系统（简化版固定价格）

    /// <summary>
    /// 获取资源价格
    /// </summary>
    public int GetResourcePrice(ResourceType type, bool isBuying = true)
    {
        ResourceData data = GetResourceData(type);
        if (data == null) return 1;
        
        return data.basePrice; // 固定价格
    }

    /// <summary>
    /// 检查是否可以购买资源
    /// </summary>
    public bool CanBuyResource(ResourceType type)
    {
        ResourceData data = GetResourceData(type);
        return data != null && data.canBeBought;
    }

    /// <summary>
    /// 检查是否可以出售资源
    /// </summary>
    public bool CanSellResource(ResourceType type)
    {
        ResourceData data = GetResourceData(type);
        return data != null && data.canBeSold;
    }

    /// <summary>
    /// 购买资源
    /// </summary>
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

    /// <summary>
    /// 出售资源
    /// </summary>
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

#region 存档数据结构

[System.Serializable]
public class ConversionSaveData
{
    public List<ConversionTaskSaveInfo> conversionTasks = new List<ConversionTaskSaveInfo>();
}

[System.Serializable]
public class ConversionTaskSaveInfo
{
    public string taskId;
    public ResourceType outputType;
    public int outputAmount;
    public ResourceCost[] inputCosts;
    public float totalTime;
    public float remainingTime;
    public DateTime startTime;
    public DateTime? pauseTime;
    public ConversionState state;
    public int sourceInstanceId;
    public BuildingType sourceBuildingType;
    public string sourceBuildingName;
    public bool canBePaused;
    public bool canBeCanceled;
    public float cancelRefundRate;
}

#endregion

