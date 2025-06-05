using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NPCManager : SingletonManager<NPCManager>
{
    [SerializeField] private NPCGenerationConfig defaultNPCGenerationConfig;
    private List<NPC> allNPCs;
    private List<NPC> availableNPCs;
    private SocialSystem socialSystem;
    
    // 事件
    public static event System.Action<NPC> OnNPCHired;
    public static event System.Action<NPC> OnNPCFired;
    public static event System.Action<NPC, NPCState> OnNPCStateChanged;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Initialize() 
    { 
        if (socialSystem == null)
        {
            socialSystem = new SocialSystem();
        }
        socialSystem.Initialize(allNPCs);
    }
    
    // NPC管理
    public bool HireNPC(NPCData npcData) { return false; }

    #region 私有NPCData生成方法
    private NPCData GenerateRandomNPCData(NPCGenerationConfig config = null) 
    {
        var activeConfig = config ?? defaultNPCGenerationConfig;

        if (activeConfig == null)
        {
            Debug.LogError("[NPCManager] 没有配置文件，无法生成NPC");
            return null;
        }

        NPCData npcData = new NPCData();

        // 生成基础属性
        npcData.npcName = GenerateRandomName(activeConfig);
        npcData.baseSalary = GenerateRandomValue(activeConfig.SalaryRange);
        npcData.baseWorkAbility = GenerateRandomValue(activeConfig.BaseWorkAbilityRange);
        
        // 生成时间属性 - 确保时间逻辑合理
        npcData.restTimeStart = GenerateRandomValue(activeConfig.RestStartHourRange);
        npcData.restTimeEnd = GenerateRandomValue(activeConfig.RestEndHourRange);
        npcData.workTimeStart = GenerateRandomValue(activeConfig.WorkStartHourRange);
        npcData.workTimeEnd = GenerateRandomValue(activeConfig.WorkEndHourRange);
        
        // 验证时间设置的合理性
        ValidateTimeSettings(npcData);
        
        // 生成性格
        npcData.personality = GenerateRandomPersonality(activeConfig);
        
        // 生成词条
        npcData.traits = GenerateRandomTraits(activeConfig);
        
        Debug.Log($"[NPCManager] 生成了新NPC: {npcData.npcName} - 性格:{npcData.personality} - 词条数量:{npcData.traits.Count}");
        return npcData;
    }

    private void ValidateTimeSettings(NPCData npcData)
    {
        // 确保休息时间不会与工作时间冲突
        // 如果休息开始时间早于结束时间，说明是跨夜休息
        bool isOvernightRest = npcData.restTimeStart > npcData.restTimeEnd;
        
        if (!isOvernightRest)
        {
            // 同一天的休息时间，确保工作时间不在休息时间内
            if (npcData.workTimeStart >= npcData.restTimeStart && npcData.workTimeStart <= npcData.restTimeEnd)
            {
                npcData.workTimeStart = npcData.restTimeEnd + 1;
            }
            if (npcData.workTimeEnd >= npcData.restTimeStart && npcData.workTimeEnd <= npcData.restTimeEnd)
            {
                npcData.workTimeEnd = npcData.restTimeStart - 1;
            }
        }
        
        // 确保工作开始时间早于结束时间
        if (npcData.workTimeStart >= npcData.workTimeEnd)
        {
            npcData.workTimeStart = Mathf.Max(9, npcData.workTimeEnd - 8); // 至少工作8小时
        }
    }
    private string GenerateRandomName(NPCGenerationConfig config)
    {
        if (config.FirstNames.Length == 0 || config.LastNames.Length == 0)
        {
            return $"NPC_{UnityEngine.Random.Range(1000, 9999)}";
        }
        
        string firstName = config.FirstNames[UnityEngine.Random.Range(0, config.FirstNames.Length)];
        string lastName = config.LastNames[UnityEngine.Random.Range(0, config.LastNames.Length)];
        
        return $"{lastName}{firstName}";
    }

    private int GenerateRandomValue(Vector2Int range, bool useGaussianDistribution = false)
    {
        if (useGaussianDistribution)
        {
            return GenerateGaussianValue(range);
        }
        else
        {
            return UnityEngine.Random.Range(range.x, range.y + 1);
        }
    }

    private int GenerateGaussianValue(Vector2Int range, float gaussianDeviation = 0.3f)
    {
        float mean = (range.x + range.y) / 2f;
        float stdDev = (range.y - range.x) * gaussianDeviation;
        
        float value = SampleGaussian(mean, stdDev);
        return Mathf.Clamp(Mathf.RoundToInt(value), range.x, range.y);
    }

    private float SampleGaussian(float mean, float stdDev)
    {
        // Box-Muller变换生成正态分布
        float u1 = 1.0f - UnityEngine.Random.value;
        float u2 = 1.0f - UnityEngine.Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
    
    private NPCPersonalityType GenerateRandomPersonality(NPCGenerationConfig config)
    {
        return WeightedRandomSelect(config.PersonalityWeights, w => w.personality, w => w.weight);
    }

    private List<NPCTraitType> GenerateRandomTraits(NPCGenerationConfig config)
    {
        int traitCount = GenerateRandomValue(config.TraitCountRange);
        List<NPCTraitType> selectedTraits = new List<NPCTraitType>();
        
        // 避免重复选择同一个词条
        List<TraitWeight> availableTraits = new List<TraitWeight>(config.TraitWeights);
        
        for (int i = 0; i < traitCount && availableTraits.Count > 0; i++)
        {
            NPCTraitType selectedTrait = WeightedRandomSelect(availableTraits.ToArray(), w => w.trait, w => w.weight);
            selectedTraits.Add(selectedTrait);
            
            // 移除已选择的词条，避免重复
            availableTraits.RemoveAll(t => t.trait == selectedTrait);
        }
        
        return selectedTraits;
    }
    
    private T WeightedRandomSelect<T, W>(W[] weights, System.Func<W, T> valueSelector, System.Func<W, float> weightSelector)
    {
        if (weights.Length == 0)
        {
            Debug.LogError("[NPCManager] WeightedRandomSelect: 权重数组为空");
            return default(T);
        }
        
        float totalWeight = 0f;
        foreach (var weight in weights)
        {
            totalWeight += weightSelector(weight);
        }
        
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("[NPCManager] WeightedRandomSelect: 总权重为0，返回第一个元素");
            return valueSelector(weights[0]);
        }
        
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var weight in weights)
        {
            currentWeight += weightSelector(weight);
            if (randomValue <= currentWeight)
            {
                return valueSelector(weight);
            }
        }
        
        // 如果没有选中任何项（不应该发生），返回第一个
        return valueSelector(weights[0]);
    }
    #endregion

    public bool FireNPC(NPC npc) { return false; }
    public bool AssignNPCToBuilding(NPC npc, Building building) { return false; }
    public void RemoveNPCFromBuilding(NPC npc) { }
    
    // 查询方法
    public List<NPC> GetAllNPCs() {
        return allNPCs;
    }
    public List<NPC> GetAvailableNPCs() {
        return availableNPCs;
    }
    public List<NPC> GetWorkingNPCs() {
        return allNPCs.Where(npc => npc.currentState == NPCState.Working).ToList();
    }
    
    // 工资系统
    public void PaySalaries() { }
    public int GetTotalSalaryCost() { return 0; }
    
    private void Update()
    {
        UpdateNPCStates();
        socialSystem.UpdateSocialInteractions();
    }
    
    private void UpdateNPCStates() { }
}