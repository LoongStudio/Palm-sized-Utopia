public static class GameEvents 
{
    // 资源相关事件
    public static event System.Action<ResourceEventArgs> OnResourceChanged;
    public static event System.Action<ResourceEventArgs> OnResourceAdded;
    public static event System.Action<ResourceEventArgs> OnResourceRemoved;
    
    // 建筑相关事件
    public static event System.Action<BuildingEventArgs> OnBuildingBuilt;
    public static event System.Action<BuildingEventArgs> OnBuildingUpgraded;
    public static event System.Action<BuildingEventArgs> OnBuildingDestroyed;
    
    // NPC相关事件
    public static event System.Action<NPCEventArgs> OnNPCHired;
    public static event System.Action<NPCEventArgs> OnNPCFired;
    public static event System.Action<NPCEventArgs> OnNPCStateChanged;
    public static event System.Action<NPCEventArgs> OnNPCRelationshipChanged;
    public static event System.Action<NPCEventArgs> OnNPCSocialInteraction;
    
    // 游戏流程事件
    public static event System.Action OnGameStarted;
    public static event System.Action OnGamePaused;
    public static event System.Action OnDayPassed;
    
    // 触发事件的方法
    public static void TriggerResourceChanged(ResourceEventArgs args) 
    {
        OnResourceChanged?.Invoke(args);
    }
    
    public static void TriggerBuildingBuilt(BuildingEventArgs args) 
    {
        OnBuildingBuilt?.Invoke(args);
    }
    public static void TriggerNPCStateChanged(NPCEventArgs args)
    {
        OnNPCStateChanged?.Invoke(args);
    }
    public static void TriggerNPCRelationshipChanged(NPCEventArgs args)
    {
        OnNPCRelationshipChanged?.Invoke(args);
    }
    public static void TriggerNPCSocialInteraction(NPCEventArgs args) 
    {
        OnNPCSocialInteraction?.Invoke(args);
    }
    // ... 其他触发方法
}

/* 用法参考
发布事件（系统内部）：
public class ResourceManager : MonoBehaviour 
{
    public bool AddResource(ResourceType type, int subType, int amount) 
    {
        int oldAmount = GetResourceAmount(type, subType);
        
        // 执行资源添加逻辑
        resources[type][subType] += amount;
        
        int newAmount = GetResourceAmount(type, subType);
        
        // 发布事件，通知其他系统
        var eventArgs = new ResourceEventArgs(type, subType, oldAmount, newAmount, "系统添加");
        GameEvents.TriggerResourceChanged(eventArgs);
        
        return true;
    }
}

监听事件（其他系统）：
public class ReportManager : MonoBehaviour 
{
    private void Start() 
    {
        // 订阅所有需要记录的事件
        GameEvents.OnResourceChanged += RecordResourceChange;
        GameEvents.OnBuildingBuilt += RecordBuildingEvent;
        GameEvents.OnNPCHired += RecordNPCEvent;
    }
    
    private void RecordResourceChange(ResourceEventArgs args) 
    {
        // 记录资源变化到报告系统
        dataCollector.RecordResourceChange(args);
    }
    
    private void RecordBuildingEvent(BuildingEventArgs args) 
    {
        // 记录建筑事件
        dataCollector.RecordBuildingEvent(args);
    }
}
public class UIManager : MonoBehaviour 
{
    private void Start() 
    {
        // UI系统也可以监听这些事件来更新界面
        GameEvents.OnResourceChanged += UpdateResourceDisplay;
        GameEvents.OnBuildingBuilt += UpdateBuildingList;
    }
    
    private void UpdateResourceDisplay(ResourceEventArgs args) 
    {
        // 更新资源显示UI
        resourceUI.UpdateDisplay(args.resourceType, args.newAmount);
    }
}

资源变化 → 通知UI更新、报告系统记录、检查解锁条件
建筑建造 → 通知加成系统重新计算、报告系统记录、UI更新
NPC状态变化 → 通知社交系统、工作效率重新计算、报告记录
时间推进 → 触发工资支付、生产周期、NPC状态检查
*/
