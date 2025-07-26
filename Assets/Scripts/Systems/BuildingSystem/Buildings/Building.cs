using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using Sirenix.OdinInspector;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
public abstract class Building : MonoBehaviour, IUpgradeable, ISaveable, ISelectable, IBuffAffectable
{
    #region 字段和属性

    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    [Header("唯一标识")]
    [SerializeField] private string buildingId;

    [Header("基本属性")]
    public BuildingData data;
    public BuildingStatus status;
    public int currentLevel;
    public List<Vector2Int> positions;
    [ShowInInspector]
    public HashSet<ResourceConfig> AcceptResources;
    
    [Header("槽位管理")] 
    // public int maxSlotAmount = 3;  // 这里被我替换为了NPCSlotAmount，因为它在data中被定义了
    public List<NPC> assignedNPCs;
    public List<NPC> tempAssignedNPCs;
    public List<Equipment> installedEquipment;
    public Inventory inventory;           // 背包
    
    [Header("调试设置")]
    public float heightOffset = 0.1f;  // 文本显示高度
    public Color textColor = Color.white;
    public float textSize = 12;
    public bool alwaysShow = true;  // 是否始终显示，否则仅在选中时显示
    private GUIStyle textStyle;
    
    // 缓存PlaceableObject引用
    private PlaceableObject cachedPlaceableObject;

    #endregion

    #region 属性访问器

    /// <summary>
    /// 建筑的唯一标识符
    /// </summary>
    public string BuildingId
    {
        get
        {
            // 如果ID为空，生成新的GUID
            if (string.IsNullOrEmpty(buildingId))
            {
                buildingId = System.Guid.NewGuid().ToString();
                if (showDebugInfo)
                    Debug.Log($"[Building] 为建筑 {data?.buildingName} 生成新ID: {buildingId}");
            }
            return buildingId;
        }
        private set
        {
            buildingId = value;
        }
    }
    
    /// <summary>
    /// 获取关联的PlaceableObject组件
    /// </summary>
    public PlaceableObject PlaceableObject
    {
        get
        {
            if (cachedPlaceableObject == null)
            {
                cachedPlaceableObject = GetComponentInParent<PlaceableObject>();
            }
            return cachedPlaceableObject;
        }
    }

    public float BaseProductionSpeedMultiplier {
        get {
            return data.baseProductionSpeedMultiplier;
        }
    }
    
    public int NPCSlotAmount {
        get {
            return data.npcSlots;
        }
        set {
            data.npcSlots = value;
        }
    }
    
    public int NPCTempSlotAmount {
        get {
            return data.npcTempSlots;
        }
        set {
            data.npcTempSlots = value;
        }
    }
    
    public int EquipmentSlotAmount {
        get {
            return data.equipmentSlots;
        }
        set {
            data.equipmentSlots = value;
        }
    }
    #endregion

    #region Unity生命周期

    public virtual void Start() 
    {
        var _ = BuildingId; // 触发getter，如果没有ID会自动生成
        
        // 同步位置信息
        SyncPositionsFromPlaceable();
        
        // LoadFromData(); // TODO: 开始时 如果数据并没有正常加载，尝试重新从Data中读取
        if (!OnTryBuilt())
        {
            Debug.LogError($"[Building] 建筑 {this.ToString()} 放置失败, 游戏开始时所有建筑应正常被放置成功");    
        }
        InitialSelfStorage();
        // 注册资源变动事件
        if (inventory != null && inventory.ownerType == Inventory.InventoryOwnerType.Building)
        {
            inventory.OnResourceChanged += BuildingManager.Instance.OnBuildingResourceChanged;
        }
    }

    private void OnDrawGizmos()
    {
        if (!alwaysShow) return;
#if UNITY_EDITOR
        DrawDebugTextWithHandles();
#endif
    }

