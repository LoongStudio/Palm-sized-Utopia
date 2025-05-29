// NPC.cs
using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour
{
    [Header("基本信息")]
    public string npcName;
    public int npcId;
    
    [Header("状态属性")]
    [Range(0, 100)] public int favorability = 50;      // 好感度
    [Range(0, 100)] public int energy = 100;           // 精力
    [Range(0, 100)] public int mood = 70;              // 心情
    [Range(0, 100)] public int experience = 0;         // 经验值
    
    [Header("工作相关")]
    public Building assignedBuilding;
    public bool isWorking = false;
    public float workTimer = 0f;
    public float workCooldown = 5f; // 工作间隔
    
    [Header("个性系统")]
    public NPCPersonality personality;
    
    [Header("AI行为")]
    public NPCState currentState = NPCState.Idle;
    public Vector3 targetPosition;
    public float moveSpeed = 2f;
    
    [Header("薪资")]
    public int dailyWage = 50;
    
    // 事件
    public event System.Action<NPC> OnFavorabilityChanged;
    public event System.Action<NPC> OnStateChanged;
    public event System.Action<NPC, int> OnWorkCompleted;
    
    private Coroutine currentBehaviorCoroutine;
    
    private void Awake()
    {
        if (personality == null)
        {
            personality = new NPCPersonality();
        }
        
        if (string.IsNullOrEmpty(npcName))
        {
            GenerateRandomName();
        }
        
        npcId = GetInstanceID();
    }
    
    private void Start()
    {
        StartBehaviorLoop();
    }
    
    private void Update()
    {
        UpdateTimers();
        UpdateMovement();
    }
    
    #region 初始化
    
    private void GenerateRandomName()
    {
        string[] names = { "小明", "小红", "小强", "小丽", "小王", "小李", "小张", "小刘" };
        npcName = names[Random.Range(0, names.Length)] + Random.Range(1, 100).ToString();
    }
    
    #endregion
    
    #region 状态管理
    
    public void ChangeState(NPCState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(this);
            Debug.Log($"{npcName} 状态变更为: {newState}");
        }
    }
    
    private void UpdateTimers()
    {
        if (isWorking)
        {
            workTimer += Time.deltaTime;
        }
        
        // 定期恢复精力和心情
        if (Time.time % 30f < Time.deltaTime) // 每30秒
        {
            RecoverAttributes();
        }
    }
    
    private void RecoverAttributes()
    {
        if (!isWorking)
        {
            energy = Mathf.Min(100, energy + 5);
            mood = Mathf.Min(100, mood + 2);
        }
    }
    
    #endregion
    
    #region 工作系统
    
    public bool AssignToBuilding(Building building)
    {
        if (assignedBuilding != null)
        {
            UnassignFromBuilding();
        }
        
        assignedBuilding = building;
        ChangeState(NPCState.MovingToWork);
        SetTargetPosition(building.transform.position);
        
        Debug.Log($"{npcName} 被分配到 {building.data.buildingName}");
        return true;
    }
    
    public void UnassignFromBuilding()
    {
        if (assignedBuilding != null)
        {
            assignedBuilding = null;
            isWorking = false;
            ChangeState(NPCState.Idle);
            Debug.Log($"{npcName} 离开了工作岗位");
        }
    }
    
    public float GetWorkEfficiency()
    {
        float efficiency = personality.workEfficiency;
        
        // 心情影响效率
        efficiency *= (mood / 100f) * 0.5f + 0.5f; // 心情50%影响
        
        // 精力影响效率
        efficiency *= (energy / 100f) * 0.3f + 0.7f; // 精力30%影响
        
        // 好感度影响效率
        efficiency *= (favorability / 100f) * 0.2f + 0.8f; // 好感度20%影响
        
        return efficiency;
    }
    
    private void PerformWork()
    {
        if (assignedBuilding == null || energy < 10)
        {
            ChangeState(NPCState.Resting);
            return;
        }
        
        isWorking = true;
        workTimer = 0f;
        
        // 消耗精力
        energy = Mathf.Max(0, energy - Random.Range(5, 15));
        
        // 工作可能影响心情
        if (Random.Range(0f, 1f) < 0.3f) // 30%概率
        {
            int moodChange = Random.Range(-5, 10);
            ChangeMood(moodChange);
        }
        
        // 获得经验
        GainExperience(Random.Range(1, 5));
        
        // 触发工作完成事件
        OnWorkCompleted?.Invoke(this, Random.Range(1, 3));
        
        Debug.Log($"{npcName} 完成了一次工作，效率: {GetWorkEfficiency():F2}");
    }
    
    #endregion
    
    #region 好感度系统
    
    public void ChangeFavorability(int amount)
    {
        int oldFavorability = favorability;
        favorability = Mathf.Clamp(favorability + Mathf.RoundToInt(amount * personality.favorabilityGainRate), 0, 100);
        
        if (favorability != oldFavorability)
        {
            OnFavorabilityChanged?.Invoke(this);
            
            // 好感度变化影响心情
            if (amount > 0)
            {
                ChangeMood(amount / 2);
            }
            
            Debug.Log($"{npcName} 好感度变化: {oldFavorability} -> {favorability}");
        }
    }
    
    public void ChangeMood(int amount)
    {
        mood = Mathf.Clamp(mood + amount, 0, 100);
    }
    
    public FavorabilityLevel GetFavorabilityLevel()
    {
        if (favorability >= 91) return FavorabilityLevel.BestFriend;
        if (favorability >= 71) return FavorabilityLevel.Close;
        if (favorability >= 41) return FavorabilityLevel.Friendly;
        if (favorability >= 21) return FavorabilityLevel.Normal;
        return FavorabilityLevel.Cold;
    }
    
    public string GetFavorabilityDescription()
    {
        switch (GetFavorabilityLevel())
        {
            case FavorabilityLevel.BestFriend: return "挚友";
            case FavorabilityLevel.Close: return "亲密";
            case FavorabilityLevel.Friendly: return "友好";
            case FavorabilityLevel.Normal: return "普通";
            case FavorabilityLevel.Cold: return "冷漠";
            default: return "未知";
        }
    }
    
    #endregion
    
    #region 经验和成长
    
    public void GainExperience(int amount)
    {
        experience = Mathf.Min(100, experience + Mathf.RoundToInt(amount * personality.learningSpeed));
        
        // 经验影响工作效率
        if (experience > 50 && personality.workEfficiency < 1.5f)
        {
            personality.workEfficiency += 0.01f;
        }
    }
    
    #endregion
    
    #region AI行为系统
    
    private void StartBehaviorLoop()
    {
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }
        
        currentBehaviorCoroutine = StartCoroutine(BehaviorLoop());
    }
    
    private IEnumerator BehaviorLoop()
    {
        while (true)
        {
            switch (currentState)
            {
                case NPCState.Idle:
                    yield return HandleIdleState();
                    break;
                    
                case NPCState.MovingToWork:
                    yield return HandleMovingToWorkState();
                    break;
                    
                case NPCState.Working:
                    yield return HandleWorkingState();
                    break;
                    
                case NPCState.Resting:
                    yield return HandleRestingState();
                    break;
                    
                case NPCState.Socializing:
                    yield return HandleSocializingState();
                    break;
            }
            
            yield return new WaitForSeconds(0.1f); // 避免过于频繁的状态检查
        }
    }
    
    private IEnumerator HandleIdleState()
    {
        // 闲置状态：寻找工作或进行其他活动
        if (assignedBuilding != null)
        {
            ChangeState(NPCState.MovingToWork);
        }
        else
        {
            // 随机活动
            if (Random.Range(0f, 1f) < 0.1f) // 10%概率社交
            {
                ChangeState(NPCState.Socializing);
            }
        }
        
        yield return new WaitForSeconds(Random.Range(2f, 5f));
    }
    
    private IEnumerator HandleMovingToWorkState()
    {
        if (assignedBuilding == null)
        {
            ChangeState(NPCState.Idle);
            yield break;
        }
        
        // 检查是否到达目标位置
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            ChangeState(NPCState.Working);
        }
        
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator HandleWorkingState()
    {
        if (assignedBuilding == null || energy < 10)
        {
            ChangeState(NPCState.Resting);
            yield break;
        }
        
        if (workTimer >= workCooldown)
        {
            PerformWork();
            workTimer = 0f;
        }
        
        // 有概率休息
        if (energy < 30 && Random.Range(0f, 1f) < 0.3f)
        {
            ChangeState(NPCState.Resting);
        }
        
        yield return new WaitForSeconds(1f);
    }
    
    private IEnumerator HandleRestingState()
    {
        // 恢复精力和心情
        energy = Mathf.Min(100, energy + 10);
        mood = Mathf.Min(100, mood + 5);
        
        // 休息足够后回到工作
        if (energy > 70)
        {
            if (assignedBuilding != null)
            {
                ChangeState(NPCState.Working);
            }
            else
            {
                ChangeState(NPCState.Idle);
            }
        }
        
        yield return new WaitForSeconds(3f);
    }
    
    private IEnumerator HandleSocializingState()
    {
        // 社交活动：提升心情和好感度
        ChangeMood(Random.Range(5, 15));
        ChangeFavorability(Random.Range(1, 5));
        
        // 社交结束
        yield return new WaitForSeconds(Random.Range(5f, 10f));
        ChangeState(NPCState.Idle);
    }
    
    #endregion
    
    #region 移动系统
    
    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }
    
    private void UpdateMovement()
    {
        if (currentState == NPCState.MovingToWork && assignedBuilding != null)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            
            // 面向移动方向
            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }
    }
    
    #endregion
    
    #region 薪资系统
    
    public void PayDailyWage()
    {
        // 根据好感度调整薪资需求
        float wageMod = 1f;
        switch (GetFavorabilityLevel())
        {
            case FavorabilityLevel.BestFriend:
                wageMod = 0.8f; // 挚友愿意少拿薪水
                break;
            case FavorabilityLevel.Cold:
                wageMod = 1.3f; // 冷漠需要更高薪水
                break;
        }
        
        int actualWage = Mathf.RoundToInt(dailyWage * wageMod);
        
        if (ResourceManager.Instance.SpendResource(ResourceType.Gold, actualWage))
        {
            ChangeFavorability(5); // 按时发薪提升好感度
            Debug.Log($"支付给 {npcName} 薪水: {actualWage} 金币");
        }
        else
        {
            ChangeFavorability(-10); // 无法支付薪水降低好感度
            ChangeMood(-15);
            Debug.LogWarning($"无法支付给 {npcName} 薪水！");
        }
    }
    
    #endregion
    
    #region 信息获取
    
    public NPCInfo GetNPCInfo()
    {
        return new NPCInfo
        {
            npcId = npcId,
            npcName = npcName,
            favorability = favorability,
            favorabilityLevel = GetFavorabilityLevel(),
            energy = energy,
            mood = mood,
            experience = experience,
            currentState = currentState,
            workEfficiency = GetWorkEfficiency(),
            assignedBuildingName = assignedBuilding?.data.buildingName ?? "无",
            personalityDescription = personality.GetPersonalityDescription(),
            dailyWage = dailyWage
        };
    }
    
    #endregion
    
    private void OnDestroy()
    {
        if (currentBehaviorCoroutine != null)
        {
            StopCoroutine(currentBehaviorCoroutine);
        }
    }
}

public enum NPCState
{
    Idle,           // 闲置
    MovingToWork,   // 前往工作
    Working,        // 工作中
    Resting,        // 休息
    Socializing     // 社交
}

public enum FavorabilityLevel
{
    Cold = 0,       // 冷漠 (0-20)
    Normal = 1,     // 普通 (21-40)
    Friendly = 2,   // 友好 (41-70)
    Close = 3,      // 亲密 (71-90)
    BestFriend = 4  // 挚友 (91-100)
}

[System.Serializable]
public class NPCInfo
{
    public int npcId;
    public string npcName;
    public int favorability;
    public FavorabilityLevel favorabilityLevel;
    public int energy;
    public int mood;
    public int experience;
    public NPCState currentState;
    public float workEfficiency;
    public string assignedBuildingName;
    public string personalityDescription;
    public int dailyWage;
}