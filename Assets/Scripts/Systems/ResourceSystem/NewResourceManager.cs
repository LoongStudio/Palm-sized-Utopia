using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class NewResourceManager : SingletonManager<NewResourceManager>, ISaveable
{

    [LabelText("打印调试信息")] [SerializeField] private bool showDebugInfo = false;

    [LabelText("资源配置"),SerializeField] private ResourceOverallConfig resourceOverallConfig;
    [LabelText("当前资源"),SerializeField] private List<ResourceStack> resourceStacks;
    
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
    private void Initialize()
    {
        InitializeResourceStacks();
    }

    private void InitializeResourceStacks()
    {
        if(resourceOverallConfig == null){
            Debug.LogError("资源配置为空");
            return;
        }
        resourceStacks = resourceOverallConfig.resourceStacks;
        if(showDebugInfo){
            Debug.Log($"资源配置初始化完成，资源数量：{resourceStacks.Count}");
            if(resourceStacks.Count == 0){
                Debug.LogWarning("[ResourceManager] 当前资源栈为空");
            }
        }
    }
    #endregion

    #region 资源操作
    public bool AddResource(ResourceConfig resource, int amount){
        if(!CanAddResource(resource, amount)){
            return false;
        }
        SetResource(resource, amount);
        return true;
    }
    public bool RemoveResource(ResourceConfig resource, int amount){
        if(!CanRemoveResource(resource, amount)){
            return false;
        }
        SetResource(resource, -amount);
        return true;
    }
    private void SetResource(ResourceConfig resource, int amount){
        ResourceStack resourceStack = GetResource(resource);
        resourceStack.amount += amount;
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
    public ResourceStack GetResource(ResourceConfig resource){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return null;
        }
        return resourceStacks.Find(stack => stack.resourceConfig == resource);
    }
    public List<ResourceStack> Resources => resourceStacks;
    public List<ResourceStack> ResourceSettings => resourceOverallConfig.resourceStacks;
    public int GetResourceLimit(ResourceConfig resource){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return 0;
        }
        return GetResource(resource).storageLimit;
    }
    public bool HasEnoughResource(ResourceConfig resource, int amount){
        if(!IsResourceExist(resource)){
            Debug.LogWarning("[ResourceManager] 资源不存在");
            return false;
        }
        return GetResource(resource).amount >= amount;
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