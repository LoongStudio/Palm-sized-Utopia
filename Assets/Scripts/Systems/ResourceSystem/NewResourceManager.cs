using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class ResourceManager : SingletonManager<ResourceManager>, ISaveable
{

    [LabelText("打印调试信息")] [SerializeField] private bool showDebugInfo = false;

    [LabelText("资源配置"),SerializeField] private ResourceOverallConfig resourceOverallConfig;
    [LabelText("当前资源"),SerializeField] private List<ResourceStack> resourceStacks;

    // 常用资源，用于其他地方快速调用
    private ResourceConfig goldConfig;
    private ResourceConfig ticketConfig;
    
    #region 生命周期
    private void OnEnable()
    {
        RegisterEvent();
    }
    private void OnDisable()
    {
        UnregisterEvent();
    }
    #endregion

    #region 事件
    private void RegisterEvent()
    {

    }
    private void UnregisterEvent()
    {

    }
    #endregion
    
    #region 初始化
    public void Initialize()
    {
        InitializeResourceStacks();
    }

    private void InitializeResourceStacks()
    {
        if(resourceOverallConfig == null){
            Debug.LogError("资源配置为空");
            return;
        }
        // 深拷贝资源配置到resourceStacks，避免修改原始配置
        resourceStacks = new List<ResourceStack>();
        foreach(var stack in resourceOverallConfig.resourceStacks){
            resourceStacks.Add(stack.Clone());
        }
        
        if(showDebugInfo){
            Debug.Log($"资源配置初始化完成，资源数量：{resourceStacks.Count}");
            if(resourceStacks.Count == 0){
                Debug.LogWarning("[ResourceManager] 当前资源栈为空");
            }
        }
        goldConfig = resourceOverallConfig.gold;
        ticketConfig = resourceOverallConfig.ticket;
    }
    #endregion

    #region 资源操作
    public bool AddResource(ResourceType resourceType, int subType, int amount){
        ResourceConfig resource = GetResourceConfig(resourceType, subType);
        return AddResource(resource, amount);
    }
    public bool AddResource(ResourceConfig resource, int amount){
        if(!CanAddResource(resource, amount)){
            return false;
        }
        SetResource(resource, GetResource(resource).amount + amount);
        return true;
    }
    public bool RemoveResource(ResourceType resourceType, int subType, int amount){
        ResourceConfig resource = GetResourceConfig(resourceType, subType);
        return RemoveResource(resource, amount);
    }
    public bool RemoveResource(ResourceConfig resource, int amount){
        if(!CanRemoveResource(resource, amount)){
            return false;
        }
        SetResource(resource, GetResource(resource).amount - amount);
        return true;
    }
    private void SetResource(ResourceConfig resource, int amount){
        ResourceStack resourceStack = GetResource(resource);
        int oldAmount = resourceStack.amount;
        resourceStack.amount = amount;

        // 触发资源变化事件
        if(showDebugInfo){
            Debug.Log($"[ResourceManager] 资源 {resource.type} {resource.subType} 数量从 {oldAmount} 变为 {amount}");
        }
        GameEvents.TriggerResourceChanged(new ResourceEventArgs(){
            resourceType = resource.type,
            subType = resource.subType,
            oldAmount = oldAmount,
            newAmount = amount,
            changeAmount = amount - oldAmount,
            changeReason = "SetResource",
            timestamp = System.DateTime.Now
        });
    }
    public bool SetResourceLimit(ResourceConfig resource, int limit){
        if(!IsValidOperation(resource, limit)){
            return false;
        }

        ResourceStack resourceStack = GetResource(resource);
        resourceStack.storageLimit = limit;
        return true;
    }
    #endregion

    #region 资源查询
    public List<ResourceStack> Resources => resourceStacks;
    public List<ResourceStack> ResourceSettings 
    { 
        get 
        {
            var result = new List<ResourceStack>();
            foreach(var stack in resourceOverallConfig.resourceStacks)
            {
                result.Add(stack.Clone());
            }
            return result;
        }
    }
    public ResourceConfig Gold => goldConfig;
    public ResourceConfig Ticket => ticketConfig;
    public ResourceStack GetResource(ResourceConfig resource){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return null;
        }
        return resourceStacks.Find(stack => stack.resourceConfig == resource);
    }
    public ResourceConfig GetResourceConfig(ResourceType resourceType, int subType){
        var stack = resourceStacks.Find(stack => stack.resourceConfig.type == resourceType && stack.resourceConfig.subType == subType);
        if(stack == null){
            Debug.LogWarning($"[ResourceManager] 未找到资源类型 {resourceType} 子类型 {subType} 的配置");
            return null;
        }
        return stack.resourceConfig;
    }
    public int GetResourceAmount(ResourceConfig resource){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return 0;
        }
        return GetResource(resource).amount;
    }
    public int GetResourceAmount(ResourceType resourceType, int subType){
        ResourceConfig resource = GetResourceConfig(resourceType, subType);
        return GetResourceAmount(resource);
    }
    public int GetResourceLimit(ResourceConfig resource){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return 0;
        }
        return GetResource(resource).storageLimit;
    }
    public int GetResourceLimit(ResourceType resourceType, int subType){
        ResourceConfig resource = GetResourceConfig(resourceType, subType);
        return GetResourceLimit(resource);
    }
    public bool HasEnoughResource(ResourceConfig resource, int amount){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return false;
        }
        return GetResource(resource).amount >= amount;
    }
    public bool HasEnoughResource(ResourceType resourceType, int subType, int amount){
        // 根据资源类型和子类型获取资源配置
        ResourceConfig resource = GetResourceConfig(resourceType, subType);
        return HasEnoughResource(resource, amount);
    }
    public bool CanAddResource(ResourceConfig resource, int amount){
        if(!IsValidOperation(resource, amount)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return false;
        }
        return GetResource(resource).amount + amount <= GetResource(resource).storageLimit;
    }
    public bool CanRemoveResource(ResourceConfig resource, int amount){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return false;
        }
        return GetResource(resource).amount - amount >= 0;
    }

    #endregion

    #region 辅助函数
    private bool IsResourceExist(ResourceConfig resource){
        if(resource == null || resourceStacks == null){
            Debug.LogError("[ResourceManager] 传入资源或资源栈为空");
            return false;
        }
        return resourceStacks.Find(stack => stack.resourceConfig == resource) != null;
    }
    private bool IsValidOperation(ResourceConfig resource, int amount){
        if(!IsResourceExist(resource)){
            Debug.LogError("[ResourceManager] 资源不存在");
            return false;
        }
        if(amount < 0){
            Debug.LogError("[ResourceManager] 操作数量不能小于等于0");
            return false;
        }
        return true;
    }
    #endregion

    #region 保存与加载
    public GameSaveData GetSaveData(){return null;}
    public void LoadFromData(GameSaveData data){}
    #endregion
}