using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPC : MonoBehaviour, ISaveable
{
    [Header("基本信息")]
    public NPCData data;
    public NPCState currentState;
    public Building assignedBuilding;
    
    [Header("社交系统")]
    public Dictionary<NPC, int> relationships; // 好感度系统
    
    [Header("状态管理")]
    private NPCState previousState;
    private float stateTimer;

    [Header("任务和背包")]
    public NPCInventory inventory;           // 背包
    public Transform currentTarget;          // 当前目标位置
    public NavMeshAgent navAgent;           // 导航组件
    
    // 状态管理
    public void ChangeState(NPCState newState) { 
        if (currentState == newState) return;
        previousState = currentState;
        currentState = newState;
        stateTimer = 0f;
    }

    #region 工作相关
    public bool CanWorkNow() 
    {
        if(TimeManager.Instance == null) return false;
        return TimeManager.Instance.CurrentTime.hour >= data.workTimeStart && TimeManager.Instance.CurrentTime.hour <= data.workTimeEnd;
    }
    public bool ShouldRest() 
    {
        if(TimeManager.Instance == null) return false;
        return TimeManager.Instance.CurrentTime.hour >= data.restTimeStart && TimeManager.Instance.CurrentTime.hour <= data.restTimeEnd;
    }
    
    public float GetWorkEfficiency() 
    {
        // 不在工作时间工作效率为0
        if (!CanWorkNow())
        return 0f;

        // 基础工作能力
        float efficiency = data.baseWorkAbility / 100.0f;
        
        // 1. 特殊词条加成
        efficiency *= GetTraitEfficiencyMultiplier();
        
        // TODO: 2. 社交关系加成（与同事的好感度）
        // efficiency *= GetRelationshipBonus();
        
        return efficiency;
    }
    
    // 词条加成
    private float GetTraitEfficiencyMultiplier()
    {
        float multiplier = 1.0f;
        
        // 这里写NPC的词条对于工作效率的加成逻辑
        foreach (var trait in data.traits)
        {
            switch (trait)
            {
                case NPCTraitType.FarmExpert:
                    if (assignedBuilding?.data.subType == BuildingSubType.Farm)
                        multiplier *= 1.5f; // 50%加成
                    break;
                case NPCTraitType.LivestockExpert:
                    if (assignedBuilding?.data.subType == BuildingSubType.Ranch)
                        multiplier *= 1.5f;
                    break;
                case NPCTraitType.CheapLabor:
                    multiplier *= 0.8f; // 工作效率降低20%
                    break;
            }
        }
        
        return multiplier;
    }
    
    #endregion
    
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

    public void AssignTask() {}
    public bool CanCarryResource(ResourceType type, int amount) { return false; }
    public void MoveToTarget(Transform target) {}
    public void CollectResource() {}
    public void DeliverResource() {}
}