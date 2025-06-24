using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    // 新版：资源堆栈结构
    public List<ResourceStack> currentStacks;
    // 只保留ResourceStack相关字段
    
    // 旧版：已过期，待移除
    [System.Obsolete("请使用currentStacks替代")] public List<SubResourceValue<int>> currentSubResource;
    [System.Obsolete("已废弃，limit由ResourceStack维护")] public List<SubResourceValue<int>> maximumSubResource;
    [System.Obsolete("请使用ResourceStack相关逻辑替代")] public List<SubResource> whiteList;
    [System.Obsolete("请使用ResourceStack相关逻辑替代")] public List<SubResource> blackList;
    // 资源过滤列表
    public List<SubResource> acceptList = new();
    public List<SubResource> rejectList = new();
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
        List<SubResource> acceptList = null,
        List<SubResource> rejectList = null,
        InventoryOwnerType ownerType = InventoryOwnerType.None, 
        int defaultMaxValue = 100)
    {
        this.currentStacks = currentStacks ?? new List<ResourceStack>();
        this.acceptMode = acceptMode;
        this.filterMode = filterMode;
        this.acceptList = acceptList ?? new List<SubResource>();
        this.rejectList = rejectList ?? new List<SubResource>();
        this.ownerType = ownerType;
        this.defaultMaxValue = defaultMaxValue;
    }

    // 通过ResourceStack查找
    private ResourceStack GetCurrent(ResourceStack type)
    {
        return currentStacks.FirstOrDefault(r => r.Equals(type));
    }

    /// <summary>
    /// 检查资源是否通过过滤规则
    /// </summary>
    private bool PassesFilter(ResourceStack resource)
    {
        if (filterMode == InventoryListFilterMode.None)
            return true;

        // 先检查黑名单（如果启用了黑名单）
        if ((filterMode == InventoryListFilterMode.RejectList || filterMode == InventoryListFilterMode.Both) 
            && rejectList != null)
        {
            foreach (var reject in rejectList)
            {
                if (reject.resourceType == resource.type && reject.subType == resource.subType)
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
                if (accept.resourceType == resource.type && accept.subType == resource.subType)
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
    public bool CanAddItem(ResourceStack type, int amount)
    {
        // 先检查过滤规则
        if (!PassesFilter(type))
            return false;

        var cur = GetCurrent(type);
        if (cur == null)
        {
            // AllowAll模式下可自动补全
            if (acceptMode == InventoryAcceptMode.AllowAll)
                return amount <= type.storageLimit;
            return false;
        }
        return cur.CanAdd(amount);
    }

    /// <summary>
    /// 添加物品，按输入策略处理（支持type+subType+limit）
    /// </summary>
    public bool AddItem(ResourceStack type, int amount)
    {
        // 先检查过滤规则
        if (!PassesFilter(type))
            return false;

        var cur = GetCurrent(type);
        if (cur == null)
        {
            if (acceptMode == InventoryAcceptMode.AllowAll)
            {
                cur = type.Clone();
                cur.amount = Mathf.Min(amount, cur.storageLimit);
                currentStacks.Add(cur);
                if (ownerType == InventoryOwnerType.Building)
                    OnResourceChanged?.Invoke(type.type, type.subType, cur.amount);
                return true;
            }
            else
            {
                return false;
            }
        }
        if (!cur.CanAdd(amount)) return false;
        cur.amount += amount;
        if (ownerType == InventoryOwnerType.Building)
            OnResourceChanged?.Invoke(type.type, type.subType, amount);
        return true;
    }

    public bool RemoveItem(ResourceStack type, int amount)
    {
        var cur = GetCurrent(type);
        if (cur == null || cur.amount < amount) return false;
        cur.amount -= amount;
        if (ownerType == InventoryOwnerType.Building)
            OnResourceChanged?.Invoke(type.type, type.subType, -amount);
        return true;
    }

    public int GetItemCount(ResourceStack type)
    {
        var cur = GetCurrent(type);
        return cur?.amount ?? 0;
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
            var type = item;
            if (item.amount <= 0)
                continue;
            var targetCurrent = target.GetCurrent(type);
            if (targetCurrent == null)
            {
                if (target.acceptMode == InventoryAcceptMode.AllowAll)
                {
                    targetCurrent = new ResourceStack(type.type, type.subType, type.storageLimit);
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
        if (!PassesFilter(type))
            return false;

        // 如果是AllowAll模式，总是可以接收
        if (acceptMode == InventoryAcceptMode.AllowAll)
            return true;

        // OnlyDefined模式下，必须已经定义了这个资源
        return GetCurrent(type) != null;
    }

    /// <summary>
    /// 交换资源（用于交易）
    /// </summary>
    public bool ExChange(Inventory other, List<ResourceStack> myPay, List<ResourceStack> otherPay)
    {
        // 检查所有资源是否都能通过过滤
        foreach (var pay in myPay)
            if (!PassesFilter(pay))
                return false;
        foreach (var pay in otherPay)
            if (!other.PassesFilter(pay))
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
            if (!CanAddItem(pay, pay.amount))
                return false;
        foreach (var pay in myPay)
            if (!other.CanAddItem(pay, pay.amount))
                return false;

        // 执行交换
        foreach (var pay in myPay)
        {
            RemoveItem(pay, pay.amount);
            other.AddItem(pay, pay.amount);
        }
        foreach (var pay in otherPay)
        {
            other.RemoveItem(pay, pay.amount);
            AddItem(pay, pay.amount);
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
            if (!PassesFilter(item))
                return false;

        // 检查是否有足够的消耗资源
        foreach (var item in consume)
            if (!HasEnough(item))
                return false;

        // 检查是否有足够的空间存放产出
        foreach (var item in produce)
            if (!CanAddItem(item, item.amount))
                return false;

        // 执行交换
        foreach (var item in consume)
            RemoveItem(item, item.amount);
        foreach (var item in produce)
            AddItem(item, item.amount);

        return true;
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
}