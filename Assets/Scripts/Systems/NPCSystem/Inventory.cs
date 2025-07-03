using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Inventory : ISaveable
{
    public enum InventoryOwnerType
    {
        Building,
        NPC,
        None
    }

    public InventoryOwnerType ownerType = InventoryOwnerType.NPC;
    public event Action<ResourceConfig, int> OnResourceChanged; // type, subType, value变化量
    // 新版：资源堆栈结构
    public List<ResourceStack> currentStacks;
    // 只保留ResourceStack相关字段

    // // 旧版：已过期，待移除
    // [System.Obsolete("请使用currentStacks替代")] public List<SubResourceValue<int>> currentSubResource;
    // [System.Obsolete("已废弃，limit由ResourceStack维护")] public List<SubResourceValue<int>> maximumSubResource;
    // [System.Obsolete("请使用ResourceStack相关逻辑替代")] public List<SubResource> whiteList;
    // [System.Obsolete("请使用ResourceStack相关逻辑替代")] public List<SubResource> blackList;
    // 资源过滤列表
    public HashSet<ResourceConfig> acceptList = new();
    public HashSet<ResourceConfig> rejectList = new();
    // 策略选项
    public InventoryAcceptMode acceptMode = InventoryAcceptMode.OnlyDefined;
    public InventoryListFilterMode filterMode = InventoryListFilterMode.None;
    public int defaultMaxValue = 100; // 新增默认最大容量
    /// <summary>
    /// 背包资源主接收模式
    /// </summary>
    public enum InventoryAcceptMode
    {
        AllowAll,      // 允许所有物品（自动补全）
        OnlyDefined    // 只允许current/max已定义的物品
    }

    /// <summary>
    /// 白名单/黑名单过滤模式
    /// </summary>
    public enum InventoryListFilterMode
    {
        None,           // 不启用白名单/黑名单
        AcceptList,     // 只允许白名单
        RejectList,     // 只拒绝黑名单
        Both            // 白名单和黑名单都启用（先黑名单后白名单）
    }
    /// <summary>
    /// 推荐唯一构造函数，必须传入所有关键参数。
    /// </summary>
    public Inventory(
        List<ResourceStack> currentStacks,
        InventoryAcceptMode acceptMode = InventoryAcceptMode.OnlyDefined,
        InventoryListFilterMode filterMode = InventoryListFilterMode.None,
        HashSet<ResourceConfig> acceptList = null,
        HashSet<ResourceConfig> rejectList = null,
        InventoryOwnerType ownerType = InventoryOwnerType.None,
        int defaultMaxValue = 100)
    {
        this.currentStacks = currentStacks ?? new List<ResourceStack>();
        this.acceptMode = acceptMode;
        this.filterMode = filterMode;
        this.acceptList = acceptList ?? new HashSet<ResourceConfig>();
        this.rejectList = rejectList ?? new HashSet<ResourceConfig>();
        this.ownerType = ownerType;
        this.defaultMaxValue = defaultMaxValue;
    }

    // 通过ResourceStack查找
    private ResourceStack GetCurrent(ResourceConfig config)
    {
        foreach (var resourceStack in currentStacks)
            if (resourceStack.resourceConfig.Equals(config))
                return resourceStack;
        return null;
    }

    /// <summary>
    /// 检查资源是否通过过滤规则
    /// </summary>
    private bool PassesFilter(ResourceConfig config)
    {
        if (filterMode == InventoryListFilterMode.None)
            return true;

        // 先检查黑名单（如果启用了黑名单）
        if ((filterMode == InventoryListFilterMode.RejectList || filterMode == InventoryListFilterMode.Both)
            && rejectList != null)
        {
            foreach (var reject in rejectList)
            {
                if (reject.Equals(config))
                    return false;
            }
        }

        // 再检查白名单（如果启用了白名单）
        if ((filterMode == InventoryListFilterMode.AcceptList || filterMode == InventoryListFilterMode.Both)
            && acceptList != null)
        {
            bool found = false;
            foreach (var accept in acceptList)
            {
                if (accept.Equals(config))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        return true;
    }

    /// <summary>
    /// 判断能否添加物品，直接委托给ResourceStack的CanAdd
    /// </summary>
    private bool CanAddItem(ResourceConfig config, int amount)
    {
        // 先检查过滤规则
        if (!PassesFilter(config))
            return false;

        var cur = GetCurrent(config);
        if (cur == null)
        {
            // AllowAll模式下可自动补全
            if (acceptMode == InventoryAcceptMode.AllowAll)
            {
                currentStacks.Add(new ResourceStack(config, 0, defaultMaxValue));
                return true;
            }
            return false;
        }
        return cur.CanAdd(amount);
    }

    /// <summary>
    /// 添加物品，按输入策略处理
    /// </summary>
    public bool AddItem(ResourceConfig config, int amount)
    {
        // 先检查过滤规则
        if (!CanAddItem(config, amount))
            return false;

        var cur = GetCurrent(config);
        cur.AddAmount(amount);

        return true;
    }

    public bool RemoveItem(ResourceConfig config, int amount)
    {
        var cur = GetCurrent(config);
        if (cur == null || cur.amount < amount) return false;
        cur.amount -= amount;
        if (ownerType == InventoryOwnerType.Building)
            OnResourceChanged?.Invoke(config, -amount);
        return true;
    }

    public bool IsFull()
    {
        foreach (var cur in currentStacks)
        {
            if (!cur.IsFull())
                return false;
        }
        return true;
    }

    public bool IsEmpty()
    {
        return currentStacks.All(r => r.amount == 0);
    }

    public float GetCapacityPercentage()
    {
        int totalCurrent = 0;
        int totalMax = 0;
        foreach (var cur in currentStacks)
        {
            totalCurrent += cur.amount;
            totalMax += cur.storageLimit;
        }
        return totalMax == 0 ? 0f : (float)totalCurrent / totalMax;
    }

    public Dictionary<ResourceType, int> GetAllItems()
    {
        var dict = new Dictionary<ResourceType, int>();
        foreach (var cur in currentStacks)
        {
            if (dict.ContainsKey(cur.type))
                dict[cur.type] += cur.amount;
            else
                dict[cur.type] = cur.amount;
        }
        return dict;
    }

    public void Clear()
    {
        foreach (var cur in currentStacks)
            cur.amount = 0;
    }

    public bool TransferTo(Inventory target, int maxTransferAmount)
    {
        int transferredTotal = 0;
        foreach (var item in currentStacks)
        {
            if (item.amount <= 0)
                continue;
            var targetCurrent = target.GetCurrent(item.resourceConfig);
            if (targetCurrent == null)
            {
                if (target.acceptMode == InventoryAcceptMode.AllowAll)
                {
                    targetCurrent = new ResourceStack(item.resourceConfig, 0, defaultMaxValue);
                    targetCurrent.amount = 0;
                    target.currentStacks.Add(targetCurrent);
                }
                else
                {
                    continue;
                }
            }
            int spaceLeft = targetCurrent.storageLimit - targetCurrent.amount;
            if (spaceLeft <= 0)
                continue;
            int transferable = Math.Min(item.amount, Math.Min(spaceLeft, maxTransferAmount - transferredTotal));
            if (transferable <= 0)
                break;
            item.amount -= transferable;
            targetCurrent.amount += transferable;
            transferredTotal += transferable;
            if (transferredTotal >= maxTransferAmount)
                break;
        }
        return transferredTotal > 0;
    }

    public bool TransferToWithFilter(Inventory target, int maxTransferAmount, HashSet<ResourceConfig> filters)
    {
        int transferredTotal = 0;
        foreach (var item in currentStacks)
        {
            if (item.amount <= 0 || !filters.Contains(item.resourceConfig))
                continue;
            var targetCurrent = target.GetCurrent(item.resourceConfig);
            if (targetCurrent == null)
            {
                if (target.acceptMode == InventoryAcceptMode.AllowAll)
                {
                    targetCurrent = new ResourceStack(item.resourceConfig, 0, defaultMaxValue);
                    targetCurrent.amount = 0;
                    target.currentStacks.Add(targetCurrent);
                }
                else
                {
                    continue;
                }
            }
            int spaceLeft = targetCurrent.storageLimit - targetCurrent.amount;
            if (spaceLeft <= 0)
                continue;
            int transferable = Math.Min(item.amount, Math.Min(spaceLeft, maxTransferAmount - transferredTotal));
            if (transferable <= 0)
                break;
            item.amount -= transferable;
            targetCurrent.amount += transferable;
            transferredTotal += transferable;
            if (transferredTotal >= maxTransferAmount)
                break;
        }
        return transferredTotal > 0;
    }

    public bool TransferToWithIgnore(Inventory target, int maxTransferAmount, HashSet<ResourceConfig> ignores)
    {
        int transferredTotal = 0;
        foreach (var item in currentStacks)
        {
            if (item.amount <= 0 || ignores.Contains(item.resourceConfig))
                continue;
            var targetCurrent = target.GetCurrent(item.resourceConfig);
            if (targetCurrent == null)
            {
                if (target.acceptMode == InventoryAcceptMode.AllowAll)
                {
                    targetCurrent = new ResourceStack(item.resourceConfig, 0, defaultMaxValue);
                    targetCurrent.amount = 0;
                    target.currentStacks.Add(targetCurrent);
                }
                else
                {
                    continue;
                }
            }
            int spaceLeft = targetCurrent.storageLimit - targetCurrent.amount;
            if (spaceLeft <= 0)
                continue;
            int transferable = Math.Min(item.amount, Math.Min(spaceLeft, maxTransferAmount - transferredTotal));
            if (transferable <= 0)
                break;
            item.amount -= transferable;
            targetCurrent.amount += transferable;
            transferredTotal += transferable;
            if (transferredTotal >= maxTransferAmount)
                break;
        }
        return transferredTotal > 0;
    }

    public bool HasEnough(ResourceStack required)
    {
        var current = currentStacks.FirstOrDefault(r => r.Equals(required));
        return current != null && current.amount >= required.amount;
    }

    /// <summary>
    /// 检查是否可以接收指定资源
    /// </summary>
    public bool CanReceive(ResourceStack type)
    {
        // 先检查过滤规则
        if (!PassesFilter(type.resourceConfig))
            return false;

        // 如果是AllowAll模式，总是可以接收
        if (acceptMode == InventoryAcceptMode.AllowAll)
            return true;

        // OnlyDefined模式下，必须已经定义了这个资源
        return GetCurrent(type.resourceConfig) != null;
    }

    /// <summary>
    /// 交换资源（用于交易）
    /// </summary>
    public bool ExChange(Inventory other, List<ResourceStack> myPay, List<ResourceStack> otherPay)
    {
        // 检查所有资源是否都能通过过滤
        foreach (var pay in myPay)
            if (!PassesFilter(pay.resourceConfig))
                return false;
        foreach (var pay in otherPay)
            if (!other.PassesFilter(pay.resourceConfig))
                return false;

        // 检查双方是否有足够的资源
        foreach (var pay in myPay)
            if (!HasEnough(pay))
                return false;
        foreach (var pay in otherPay)
            if (!other.HasEnough(pay))
                return false;

        // 检查双方是否有足够的空间
        foreach (var pay in otherPay)
            if (!CanAddItem(pay.resourceConfig, pay.amount))
                return false;
        foreach (var pay in myPay)
            if (!other.CanAddItem(pay.resourceConfig, pay.amount))
                return false;

        // 执行交换
        foreach (var pay in myPay)
        {
            RemoveItem(pay.resourceConfig, pay.amount);
            other.AddItem(pay.resourceConfig, pay.amount);
        }
        foreach (var pay in otherPay)
        {
            other.RemoveItem(pay.resourceConfig, pay.amount);
            AddItem(pay.resourceConfig, pay.amount);
        }

        return true;
    }

    /// <summary>
    /// 自交换资源（用于生产）
    /// </summary>
    public bool SelfExchange(List<ResourceStack> consume, List<ResourceStack> produce)
    {
        // 检查所有资源是否都能通过过滤
        foreach (var item in consume.Concat(produce))
            if (!PassesFilter(item.resourceConfig))
                return false;

        // 检查是否有足够的消耗资源
        foreach (var item in consume)
            if (!HasEnough(item))
                return false;

        // 检查是否有足够的空间存放产出
        foreach (var item in produce)
            if (!CanAddItem(item.resourceConfig, item.amount))
                return false;

        // 执行交换
        foreach (var item in consume)
            RemoveItem(item.resourceConfig, item.amount);
        foreach (var item in produce)
            AddItem(item.resourceConfig, item.amount);

        return true;
    }

    public float GetResourceRatioLimitInvolvingList(HashSet<ResourceConfig> resourceConfigs)
    {
        int count = 0;
        int totalLimit = 0;
        foreach (var stack in currentStacks)
        {
            if (resourceConfigs.Contains(stack.resourceConfig))
            {
                count += stack.amount;
                totalLimit += stack.storageLimit;
            }
        }
        if (totalLimit == 0) return 0f;
        return count / (float)totalLimit;
    }

    public float GetResourceRatioLimitAgainstList(HashSet<ResourceConfig> resourceConfigs)
    {
        int count = 0;
        int totalLimit = 0;
        foreach (var stack in currentStacks)
        {
            if (!resourceConfigs.Contains(stack.resourceConfig))
            {
                count += stack.amount;
                totalLimit += stack.storageLimit;
            }
        }
        if (totalLimit == 0) return 0f;
        return count / (float)totalLimit;
    }

    public (float involving, float against) GetResourceRatioLimitInvolvingAgainstList(HashSet<ResourceConfig> resourceConfigs)
    {
        int countInvolving = 0;
        int totalInvolvingLimit = 0;
        int countAgainst = 0;
        int totalAgainstLimit = 0;
        foreach (var stack in currentStacks)
        {
            if (resourceConfigs.Contains(stack.resourceConfig))
            {
                countInvolving += stack.amount;
                totalInvolvingLimit += stack.storageLimit;
            }
            else
            {
                countAgainst += stack.amount;
                totalAgainstLimit += stack.storageLimit;
            }
        }
        float involving = totalInvolvingLimit == 0 ? 0f : countInvolving / (float)totalInvolvingLimit;
        float against = totalAgainstLimit == 0 ? 0f : countAgainst / (float)totalAgainstLimit;
        return (involving, against);
    }

    public float GetResourceMappingWithFilter(
        Inventory others, HashSet<ResourceConfig> filter)
    {
        List<ResourceStack> result = new List<ResourceStack>();
        int totalNeeds = 0;
        int totalTransfer = 0;
        foreach (var stack in currentStacks)
        {
            if (!filter.Contains(stack.resourceConfig)) continue;
            int othersAmount = others.GetCurrent(stack.resourceConfig)?.amount ?? 0;
            if (othersAmount != 0)
            {
                result.Add(new ResourceStack(
                    stack.resourceConfig, Math.Min(stack.storageLimit - stack.amount, othersAmount)));
                totalTransfer += Math.Min(stack.storageLimit - stack.amount, othersAmount);
            }
            totalNeeds += stack.storageLimit - stack.amount;
        }
        if (totalNeeds == 0) return 0f;
        return MathUtility.FastSqrt(totalTransfer / (float)totalNeeds);
    }

    // 兼容旧接口（ResourceType+int）
    [System.Obsolete("请使用ResourceStack+ResourceConfig的新接口，旧接口已废弃", true)]
    public bool AddItem(ResourceType type, int subType, int amount)
    {
        throw new System.NotSupportedException("请使用ResourceStack+ResourceConfig的新接口");
    }
    [System.Obsolete("请使用ResourceStack+ResourceConfig的新接口，旧接口已废弃", true)]
    public bool RemoveItem(ResourceType type, int subType, int amount)
    {
        throw new System.NotSupportedException("请使用ResourceStack+ResourceConfig的新接口");
    }
    [System.Obsolete("请使用ResourceStack+ResourceConfig的新接口，旧接口已废弃", true)]
    public int GetItemCount(ResourceType type, int subType)
    {
        throw new System.NotSupportedException("请使用ResourceStack+ResourceConfig的新接口");
    }
    #region 保存与加载
    public GameSaveData GetSaveData()
    {
        return new InventorySaveData()
        {
            ownerType = ownerType,
            currentStacks = currentStacks.Select(r => r.GetSaveData() as ResourceStackSaveData).ToList(),
            acceptMode = acceptMode,
            filterMode = filterMode,
            acceptList = hashSetToDict(acceptList),
            rejectList = hashSetToDict(rejectList),
            defaultMaxValue = defaultMaxValue
        };
    }

    public void LoadFromData(GameSaveData data)
    {
        Debug.LogWarning("Inventory的LoadFromData还没有实现");
    }
    
    private Dictionary<ResourceType, int> hashSetToDict(HashSet<ResourceConfig> hashSet){
        Dictionary<ResourceType, int> dict = new Dictionary<ResourceType, int>();
        foreach (var config in hashSet)
        {
            dict[config.type] = config.subType;
        }
        return dict;
    }
    #endregion
}