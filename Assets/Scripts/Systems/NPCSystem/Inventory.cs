using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    public List<SubResourceValue<int>> currentSubResource;
    public List<SubResourceValue<int>> maximumSubResource;
    public List<SubResource> whiteList;
    public List<SubResource> blackList;
    // 事件
    // TODO: 处理这些事件的订阅和触发，用GameEvents代替
    // public event System.Action<ResourceType, int> OnItemAdded;
    // public event System.Action<ResourceType, int> OnItemRemoved;
    // public event System.Action OnInventoryFull;
    // public event System.Action OnInventoryEmpty;
    public Inventory(
        List<SubResourceValue<int>> currentSubResource,
        List<SubResourceValue<int>> maximumSubResource,
        List<SubResource> whiteList,
        List<SubResource> blackList)
    {
        // 初始化输入
        this.currentSubResource = currentSubResource ?? new List<SubResourceValue<int>>();
        this.maximumSubResource = maximumSubResource ?? new List<SubResourceValue<int>>();

        // 构建所有出现过的资源类型
        var allTypes = new HashSet<SubResource>();
        foreach (var c in this.currentSubResource)
            allTypes.Add(c.subResource);
        foreach (var m in this.maximumSubResource)
            allTypes.Add(m.subResource);

        // --- 应用白名单优先规则 ---
        if (whiteList != null && whiteList.Count > 0)
        {
            allTypes.IntersectWith(whiteList);

            // 清除 current 和 max 中不在白名单中的资源
            this.currentSubResource = this.currentSubResource
                .Where(r => whiteList.Contains(r.subResource)).ToList();
            this.maximumSubResource = this.maximumSubResource
                .Where(r => whiteList.Contains(r.subResource)).ToList();
        }
        else if (blackList != null && blackList.Count > 0)
        {
            allTypes.ExceptWith(blackList);

            // 清除 current 和 max 中出现在黑名单中的资源
            this.currentSubResource = this.currentSubResource
                .Where(r => !blackList.Contains(r.subResource)).ToList();
            this.maximumSubResource = this.maximumSubResource
                .Where(r => !blackList.Contains(r.subResource)).ToList();
        }

        // --- 补齐 current / maximum 中缺失的资源项 ---
        foreach (var type in allTypes)
        {
            if (!this.currentSubResource.Exists(r => r.subResource.Equals(type)))
                this.currentSubResource.Add(new SubResourceValue<int>(type, 0));

            if (!this.maximumSubResource.Exists(r => r.subResource.Equals(type)))
                this.maximumSubResource.Add(new SubResourceValue<int>(type, 0));
        }
    }

    public Inventory()
    {
        currentSubResource = new List<SubResourceValue<int>>();
        maximumSubResource = new List<SubResourceValue<int>>();
    }

    public Inventory(
        List<SubResourceValue<int>> currentSubResource,
        List<SubResourceValue<int>> maximumSubResource)
    {
        this.currentSubResource = currentSubResource;
        this.maximumSubResource = maximumSubResource;
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
                this.maximumSubResource.Add(new SubResourceValue<int>(type, 0));
        }
    }
    // 获取匹配资源项（默认忽略 subType）
    
    public bool HasEnough(SubResourceValue<int> required)
    {
        var current = currentSubResource.FirstOrDefault(r =>
            r.subResource.resourceType == required.subResource.resourceType &&
            r.subResource.subType == required.subResource.subType);
        return current != null && current.resourceValue >= required.resourceValue;
    }

    public bool CanReceive(SubResourceValue<int> output)
    {
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

    public bool CanAddItem(ResourceType type, int amount)
    {
        var cur = GetCurrent(type);
        var max = GetMaximum(type);
        return cur != null && max != null && (cur.resourceValue + amount <= max.resourceValue);
    }

    public bool AddItem(ResourceType type, int amount)
    {
        if (!CanAddItem(type, amount)) return false;
        var cur = GetCurrent(type);
        cur.resourceValue += amount;
        return true;
    }

    public bool RemoveItem(ResourceType type, int amount)
    {
        var cur = GetCurrent(type);
        if (cur == null || cur.resourceValue < amount) return false;
        cur.resourceValue -= amount;
        return true;
    }

    public int GetItemCount(ResourceType type)
    {
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