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
        if (BuildingManager.Instance.BuildingBuilt(this))
            return true;
        BuildingManager.Instance.RegisterBuilding(this);
        status = BuildingStatus.Inactive;
        return false;
    }
    // 抽象方法
    public abstract void OnUpgraded();
    public abstract void OnDestroyed();
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
    
    // 接口实现
    public virtual GameSaveData GetSaveData() { return null; }
    public virtual bool LoadFromData(GameSaveData data) { return false; }
    
    // 游戏循环
    public virtual void Start() {
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
        return $"[{data.buildingName} - {data.buildingType}/{data.subType}] Pos: {string.Join(" ", positions)}";
    }
    
}