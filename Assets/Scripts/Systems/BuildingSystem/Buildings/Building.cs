using UnityEngine;
using System.Collections.Generic;

public abstract class Building : MonoBehaviour, IUpgradeable, ISaveable
{
    [Header("基本属性")]
    public BuildingData data;
    public BuildingStatus status;
    public int currentLevel;
    public Vector2Int position;
    
    [Header("槽位管理")]
    public List<NPC> assignedNPCs;
    public List<Equipment> installedEquipment;
    
    // 抽象方法
    public abstract void OnBuilt();
    public abstract void OnUpgraded();
    public abstract void OnDestroyed();
    public abstract float GetCurrentEfficiency();
    
    // 通用方法
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
}