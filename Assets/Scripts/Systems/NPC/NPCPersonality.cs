using UnityEngine;

[System.Serializable]
public class NPCPersonality
{
    [Header("工作属性")]
    [Range(0.5f, 2f)] public float workEfficiency = 1f;        // 工作效率
    [Range(0.5f, 2f)] public float learningSpeed = 1f;         // 学习速度
    [Range(0.1f, 0.5f)] public float errorRate = 0.1f;         // 错误率
    
    [Header("社交属性")]
    [Range(0.5f, 2f)] public float favorabilityGainRate = 1f;  // 好感度增长率
    [Range(0.5f, 1.5f)] public float teamworkBonus = 1f;       // 团队合作加成
    
    [Header("个性标签")]
    public PersonalityTrait[] traits;
    
    public NPCPersonality()
    {
        // 随机生成个性
        GenerateRandomPersonality();
    }
    
    public void GenerateRandomPersonality()
    {
        workEfficiency = Random.Range(0.8f, 1.5f);
        learningSpeed = Random.Range(0.8f, 1.3f);
        errorRate = Random.Range(0.05f, 0.2f);
        favorabilityGainRate = Random.Range(0.8f, 1.4f);
        teamworkBonus = Random.Range(0.9f, 1.2f);
        
        // 随机选择1-3个特质
        int traitCount = Random.Range(1, 4);
        traits = new PersonalityTrait[traitCount];
        
        PersonalityTrait[] allTraits = System.Enum.GetValues(typeof(PersonalityTrait)) as PersonalityTrait[];
        
        for (int i = 0; i < traitCount; i++)
        {
            PersonalityTrait trait;
            do
            {
                trait = allTraits[Random.Range(0, allTraits.Length)];
            } 
            while (System.Array.Exists(traits, t => t == trait));
            
            traits[i] = trait;
            ApplyTraitEffects(trait);
        }
    }
    
    private void ApplyTraitEffects(PersonalityTrait trait)
    {
        switch (trait)
        {
            case PersonalityTrait.Diligent:
                workEfficiency *= 1.15f;
                break;
            case PersonalityTrait.Curious:
                learningSpeed *= 1.3f;
                break;
            case PersonalityTrait.Procrastinator:
                workEfficiency *= 0.9f;
                errorRate *= 0.5f; // 慢但准确
                break;
            case PersonalityTrait.SocialButterfly:
                favorabilityGainRate *= 1.25f;
                teamworkBonus *= 1.1f;
                break;
            case PersonalityTrait.Perfectionist:
                errorRate *= 0.3f;
                workEfficiency *= 0.85f; // 慢但精确
                break;
        }
    }
    
    public string GetPersonalityDescription()
    {
        string desc = "个性特征: ";
        foreach (var trait in traits)
        {
            desc += GetTraitName(trait) + " ";
        }
        return desc;
    }
    
    private string GetTraitName(PersonalityTrait trait)
    {
        switch (trait)
        {
            case PersonalityTrait.Diligent: return "勤劳";
            case PersonalityTrait.Curious: return "好奇";
            case PersonalityTrait.Procrastinator: return "拖延";
            case PersonalityTrait.SocialButterfly: return "社交达人";
            case PersonalityTrait.Perfectionist: return "完美主义";
            default: return trait.ToString();
        }
    }
}

public enum PersonalityTrait
{
    Diligent,           // 勤劳
    Curious,            // 好奇
    Procrastinator,     // 拖延
    SocialButterfly,    // 社交达人
    Perfectionist       // 完美主义
}