    /// <summary>
    /// Unity生命周期方法 - GameObject被销毁时调用
    /// </summary>
    private void OnDestroy()
    {
        // 调用自定义的销毁回调方法
        OnDestroyed();
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 初始化建筑存储
    /// </summary>
    public abstract void InitialSelfStorage();

    /// <summary>
    /// 设置建筑数据
    /// </summary>
    public virtual void SetBuildingData(BuildingPrefabData data)
    {
        // 设置基础数据
        this.data = data.buildingDatas;

        // 设置可放入资源，转换为HashSet
        if(data.productionBuildingDatas.acceptResources != null){
            AcceptResources = new HashSet<ResourceConfig>(data.productionBuildingDatas.acceptResources);
        }
        else
        {
            // 即使内容不存在也需要添加一个空列表
            AcceptResources = new HashSet<ResourceConfig>();
        }

        // 设置Inventory
        if(inventory != null){
            if(data.productionBuildingDatas.defaultResources != null){
                inventory.currentStacks = data.productionBuildingDatas.defaultResources;
            }
            inventory.acceptMode = data.productionBuildingDatas.defaultAcceptMode;
            inventory.filterMode = data.productionBuildingDatas.defaultFilterMode;
            if(data.productionBuildingDatas.defaultAcceptList != null){
                inventory.acceptList = new HashSet<ResourceConfig>(data.productionBuildingDatas.defaultAcceptList);
            }
            if(data.productionBuildingDatas.defaultRejectList != null){
                inventory.rejectList = new HashSet<ResourceConfig>(data.productionBuildingDatas.defaultRejectList);
            }
            inventory.defaultMaxValue = data.productionBuildingDatas.defaultMaxValue;
        }
    }

    /// <summary>
    /// 手动设置建筑ID（仅用于加载存档）
    /// </summary>
    public void SetBuildingId(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            buildingId = id;
            if (showDebugInfo)
                Debug.Log($"[Building] 设置建筑 {data?.buildingName} 的ID为: {buildingId}");
        }
    }

    /// <summary>
    /// 强制重新生成ID（慎用）
    /// </summary>
    public void RegenerateId()
    {
        string oldId = buildingId;
        buildingId = System.Guid.NewGuid().ToString();
        if (showDebugInfo)
            Debug.Log($"[Building] 建筑 {data?.buildingName} ID从 {oldId} 重新生成为 {buildingId}");
    }

    #endregion

    #region 建筑生命周期

    /// <summary>
    /// Try Snap 会先给其赋值 positions, 然后调用它，
    /// 如果回传是 true 登记成功
    /// 如果回传是 false 它会把内容 Deactive 保存到未使用空间
    /// </summary>
    /// <returns></returns>
    public virtual bool OnTryBuilt()
    {
        Debug.Log($"[Building] OnTryBuilt called for building {data?.buildingName} (ID: {BuildingId})");
        
        if (BuildingManager.Instance.BuildingBuilt(this))
        {
            Debug.Log($"[Building] BuildingBuilt returned true for {data?.buildingName}");
            return true;
        }
        
        Debug.Log($"[Building] BuildingBuilt returned false, calling RegisterBuilding for {data?.buildingName}");
        BuildingManager.Instance.RegisterBuilding(this);
        status = BuildingStatus.Inactive;
        return false;
    }

    /// <summary>
    /// 建筑升级回调
    /// </summary>
    public abstract void OnUpgraded();

