using UnityEngine;

[System.Serializable]
public class PersonalitySystem
{
    // 检查性格兼容性
    public static bool ArePersonalitiesCompatible(PersonalityType type1, PersonalityType type2)
    {
        // 相反性格增加争吵概率
        return false;
    }
    
    // 获取性格冲突修正值
    public static float GetPersonalityConflictModifier(PersonalityType type1, PersonalityType type2)
    {
        return 0f;
    }
    
    // 获取词条效果
    public static float GetTraitEffect(PersonalityTrait trait, string effectType)
    {
        return 0f;
    }
    
    // 应用词条到NPC属性
    public static void ApplyTraitEffects(NPCInstance npc)
    {
        // 根据词条修改NPC属性
    }
    
    // 检查词条是否影响建筑效率
    public static float GetTraitBuildingBonus(PersonalityTrait trait, BuildingSubType buildingType)
    {
        return 0f;
    }
    
    // 获取工资修正（工贼词条）
    public static float GetWageModifier(PersonalityTrait[] traits)
    {
        return 1f;
    }
    
    // 获取维护成本减免（保养大师词条）
    public static float GetMaintenanceReduction(PersonalityTrait[] traits)
    {
        return 0f;
    }
}