using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
public abstract class Building : MonoBehaviour, IUpgradeable, ISaveable
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    [Header("唯一标识")]
    [SerializeField] private string buildingId;

    [Header("基本属性")]
    public BuildingData data;
    public BuildingStatus status;
    public int currentLevel;
    public List<Vector2Int> positions;
    public HashSet<ResourceConfig> AcceptResources;
    [Header("槽位管理")] 
    public int maxSlotAmount = 3;
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
    private void OnDrawGizmos()
    {
        if (!alwaysShow) return;
#if UNITY_EDITOR
        DrawDebugTextWithHandles();
#endif
    }

#if UNITY_EDITOR
    private void DrawDebugTextWithHandles()
    {
        if (assignedNPCs == null || tempAssignedNPCs == null) return;
        // 计算文本位置（在对象上方）
        Vector3 textPosition = transform.position + Vector3.up * heightOffset;
        // 设置文本样式
        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.fontSize = (int)textSize;
        style.alignment = TextAnchor.UpperCenter;
        string displayText = $"[A:{assignedNPCs.Count}/{maxSlotAmount}][T:{tempAssignedNPCs.Count}]";
        Handles.Label(textPosition, displayText, style);
    }
#endif
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
    // 抽象方法
    public abstract void OnUpgraded();
    public virtual bool DestroySelf()
    {
        var parentPlaceable = GetComponentInParent<PlaceableObject>();
        if (parentPlaceable != null)
        {
            Destroy(parentPlaceable.gameObject);
            return true;
        }
        return false;
    }
    public virtual void OnDestroyed()
    {
        // 从BuildingManager中移除
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.UnregisterBuilding(this);
        }

        if (showDebugInfo)
            Debug.Log($"[Building] 建筑 {data?.buildingName} (ID: {BuildingId}) 已被销毁");
    }
    // 通用方法
    public abstract void InitialSelfStorage();
    public virtual bool CanUpgrade() { return false; }
    public virtual bool Upgrade() { return false; }
    public virtual int GetUpgradePrice() { return 0; }
    public virtual bool TryAssignNPC(NPC npc)
    {
        // 如果NPC不在已有槽位中且目标就是这个建筑
        if (!assignedNPCs.Contains(npc) 
            && npc.assignedTask.building == this
            && npc.assignedTask.taskType == TaskType.Production
            && assignedNPCs.Count < maxSlotAmount)
        {
            assignedNPCs.Add(npc);
            return true;
        }
        if (!tempAssignedNPCs.Contains(npc)
            && npc.assignedTask.building == this
            && (npc.assignedTask.taskType == TaskType.HandlingAccept
            || npc.assignedTask.taskType == TaskType.HandlingDrop))
        {
            tempAssignedNPCs.Add(npc);
            return true;
        }
        return false;
    }
    public virtual void TryRemoveNPC(NPC npc)
    {
        assignedNPCs.Remove(npc);
        tempAssignedNPCs.Remove(npc);
        // npc.AssignedBuilding = null;
        // npc.ChangeState(NPCState.Idle);
    }
    public virtual void InstallEquipment(Equipment equipment) { }
    public virtual void RemoveEquipment(Equipment equipment) { }

    #region 保存与加载
    public virtual GameSaveData GetSaveData()
    {
        InventorySaveData inventorySaveData = inventory.GetSaveData() as InventorySaveData;
        return new BuildingInstanceSaveData(){
            buildingId = BuildingId,
            subType = data.subType,
            status = status,
            currentLevel = currentLevel,
            positions = positions,
            inventory = inventorySaveData,
            // TODO: 添加诸如installedEquipment等数据
        };
    }
    public virtual void LoadFromData(GameSaveData data) { }
    #endregion
    // 游戏循环
    public virtual void Start() {
        var _ = BuildingId; // 触发getter，如果没有ID会自动生成
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

    public override string ToString()
    {
        return $"[{data.buildingName} - {data.buildingType}/{data.subType}] ID: {BuildingId} Pos: {string.Join(" ", positions)}";
    }

    #region 调试
    [ContextMenu("Print Building Info")]
    public void PrintBuildingInfo()
    {
        Debug.Log($"[Building] {data.buildingName} 信息: ==========================");
        Debug.Log($"ID: {BuildingId}");
        Debug.Log($"类型: {data.buildingType}/{data.subType}");
        Debug.Log($"状态: {status}");
        Debug.Log($"等级: {currentLevel}");
        Debug.Log($"位置: {string.Join(" ", positions)}");
        Debug.Log($"分配NPC数量: {assignedNPCs?.Count ?? 0}/{maxSlotAmount}");
        Debug.Log($"临时NPC数量: {tempAssignedNPCs?.Count ?? 0}");
        Debug.Log($"[Building] ====================================================");
    }
    #endregion
    
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

    /// <summary>
    /// 从存档数据创建建筑实例
    /// </summary>
    /// <param name="buildingInstanceData">建筑实例存档数据</param>
    /// <returns>建筑实例</returns>
    public static Building CreateBuildingFromData(BuildingInstanceSaveData buildingInstanceData)
    {
        // 这里需要根据buildingInstanceData.subType创建对应的建筑类型
        // 由于Building是抽象类，具体的创建逻辑应该在子类中实现
        // 或者通过工厂模式来实现
        Debug.LogWarning("[Building] CreateBuildingFromData方法需要在具体实现中重写");
        return null;
    }
    public void SetBuildingData(BuildingData data)
    {
        this.data = data;
    }
}