    /// <summary>
    /// 销毁建筑
    /// </summary>
    [Button("销毁建筑")]
    public virtual bool DestroySelf()
    {
        // 注意：不需要在这里手动调用UnregisterBuilding，因为OnDestroy会调用OnDestroyed来处理
        var parentPlaceable = GetComponentInParent<PlaceableObject>();
        if (parentPlaceable != null)
        {
            Destroy(parentPlaceable.gameObject);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 建筑被销毁时的回调
    /// </summary>
    public virtual void OnDestroyed()
    {
        // 从BuildingManager中移除
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterBuilding(this);
        }

        // 确保网格占用被释放
        if (PlaceableObject != null && PlaceableObject.IsPlaced)
        {
            PlaceableObject.RemoveFromGrid();
        }

        if (showDebugInfo)
            Debug.Log($"[Building] 建筑 {data?.buildingName} (ID: {BuildingId}) 已被销毁");
    }

    #endregion

    #region 位置管理

    /// <summary>
    /// 同步位置信息 - 从PlaceableObject获取当前位置
    /// </summary>
    public void SyncPositionsFromPlaceable()
    {
        if (PlaceableObject != null)
        {
            var currentPositions = PlaceableObject.GetOccupiedPositions();
            if (currentPositions != null && currentPositions.Length > 0)
            {
                // 转换Vector3Int数组为Vector2Int列表
                positions = new List<Vector2Int>();
                foreach (var pos in currentPositions)
                {
                    positions.Add(new Vector2Int(pos.x, pos.z));
                }
                
                if (showDebugInfo)
                    Debug.Log($"[Building] {data?.buildingName} (ID: {BuildingId}) 位置已同步: {string.Join(", ", positions)}");
            }
            else
            {
                // 如果PlaceableObject没有位置信息，清空positions
                positions = new List<Vector2Int>();
                if (showDebugInfo)
                    Debug.Log($"[Building] {data?.buildingName} (ID: {BuildingId}) 位置已清空");
            }
        }
        else
        {
            // 如果没有PlaceableObject，初始化空列表
            if (positions == null)
                positions = new List<Vector2Int>();
        }
    }
    
    /// <summary>
    /// 强制刷新位置信息
    /// </summary>
    [Button("刷新位置信息")]
    public void RefreshPositions()
    {
        SyncPositionsFromPlaceable();
    }

    #endregion

    #region NPC管理
    #region NPC查询
    public bool IsNPCAssigned(NPC npc){
        return assignedNPCs.Contains(npc);
    }
    /// <summary>
    /// 检查NPC是否正在这个建筑中工作
    /// </summary>
    public bool IsNPCActiveWorking(NPC npc){
        return IsNPCAssigned(npc)
        && npc.currentState == NPCState.Working 
        && npc.assignedTask.building == this;
    }
    public bool IsNPCLocked(NPC npc){
        if(npc == null){
            Debug.LogError($"[Building] NPC为空");
            return false;
        }

        // 检查NPC是否在assignedNPCs中且同时在lockedNPCs中
        if(assignedNPCs.Contains(npc)){
            return npc.IsLocked;
        }
        Debug.LogWarning($"[Building] NPC {npc.data.npcName} 没有被分配到建筑 {data.buildingName} 中");
        return false;
    }
    #endregion
    public void LockNPC(NPC npc){
        if(npc == null){
            Debug.LogError($"[Building] NPC为空");
            return;
        }
        if(IsNPCAssigned(npc)){
            npc.LockWork(this);
            return;
        }else{
            Debug.LogWarning($"[Building] 无法锁定NPC {npc.data.npcName} ，没有被分配到建筑 {data.buildingName} 中");
        }


    }
    public void UnlockNPC(NPC npc){
        if(npc == null){
            Debug.LogError($"[Building] NPC为空");
            return;
        }
        if(IsNPCAssigned(npc)){
            // 如果NPC当前未在建筑中工作，则将其从assignedNPCs中一并移除
            if(!IsNPCActiveWorking(npc)){
                assignedNPCs.Remove(npc);
            }
            npc.UnlockWork(this);
            return;
        }else{
            Debug.LogWarning($"[Building] 无法解锁NPC {npc.data.npcName} ，没有被分配到建筑 {data.buildingName} 中");
        }
    }
    /// <summary>
    /// 尝试分配NPC到建筑
    /// </summary>
    public virtual bool TryAssignNPC(NPC npc)
    {
        // 如果NPC不在已有槽位中且目标就是这个建筑
        if (!assignedNPCs.Contains(npc) 
            && npc.assignedTask.building == this
            && npc.assignedTask.taskType == TaskType.Production
            && assignedNPCs.Count < NPCSlotAmount) // 由于lockedNPCs是assignedNPCs的子集，所以只需要检查assignedNPCs.Count
        {
            assignedNPCs.Add(npc);
            return true;
        }
        if (!tempAssignedNPCs.Contains(npc)
            && npc.assignedTask.building == this
            && (npc.assignedTask.taskType == TaskType.HandlingAccept
            || npc.assignedTask.taskType == TaskType.HandlingDrop)
            && tempAssignedNPCs.Count < NPCTempSlotAmount)
        {
            tempAssignedNPCs.Add(npc);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 尝试移除NPC
    /// </summary>
    public virtual void TryRemoveNPC(NPC npc)
    {
        // 如果NPC被锁定，则不移除
        if(IsNPCAssigned(npc) && IsNPCLocked(npc)){
            Debug.LogWarning($"[Building] NPC {npc.data.npcName} 被锁定，不能移除");
            return;
        }
        ForceRemoveNPC(npc);
        // npc.AssignedBuilding = null;
        // npc.ChangeState(NPCState.Idle);
    }
    /// <summary>
    /// 强制移除NPC
    /// </summary>
    public void ForceRemoveNPC(NPC npc){
        assignedNPCs.Remove(npc);
        tempAssignedNPCs.Remove(npc);
    }
    /// <summary>
    /// 获取分配NPC中正在这个建筑工作的NPC
    /// </summary>
    public List<NPC> GetActiveAssignedNPCs(){
        List<NPC> result = new List<NPC>();
        foreach(var npc in assignedNPCs){
            if(IsNPCActiveWorking(npc)){
                result.Add(npc);
            }
        }
        return result;
    }
    /// <summary>
    /// 获取分配NPC中正在这个建筑工作的NPC数量
    /// </summary>
    [Button("打印活跃NPC数量")]
    public int ActiveAssignedNPCsCount(){
        int result = GetActiveAssignedNPCs().Count;
        Debug.Log($"[Building] 建筑 {data.buildingName} 当前有 {result} 个活跃NPC正在实际工作");
        return result;
    }


    #endregion

    #region 装备管理

    /// <summary>
    /// 安装装备
    /// </summary>
    public virtual void InstallEquipment(Equipment equipment) { }

    /// <summary>
    /// 移除装备
    /// </summary>
    public virtual void RemoveEquipment(Equipment equipment) { }

    #endregion

    #region 升级系统

    /// <summary>
    /// 检查是否可以升级
    /// </summary>
    public virtual bool CanUpgrade() { return false; }

    /// <summary>
    /// 执行升级
    /// </summary>
    public virtual bool Upgrade() { return false; }

    /// <summary>
    /// 获取升级价格
    /// </summary>
    public virtual int GetUpgradePrice() { return 0; }

    #endregion

    #region 选中系统
    private bool _canBeSelected = false;
    public bool CanBeSelected {
        get{
            return _canBeSelected;
        }
        set{
            _canBeSelected = value;
        }
    }
    private Outline _outline;
    public Outline Outline {
        get{
            if(_outline == null){
                _outline = GetComponent<Outline>();
            }
            return _outline;
        }
        set{
            _outline = value;
        }
    }
    public void OnSelect(){
        Debug.Log($"[Building] 建筑{data.buildingName}被选中");
        GameEvents.TriggerBuildingSelected(this);
        // 高亮建筑
        HighlightSelf();
    }
    public void OnDeselect(){
        Debug.Log($"[Building] 建筑{data.buildingName}被取消选中");
        UnhighlightSelf();
    }
    public void HighlightSelf(){
        // 没有的话加一个
        if(Outline == null){
            Outline = gameObject.AddComponent<Outline>();
            Outline.OutlineColor = Color.white;
            Outline.OutlineWidth = 2f;
            Outline.OutlineMode = Outline.Mode.OutlineVisible;

        }

        Outline.enabled = true;
    }
    public void UnhighlightSelf(){
        if(Outline != null){
            Outline.enabled = false;
        }
    }

    #endregion

    #region Buff系统
    private Dictionary<BuffEnums, int> _buffs;
    public Dictionary<BuffEnums, int> Buffs{
        get{
            if(_buffs == null){
                _buffs = new Dictionary<BuffEnums, int>();
            }
            return _buffs;
        }
        set{
            _buffs = value;
        }
    }
    public void OnBuffAffected(Buff buff){
        Buffs[buff.type] = buff.intensity;
    }
    public void OnBuffAffectedEnd(Buff buff){
        Buffs.Remove(buff.type);
    }
    /// <summary>
    /// 每个有活跃NPC的槽位提供正向Buff, 但不存储在Buffs中
    /// </summary>
    public Buff GetNPCSlotBuff(){
        BuffEnums type = BuffEnums.NPCWorkingInSlot;
        float intensity = ActiveAssignedNPCsCount() * 0.1f;
        return new Buff(type, intensity);
    }
    /// <summary>
    /// 狗策划老马要求，建筑的NPC槽位不超过两个，所以这里只考虑两个以下NPC的情况
    /// </summary>
    /// <returns></returns>
    public Buff GetFriendWorkTogetherBuff(){
        BuffEnums type = BuffEnums.FriendWorkTogether;
        // 仅有2个NPC在槽位时，有可能提供正向Buff
        if(NPCSlotAmount < 2){
            return new Buff(type, 0);
        }
        var activeNPCs = GetActiveAssignedNPCs();
        if(activeNPCs.Count != 2){
            return new Buff(type, 0);
        }
        // 从NPCManager中的SocialSystem中获取NPC之间的关系
        NPC npc1 = activeNPCs[0];
        NPC npc2 = activeNPCs[1];
        int relationship = NPCManager.Instance.socialSystem.GetRelationship(npc1, npc2);
        int intensity = 0;
        // 关系50-100时，加成10%，100时加成25%,50以下没有加成
        if(relationship >= 50 && relationship < 100){
            intensity = 10;
        }
        else if(relationship >= 100){
            intensity = 25;
        }
        return new Buff(type, intensity);
    }
    /// <summary>
    /// 获取所有在槽位中的活跃的NPC自身能力的总加成
    /// </summary>
    /// <returns></returns>
    public Buff GetInSlotNPCBuff(){
        BuffEnums type = BuffEnums.NPCEfficiency;
        float intensity = 0;
        foreach(var npc in GetActiveAssignedNPCs()){
            intensity += npc.GetWorkEfficiency();
        }
        return new Buff(type, intensity);
    }

    #endregion

    #region 存档系统

    /// <summary>
    /// 获取存档数据
    /// </summary>
    public virtual GameSaveData GetSaveData()
    {
        InventorySaveData inventorySaveData = inventory.GetSaveData() as InventorySaveData;
        return new BuildingInstanceSaveData()
        {
            buildingId = BuildingId,
            subType = data.subType,
            status = status,
            currentLevel = currentLevel,
            positions = positions,
            acceptResources = GetAcceptResourcesSaveData(),
            inventory = inventorySaveData,
            // TODO: 实现安装的设备保存
            // installedEquipment = GetEquipmentsSaveData(),
        };
    }

    /// <summary>
    /// 从数据加载
    /// </summary>
    public virtual void LoadFromData(GameSaveData data)
    {
        var saveData = data as BuildingInstanceSaveData;
        // 1. 先设置建筑数据
        SetBuildingData(BuildingManager.Instance.GetBuildingOverallData(saveData.subType));
        
        // 2. 再恢复存档数据
        BuildingId = saveData.buildingId;
        status = saveData.status;
        currentLevel = saveData.currentLevel;
        positions = saveData.positions;
        AcceptResources = AcceptResourcesFromSaveData(saveData.acceptResources);
        inventory.LoadFromData(saveData.inventory);
        // TODO: 实现安装的设备加载
        // installedEquipment = GetEquipmentsFromData(saveData.installedEquipment);
    }
    public virtual List<EquipmentSaveData> GetEquipmentsSaveData()
    {
        // 用linq 把installedEquipment 转换为 EquipmentSaveData
        return installedEquipment.Select(e => e.GetSaveData() as EquipmentSaveData).ToList();
    }
    public virtual List<Equipment> GetEquipmentsFromData(List<EquipmentSaveData> saveData)
    {
        // TODO: 实现GetEquipmentsFromData
        // 使用Equipment提供的静态方法CreateEquipmentFromData
        return saveData.Select(e => Equipment.CreateEquipmentFromData(e)).ToList();
    }
    public List<ResourceStackSaveData> GetAcceptResourcesSaveData(){
        return AcceptResources.Select(r => new ResourceStackSaveData(){
            type = r.type,
            subType = r.subType,
        }).ToList();
    }
    public HashSet<ResourceConfig> AcceptResourcesFromSaveData(List<ResourceStackSaveData> saveData){
        if(saveData == null){
            return new HashSet<ResourceConfig>();
        }
        var acceptResources = new HashSet<ResourceConfig>(saveData.Select(r => ResourceManager.Instance.GetResourceConfig(r.type, r.subType)));
        return acceptResources;
    }
    #endregion

    #region 调试和工具方法

    public override string ToString()
    {
        return $"[{data.buildingName} - {data.buildingType}/{data.subType}] ID: {BuildingId} Pos: {string.Join(" ", positions)}";
    }

    [ContextMenu("Print Building Info")]
    public void PrintBuildingInfo()
    {
        Debug.Log($"[Building] {data.buildingName} 信息: ==========================");
        Debug.Log($"ID: {BuildingId}");
        Debug.Log($"类型: {data.buildingType}/{data.subType}");
        Debug.Log($"状态: {status}");
        Debug.Log($"等级: {currentLevel}");
        Debug.Log($"位置: {string.Join(" ", positions)}");
        Debug.Log($"分配NPC数量: {assignedNPCs?.Count ?? 0}/{NPCSlotAmount}");
        Debug.Log($"临时NPC数量: {tempAssignedNPCs?.Count ?? 0}");
        Debug.Log($"锁定NPC数量: {assignedNPCs?.Count(npc => IsNPCLocked(npc)) ?? 0}");
        Debug.Log($"[Building] ====================================================");
    }

    #endregion

    #region 编辑器调试

#if UNITY_EDITOR
    // 在对象上方显示分配NPC和临时NPC数量，以及背包资源数量
    private void DrawDebugTextWithHandles()
    {
        if (assignedNPCs == null || tempAssignedNPCs == null) return;
        
        // 构建显示文本
        List<string> displayLines = new List<string>();
        
        // NPC信息 - 显示分配数量、锁定数量和临时数量
        int lockedCount = assignedNPCs?.Count(npc => IsNPCLocked(npc)) ?? 0;
        displayLines.Add($"[A:{assignedNPCs.Count}/{NPCSlotAmount}][L:{lockedCount}][T:{tempAssignedNPCs.Count}]");
        
        // Inventory资源信息
        if (inventory != null && inventory.currentStacks != null)
        {
            foreach (var resourceStack in inventory.currentStacks)
            {
                if (resourceStack.amount > 0) // 只显示有资源的项目
                {
                    displayLines.Add($"[{resourceStack.displayName}] {resourceStack.amount}/{resourceStack.storageLimit}");
                }
            }
        }
        
        // 计算文本位置（在对象上方）
        Vector3 textPosition = transform.position + Vector3.up * heightOffset;
        
        // 设置文本样式
        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.fontSize = (int)textSize;
        style.alignment = TextAnchor.UpperCenter;
        
        // 绘制多行文本
        string displayText = string.Join("\n", displayLines);
        Handles.Label(textPosition, displayText, style);
    }
#endif

    #endregion
}