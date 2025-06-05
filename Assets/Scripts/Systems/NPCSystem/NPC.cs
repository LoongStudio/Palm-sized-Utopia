using UnityEngine;
using System.Collections.Generic;

public class NPC : MonoBehaviour, ISaveable
{
    [Header("基本信息")]
    public NPCData data;
    public NPCState currentState;
    public Building assignedBuilding;
    
    [Header("社交系统")]
    public Dictionary<NPC, int> relationships; // 好感度系统
    
    private NPCState previousState;
    private float stateTimer;
    
    // 状态管理
    public void ChangeState(NPCState newState) { }
    public bool CanWorkNow() { return false; }
    public bool ShouldRest() { return false; }
    
    // 工作相关
    public float GetWorkEfficiency() { return 0f; }
    public float GetBuildingBonus(BuildingType buildingType) { return 0f; }
    public void StartWorking() { }
    public void StopWorking() { }
    
    // 社交相关
    public void InteractWith(NPC other) { }
    public void IncreaseRelationship(NPC other, int amount) { }
    public void DecreaseRelationship(NPC other, int amount) { }
    public int GetRelationshipWith(NPC other) { return 0; }
    
    // 特殊能力
    public bool HasTrait(NPCTraitType trait) { return false; }
    public float GetTraitBonus(NPCTraitType trait) { return 0f; }
    
    private void Update()
    {
        UpdateState();
        UpdateMovement();
    }
    
    private void UpdateState() { }
    private void UpdateMovement() { }
    
    // 接口实现
    public SaveData SaveToData() { return null; }
    public void LoadFromData(SaveData data) { }
}