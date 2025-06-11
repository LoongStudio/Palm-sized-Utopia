using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

[RequireComponent(typeof(Collider))]
public abstract class Building : MonoBehaviour, IUpgradeable, ISaveable
{
    [Header("基本属性")]
    public BuildingData data;
    public BuildingStatus status;
    public int currentLevel;
    public List<Vector2Int> positions;
    public List<SubResource> AcceptResources;
    [Header("槽位管理")] 
    public int maxSlotAmount = 3;
    public List<NPC> assignedNPCs;
    public List<NPC> tempAssignedNPCs;
    public List<Equipment> installedEquipment;
    public Inventory inventory;           // 背包
    
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
        BuildingManager.Instance.RegistBuilding(this);
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
    public virtual void TryAssignNPC(NPC npc)
    {
        // 如果NPC不在已有槽位中且目标就是这个建筑
        if (!assignedNPCs.Contains(npc) 
            && npc.assignedBuilding == this
            && (npc.currentState == NPCState.Working 
                || npc.currentState == NPCState.Transporting))
        {
            assignedNPCs.Add(npc);
        }
        if (!tempAssignedNPCs.Contains(npc)
            && npc.assignedBuilding == this
            && npc.currentState == NPCState.Transporting)
        {
            tempAssignedNPCs.Add(npc);
            npc.StartDelivering(this);
        }
    }
    public virtual void TryRemoveNPC(NPC npc)
    {
        assignedNPCs.Remove(npc);
        npc.assignedBuilding = null;
    }
    public virtual void InstallEquipment(Equipment equipment) { }
    public virtual void RemoveEquipment(Equipment equipment) { }
    
    // 接口实现
    public virtual SaveData SaveToData() { return null; }
    public virtual void LoadFromData(SaveData data) { }
    
    // 游戏循环
    public virtual void Start() {
        // LoadFromData(); // TODO: 开始时 如果数据并没有正常加载，尝试重新从Data中读取
        if (!OnTryBuilt())
        {
            Debug.LogError($"[Building] 建筑 {this.ToString()} 放置失败, 游戏开始时所有建筑应正常被放置成功");    
        }
        InitialSelfStorage();
    }

    public override string ToString()
    {
        return $"[{data.buildingName} - {data.buildingType}/{data.subType}] Pos: {string.Join(" ", positions)}";
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"进来一个畜生 {other.name}");
        assignedNPCs ??= new List<NPC>();
        // 如果遇到的是NPC且槽位有空余
        if (other.CompareTag("NPC") && assignedNPCs.Count < maxSlotAmount)
        {
            // 一般来说必定存在
            if (other.transform.TryGetComponent<NPC>(out NPC npc))
            {
                TryAssignNPC(npc);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            if (other.transform.TryGetComponent<NPC>(out NPC npc))
            {
                TryRemoveNPC(npc);
            }
        }
    }
}