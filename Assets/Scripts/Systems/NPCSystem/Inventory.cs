using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public enum InventoryOwnerType
    {
        Building,
        NPC,
        None
    }

    public InventoryOwnerType ownerType = InventoryOwnerType.NPC;
    public event Action<ResourceType, int, int> OnResourceChanged; // type, subType, value变化量
    public List<SubResourceValue<int>> currentSubResource;
    public List<SubResourceValue<int>> maximumSubResource;
    public List<SubResource> whiteList;
    public List<SubResource> blackList;
    // 策略选项
    public InventoryAcceptMode acceptMode = InventoryAcceptMode.OnlyDefined;
    public InventoryListFilterMode filterMode = InventoryListFilterMode.None;
    public List<SubResource> acceptList = new();
    public List<SubResource> rejectList = new();
    // 事件
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public event System.Action<ResourceType, int> OnItemAdded;
    // public event System.Action<ResourceType, int> OnItemRemoved;
    // public event System.Action OnInventoryFull;
    // public event System.Action OnInventoryEmpty;
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
    public int defaultMaxValue = 100; // 新增默认最大容量
    /// <summary>
    /// 推荐唯一构造函数，必须传入所有关键参数。
    /// </summary>
    public Inventory(
        List<SubResourceValue<int>> currentSubResource,
        List<SubResourceValue<int>> maximumSubResource,
        InventoryAcceptMode acceptMode,
        InventoryListFilterMode filterMode,
        List<SubResource> acceptList,
        List<SubResource> rejectList,
        InventoryOwnerType ownerType = InventoryOwnerType.None,
        int defaultMaxValue = 100) // 新增参数
    {
        this.currentSubResource = currentSubResource ?? new List<SubResourceValue<int>>();
        this.maximumSubResource = maximumSubResource ?? new List<SubResourceValue<int>>();
        this.acceptMode = acceptMode;
        this.filterMode = filterMode;
        this.acceptList = acceptList ?? new List<SubResource>();
        this.rejectList = rejectList ?? new List<SubResource>();
        this.ownerType = ownerType;
        this.defaultMaxValue = defaultMaxValue;
        // 自动补全机制同原有逻辑
        var allTypes = new HashSet<SubResource>();
        foreach (var c in this.currentSubResource)
            allTypes.Add(c.subResource);
        foreach (var m in this.maximumSubResource)
            allTypes.Add(m.subResource);
        foreach (var type in allTypes)
        {
            if (!this.currentSubResource.Exists(r => r.subResource.Equals(type)))
                this.currentSubResource.Add(new SubResourceValue<int>(type, 0));
            if (!this.maximumSubResource.Exists(r => r.subResource.Equals(type)))
                this.maximumSubResource.Add(new SubResourceValue<int>(type, defaultMaxValue)); // 用默认值
        }
    }

    // [Obsolete("请使用完整参数构造函数")] public Inventory() { throw new NotSupportedException("请使用完整参数构造函数"); }
    // [Obsolete("请使用完整参数构造函数")] public Inventory(List<SubResourceValue<int>> currentSubResource, List<SubResourceValue<int>> maximumSubResource) { throw new NotSupportedException("请使用完整参数构造函数"); }
    // [Obsolete("请使用完整参数构造函数")] public Inventory(List<SubResourceValue<int>> currentSubResource, List<SubResourceValue<int>> maximumSubResource, List<SubResource> whiteList, List<SubResource> blackList, InventoryOwnerType ownerType = InventoryOwnerType.None) { throw new NotSupportedException("请使用完整参数构造函数"); }

    public Inventory(
        List<SubResourceValue<int>> currentSubResource,
        List<SubResourceValue<int>> maximumSubResource,
        InventoryOwnerType ownerType = InventoryOwnerType.None,
        int defaultMaxValue = 100)
    {
        this.ownerType = ownerType;
        this.currentSubResource = currentSubResource;
        this.maximumSubResource = maximumSubResource;
        this.defaultMaxValue = defaultMaxValue;
        // 构建资源种类全集
        var allTypes = new HashSet<SubResource>();
        foreach (var c in this.currentSubResource)
            allTypes.Add(c.subResource);
        foreach (var m in this.maximumSubResource)
            allTypes.Add(m.subResource);

        // 补 current 中缺失的
        foreach (var type in allTypes)
        {
            if (!this.currentSubResource.Exists(r => r.subResource.Equals(type)))
                this.currentSubResource.Add(new SubResourceValue<int>(type, 0));

            if (!this.maximumSubResource.Exists(r => r.subResource.Equals(type)))
                this.maximumSubResource.Add(new SubResourceValue<int>(type, defaultMaxValue));
        }
    }
    // 获取匹配资源项（默认忽略 subType）
    public bool TransferTo(Inventory target, int maxTransferAmount)
    {
        int transferredTotal = 0;

        foreach (var item in currentSubResource)
        {
            var type = item.subResource;

            // 白名单优先
            if (target.whiteList != null && target.whiteList.Count > 0 && !target.whiteList.Contains(type))
                continue;

            // 黑名单排除
            if (target.blackList != null && target.blackList.Contains(type))
                continue;

            if (item.resourceValue <= 0)
                continue;

            // 获取目标当前值
            var targetCurrent = target.currentSubResource.FirstOrDefault(r => r.subResource.Equals(type));
            if (targetCurrent == null)
            {
                targetCurrent = new SubResourceValue<int>(type, 0);
                target.currentSubResource.Add(targetCurrent);
            }

            // 获取目标最大值
            var targetMax = target.maximumSubResource.FirstOrDefault(r => r.subResource.Equals(type));
            int maxValue = targetMax?.resourceValue ?? int.MaxValue;

            int spaceLeft = Math.Max(0, maxValue - targetCurrent.resourceValue);
            if (spaceLeft <= 0)
                continue;

            // 可转移量 = 当前持有量 ∩ 空间 ∩ 剩余传输配额
            int transferable = Math.Min(item.resourceValue, Math.Min(spaceLeft, maxTransferAmount - transferredTotal));
            if (transferable <= 0)
                break;

            // 执行转移
            item.resourceValue -= transferable;
            targetCurrent.resourceValue += transferable;
            transferredTotal += transferable;

            if (transferredTotal >= maxTransferAmount)
                break;
        }

        return transferredTotal > 0;
    }

    // 新增：统一资源允许性判断
    private bool IsResourceAllowed(SubResource type)
    {
        // 先处理filterMode
        switch (filterMode)
        {
            case InventoryListFilterMode.AcceptList:
                if (!acceptList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.RejectList:
                if (rejectList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.Both:
                if (rejectList.Contains(type)) return false;
                if (!acceptList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.None:
            default:
                break;
        }
        // 再处理acceptMode
        if (acceptMode == InventoryAcceptMode.OnlyDefined)
        {
            bool currHas = currentSubResource.Exists(r => r.subResource.Equals(type));
            bool maxHas = maximumSubResource.Exists(r => r.subResource.Equals(type));
            if (!currHas && !maxHas) return false;
        }
        return true;
    }

    public bool HasEnough(SubResourceValue<int> required)
    {
        if (!IsResourceAllowed(required.subResource)) return false;
        var current = currentSubResource.FirstOrDefault(r =>
            r.subResource.resourceType == required.subResource.resourceType &&
            r.subResource.subType == required.subResource.subType);
        return current != null && current.resourceValue >= required.resourceValue;
    }

    public bool CanReceive(SubResourceValue<int> output)
    {
        if (!IsResourceAllowed(output.subResource)) return false;
        if (whiteList != null && !whiteList.Contains(output.subResource)) return false;
        if (blackList != null && blackList.Contains(output.subResource)) return false;
        var current = currentSubResource.FirstOrDefault(r =>
            r.subResource.resourceType == output.subResource.resourceType &&
            r.subResource.subType == output.subResource.subType);
        var max = maximumSubResource.FirstOrDefault(r =>
            r.subResource.resourceType == output.subResource.resourceType &&
            r.subResource.subType == output.subResource.subType);

        int currentValue = current?.resourceValue ?? 0;
        int maxValue = max?.resourceValue ?? 0;

        return maxValue == 0 || currentValue + output.resourceValue <= maxValue;
    }

    
    private SubResourceValue<int> GetCurrent(ResourceType type)
    {
        return currentSubResource.Find(r => r.subResource.resourceType == type);
    }

    private SubResourceValue<int> GetMaximum(ResourceType type)
    {
        return maximumSubResource.Find(r => r.subResource.resourceType == type);
    }
    
    /// <summary>
    /// 判断能否添加物品，先按过滤模式处理，再按主接收模式处理
    /// </summary>
    public bool CanAddItem(SubResource type, int amount)
    {
        // 1. 先处理白名单/黑名单过滤
        switch (filterMode)
        {
            case InventoryListFilterMode.AcceptList:
                if (!acceptList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.RejectList:
                if (rejectList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.Both:
                if (rejectList.Contains(type)) return false;
                if (!acceptList.Contains(type)) return false;
                break;
            case InventoryListFilterMode.None:
            default:
                break;
        }
        // 2. 主策略
        switch (acceptMode)
        {
            case InventoryAcceptMode.AllowAll:
                if (!currentSubResource.Exists(r => r.subResource.Equals(type)))
                    currentSubResource.Add(new SubResourceValue<int>(type, 0));
                if (!maximumSubResource.Exists(r => r.subResource.Equals(type)))
                    maximumSubResource.Add(new SubResourceValue<int>(type, defaultMaxValue));
                return true;
            case InventoryAcceptMode.OnlyDefined:
                bool currHas = currentSubResource.Exists(r => r.subResource.Equals(type));
                bool maxHas = maximumSubResource.Exists(r => r.subResource.Equals(type));
                if (!currHas && !maxHas) return false;
                if (!currHas && maxHas) currentSubResource.Add(new SubResourceValue<int>(type, 0));
                if (currHas && !maxHas)
                {
                    var curr = currentSubResource.First(r => r.subResource.Equals(type));
                    maximumSubResource.Add(new SubResourceValue<int>(type, curr.resourceValue));
                }
                return true;
        }
        return false;
    }

    /// <summary>
    /// 添加物品，按输入策略处理
    /// </summary>
    public bool AddItem(ResourceType type, int amount)
    {
        if (!CanAddItem(new SubResource(type, amount), amount)) return false;
        var cur = GetCurrent(type);
        var max = GetMaximum(type);
        int maxValue = max?.resourceValue ?? defaultMaxValue;
        int canAdd = Math.Min(amount, maxValue - cur.resourceValue);
        if (canAdd <= 0) return false;
        cur.resourceValue += canAdd;
        if (ownerType == InventoryOwnerType.Building)
            OnResourceChanged?.Invoke(type, cur.subResource.subType, canAdd);
        return canAdd > 0;
    }

    /// <summary>
    /// 判断能否移除物品，只判断数量
    /// </summary>
    public bool CanRemoveItem(ResourceType type, int amount)
    {
        var cur = GetCurrent(type);
        return cur != null && cur.resourceValue >= amount;
    }

    /// <summary>
    /// 移除物品，只判断数量
    /// </summary>
    public bool RemoveItem(ResourceType type, int amount)
    {
        var subType = new SubResource(type, amount);
        if (!IsResourceAllowed(subType)) return false;
        var cur = GetCurrent(type);
        if (cur == null || cur.resourceValue < amount) return false;
        cur.resourceValue -= amount;
        if (ownerType == InventoryOwnerType.Building)
            OnResourceChanged?.Invoke(type, cur.subResource.subType, -amount);
        return true;
    }

    public int GetItemCount(ResourceType type)
    {
        var subType = new SubResource(type, 0);
        if (!IsResourceAllowed(subType)) return 0;
        var cur = GetCurrent(type);
        return cur?.resourceValue ?? 0;
    }

    public bool IsFull()
    {
        foreach (var cur in currentSubResource)
        {
            var max = maximumSubResource.Find(m => m.subResource.Equals(cur.subResource));
            if (max != null && cur.resourceValue < max.resourceValue)
                return false;
        }
        return true;
    }

    public bool IsEmpty()
    {
        return currentSubResource.All(r => r.resourceValue == 0);
    }

    public float GetCapacityPercentage()
    {
        int totalCurrent = 0;
        int totalMax = 0;
        foreach (var cur in currentSubResource)
        {
            var max = maximumSubResource.Find(m => m.subResource.Equals(cur.subResource));
            if (max != null)
            {
                totalCurrent += cur.resourceValue;
                totalMax += max.resourceValue;
            }
        }
        return totalMax == 0 ? 0f : (float)totalCurrent / totalMax;
    }

    public Dictionary<ResourceType, int> GetAllItems()
    {
        var dict = new Dictionary<ResourceType, int>();
        foreach (var cur in currentSubResource)
        {
            if (dict.ContainsKey(cur.subResource.resourceType))
                dict[cur.subResource.resourceType] += cur.resourceValue;
            else
                dict[cur.subResource.resourceType] = cur.resourceValue;
        }
        return dict;
    }

    public void Clear()
    {
        foreach (var cur in currentSubResource)
            cur.resourceValue = 0;
    }

    

}