using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

public abstract class Building : MonoBehaviour, IUpgradeable, ISaveable
{
    [Header("基本属性")]
    public BuildingData data;
    public BuildingStatus status;
    public int currentLevel;
    public List<Vector2Int> positions;
    public List<Enum> AcceptResources;
    [Header("槽位管理")]
    public List<NPC> assignedNPCs;
    public List<Equipment> installedEquipment;
    public List<SubResourceValue<int>> currentSubResource;
    public List<SubResourceValue<int>> maximumSubResource;
    
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
    public virtual void InitialSelfStorage()
    {
        AcceptResources = new List<Enum>();
        currentSubResource = new List<SubResourceValue<int>>();
        maximumSubResource = new List<SubResourceValue<int>>();
    }
    public virtual bool CanUpgrade() { return false; }
    public virtual bool Upgrade() { return false; }
    public virtual int GetUpgradePrice() { return 0; }
    public virtual void AssignNPC(NPC npc) { }
    public virtual void RemoveNPC(NPC npc) { }
    public virtual void InstallEquipment(Equipment equipment) { }
    public virtual void RemoveEquipment(Equipment equipment) { }
    
    // 接口实现
    public virtual SaveData SaveToData() { return null; }
    public virtual void LoadFromData(SaveData data) { }
    
    // 游戏循环
    public void Start() {
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
}