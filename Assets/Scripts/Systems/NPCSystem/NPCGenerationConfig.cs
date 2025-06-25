using UnityEngine;

[CreateAssetMenu(fileName = "NPCGenerationConfig", menuName = "Utopia/NPC Generation Config")]
public class NPCGenerationConfig : ScriptableObject
{
    [Header("基础属性范围")]
    [SerializeField] private Vector2Int salaryRange = new Vector2Int(1, 3);
    [SerializeField] private Vector2Int baseWorkAbilityRange = new Vector2Int(40, 100);
    [SerializeField] private Vector2Int itemCapacityRange = new Vector2Int(10, 50);
    [SerializeField] private Vector2Int itemTakeEachTimeCapacityRange = new Vector2Int(1, 5);
    
    [Header("时间设置")]
    [SerializeField] private Vector2Int restStartHourRange = new Vector2Int(21, 23);  // 休息开始时间范围
    [SerializeField] private Vector2Int restEndHourRange = new Vector2Int(6, 8);     // 休息结束时间范围
    [SerializeField] private Vector2Int workStartHourRange = new Vector2Int(9, 11);   // 工作开始时间范围
    [SerializeField] private Vector2Int workEndHourRange = new Vector2Int(17, 19);   // 工作结束时间范围
    
    [Header("性格和词条概率")]
    [SerializeField] private PersonalityWeight[] personalityWeights = new PersonalityWeight[]
    {
        new PersonalityWeight { personality = NPCPersonalityType.Diligent, weight = 0.15f },
        new PersonalityWeight { personality = NPCPersonalityType.Lazy, weight = 0.15f },
        new PersonalityWeight { personality = NPCPersonalityType.Honest, weight = 0.15f },
        new PersonalityWeight { personality = NPCPersonalityType.Hypocritical, weight = 0.15f },
        new PersonalityWeight { personality = NPCPersonalityType.Kind, weight = 0.15f },
        new PersonalityWeight { personality = NPCPersonalityType.Evil, weight = 0.15f }
    };
    [SerializeField] private TraitWeight[] traitWeights = new TraitWeight[]
    {
        new TraitWeight { trait = NPCTraitType.FarmExpert, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.LivestockExpert, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.CheapLabor, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.NightOwl, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.EarlyBird, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.SocialMaster, weight = 0.1f },
        new TraitWeight { trait = NPCTraitType.Bootlicker, weight = 0.1f }
    };
    [SerializeField] private Vector2Int traitCountRange = new Vector2Int(0, 3);  // 每个NPC拥有的词条数量范围
    
    [Header("姓名库")]
    [SerializeField] private string[] firstNames = new string[]
    {
        // 美好寓意类 (15个)
        "嘉禾", "瑞丰", "和谐", "康宁", "吉祥",
        "安康", "福禄", "贵荣", "昌盛", "兴旺", 
        "富贵", "如意", "平安", "顺利", "美好",
        
        // 农业主题类 (15个)
        "丰收", "播种", "耕耘", "灌溉", "施肥",
        "育苗", "培土", "收割", "晒谷", "储粮",
        "春耕", "夏耘", "秋收", "冬藏", "五谷",
        
        // 品德品质类 (10个)
        "勤劳", "朴实", "诚信", "善良", "忠厚",
        "踏实", "勇敢", "智慧", "仁爱", "正直"
    };
    [SerializeField] private string[] lastNames  = new string[]
    {
        // 常见姓氏 (20个)
        "张", "王", "李", "赵", "刘", 
        "陈", "杨", "黄", "周", "吴",
        "徐", "孙", "胡", "朱", "高",
        "林", "何", "郭", "马", "罗",
        
        // 农业相关姓氏 (10个)
        "田", "秧", "禾", "苗", "稻", 
        "粟", "谷", "麦", "豆", "菜",
        
        // 自然相关姓氏 (10个) 
        "山", "水", "石", "木", "花",
        "草", "柳", "松", "竹", "梅"
    };
    
    // 属性访问器
    public Vector2Int SalaryRange => salaryRange;
    public Vector2Int BaseWorkAbilityRange => baseWorkAbilityRange;
    public Vector2Int ItemCapacityRange => itemCapacityRange;
    public Vector2Int ItemTakeEachTimeCapacityRange => itemTakeEachTimeCapacityRange;
    public Vector2Int RestStartHourRange => restStartHourRange;
    public Vector2Int RestEndHourRange => restEndHourRange;
    public Vector2Int WorkStartHourRange => workStartHourRange;
    public Vector2Int WorkEndHourRange => workEndHourRange;
    public PersonalityWeight[] PersonalityWeights => personalityWeights;
    public TraitWeight[] TraitWeights => traitWeights;
    public Vector2Int TraitCountRange => traitCountRange;
    public string[] FirstNames => firstNames;
    public string[] LastNames => lastNames;
}

[System.Serializable]
public class PersonalityWeight
{
    public NPCPersonalityType personality;
    [Range(0f, 1f)]
    public float weight = 1f;
}

[System.Serializable]
public class TraitWeight
{
    public NPCTraitType trait;
    [Range(0f, 1f)]
    public float weight = 1f;
}