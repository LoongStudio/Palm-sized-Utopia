using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "PalmUtopia/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("基本属性")]
    public string npcName;
    public Sprite avatar;
    public int dailyWage;
    
    [Header("时间设置")]
    public int workStartHour = 8;
    public int workEndHour = 18;
    public int restStartHour = 22;
    public int restEndHour = 6;
    
    [Header("技能属性")]
    public float workAbility = 1f;      // 工作能力基础加成
    public float plantingSkill = 1f;    // 种植技能
    public float breedingSkill = 1f;    // 养殖技能
    public float operationSkill = 1f;   // 运营技能
    
    [Header("性格设置")]
    public PersonalityType personality;
    public PersonalityTrait[] traits;
    
    // 获取技能加成
    public float GetSkillBonus(BuildingSubType buildingType) { return 0f; }
    
    // 检查是否有特定词条
    public bool HasTrait(PersonalityTrait trait) { return false; }
    
    // 获取词条效果
    public float GetTraitEffect(PersonalityTrait trait) { return 0f; }
}

[System.Serializable]
public class NPCInstance
{
    public int npcId;
    public NPCData data;
    public NPCState currentState;
    public NPCState targetState;
    
    [Header("当前分配")]
    public int assignedBuildingId = -1;
    public Vector3 restLocation;
    public Vector3 currentPosition;
    
    [Header("社交关系")]
    public Dictionary<int, float> relationships; // NPC ID -> 好感度
    
    [Header("状态计时")]
    public float stateTimer;
    public float lastWagePaid;
    public bool wageRefused; // 拒绝工作（工资不足）
    
    [Header("社交状态")]
    public bool isSocializing;
    public int socializingWithNPC = -1;
    public float socializingTimer;
    
    // 更新NPC状态
    public void UpdateState(float deltaTime) { }
    
    // 切换状态
    public void ChangeState(NPCState newState) { }
    
    // 移动到目标位置
    public void MoveToPosition(Vector3 targetPos) { }
    
    // 开始社交
    public void StartSocializing(int otherNPCId) { }
    
    // 结束社交
    public void EndSocializing() { }
    
    // 获取工作效率
    public float GetWorkEfficiency(BuildingSubType buildingType) { return 0f; }
    
    // 获取与其他NPC的好感度
    public float GetRelationship(int otherNPCId) { return 0f; }
    
    // 设置与其他NPC的好感度
    public void SetRelationship(int otherNPCId, float value) { }
    
    // 修改好感度
    public void ModifyRelationship(int otherNPCId, float delta) { }
    
    // 检查是否在工作时间
    public bool IsWorkTime() { return false; }
    
    // 检查是否在休息时间
    public bool IsRestTime() { return false; }
}
