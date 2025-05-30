// NPCManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : SingletonManager<NPCManager>
{
    [Header("NPC配置")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Transform npcParent;
    [SerializeField] private int maxNPCs = 20;
    [SerializeField] private int hireBaseCost = 100;
    
    [Header("当前NPC")]
    private List<NPC> allNPCs;
    private Dictionary<int, NPC> npcDict;
    
    [Header("薪资系统")]
    [SerializeField] private float payrollInterval = 86400f; // 24小时 = 86400秒
    private float payrollTimer = 0f;
    
    // 事件
    public event System.Action<NPC> OnNPCHired;
    public event System.Action<NPC> OnNPCFired;
    public event System.Action<NPC> OnNPCFavorabilityChanged;
    public event System.Action OnPayrollProcessed;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeNPCSystem();
    }
    
    private void Update()
    {
        UpdatePayroll();
    }
    
    public void Initialize()
    {
        LoadNPCPrefab();
        Debug.Log("NPC系统初始化完成");
    }
    
    private void InitializeNPCSystem()
    {
        allNPCs = new List<NPC>();
        npcDict = new Dictionary<int, NPC>();
        
        if (npcParent == null)
        {
            GameObject parent = new GameObject("NPCs");
            npcParent = parent.transform;
        }
    }
    
    private void LoadNPCPrefab()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC预制体未设置！");
        }
    }
    
    #region NPC雇佣和解雇
    
    public bool CanHireNPC()
    {
        if (allNPCs.Count >= maxNPCs)
        {
            Debug.LogWarning("已达到NPC雇佣上限");
            return false;
        }
        
        int hireCost = GetHireCost();
        if (!ResourceManager.Instance.HasEnoughResources(new ResourceCost[] 
        {
            new ResourceCost { resourceType = ResourceType.Gold, amount = hireCost }
        }))
        {
            Debug.LogWarning($"金币不足，雇佣需要 {hireCost} 金币");
            return false;
        }
        
        return true;
    }
    
    public NPC HireNPC()
    {
        if (!CanHireNPC())
        {
            return null;
        }
        
        int hireCost = GetHireCost();
        if (!ResourceManager.Instance.SpendResource(ResourceType.Gold, hireCost))
        {
            return null;
        }
        
        // 创建NPC
        GameObject npcObj = Instantiate(npcPrefab, GetRandomSpawnPosition(), Quaternion.identity, npcParent);
        NPC npc = npcObj.GetComponent<NPC>();
        
        if (npc == null)
        {
            npc = npcObj.AddComponent<NPC>();
        }
        
        // 注册NPC
        RegisterNPC(npc);
        
        // 触发事件
        OnNPCHired?.Invoke(npc);
        
        Debug.Log($"雇佣了新NPC: {npc.npcName}，花费 {hireCost} 金币");
       return npc;
   }
   
   public bool FireNPC(NPC npc)
   {
       if (npc == null || !allNPCs.Contains(npc))
       {
           return false;
       }
       
       // 从建筑中移除
       if (npc.assignedBuilding != null)
       {
           npc.assignedBuilding.RemoveNPC(npc);
       }
       
       // 注销NPC
       UnregisterNPC(npc);
       
       // 触发事件
       OnNPCFired?.Invoke(npc);
       
       // 销毁对象
       Destroy(npc.gameObject);
       
       Debug.Log($"解雇了NPC: {npc.npcName}");
       return true;
   }
   
   private int GetHireCost()
   {
       // 雇佣成本随现有NPC数量增加
       return hireBaseCost + (allNPCs.Count * 20);
   }
   
   private Vector3 GetRandomSpawnPosition()
   {
       // 在农场周围随机生成位置
       return new Vector3(
           Random.Range(-5f, 5f),
           0f,
           Random.Range(-5f, 5f)
       );
   }
   
   #endregion
   
   #region NPC注册和管理
   
   public void RegisterNPC(NPC npc)
   {
       if (!allNPCs.Contains(npc))
       {
           allNPCs.Add(npc);
           npcDict[npc.npcId] = npc;
           
           // 订阅NPC事件
           npc.OnFavorabilityChanged += HandleNPCFavorabilityChanged;
           npc.OnWorkCompleted += HandleNPCWorkCompleted;
       }
   }
   
   public void UnregisterNPC(NPC npc)
   {
       if (allNPCs.Remove(npc))
       {
           npcDict.Remove(npc.npcId);
           
           // 取消订阅事件
           npc.OnFavorabilityChanged -= HandleNPCFavorabilityChanged;
           npc.OnWorkCompleted -= HandleNPCWorkCompleted;
       }
   }
   
   private void HandleNPCFavorabilityChanged(NPC npc)
   {
       OnNPCFavorabilityChanged?.Invoke(npc);
   }
   
   private void HandleNPCWorkCompleted(NPC npc, int productivity)
   {
       // 处理NPC工作完成，可以在这里添加额外奖励等
       Debug.Log($"{npc.npcName} 完成工作，生产力: {productivity}");
   }
   
   #endregion
   
   #region NPC分配和查询
   
   public bool AssignNPCToBuilding(NPC npc, Building building)
   {
       if (npc == null || building == null)
           return false;
       
       return building.AssignNPC(npc);
   }
   
   public bool RemoveNPCFromBuilding(NPC npc)
   {
       if (npc == null || npc.assignedBuilding == null)
           return false;
       
       return npc.assignedBuilding.RemoveNPC(npc);
   }
   
   public List<NPC> GetUnassignedNPCs()
   {
       return allNPCs.FindAll(npc => npc.assignedBuilding == null);
   }
   
   public List<NPC> GetNPCsAssignedToBuilding(Building building)
   {
       return allNPCs.FindAll(npc => npc.assignedBuilding == building);
   }
   
   public NPC GetNPCById(int id)
   {
       return npcDict.ContainsKey(id) ? npcDict[id] : null;
   }
   
   public List<NPC> GetAllNPCs()
   {
       return new List<NPC>(allNPCs);
   }
   
   #endregion
   
   #region 薪资系统
   
   private void UpdatePayroll()
   {
       payrollTimer += Time.deltaTime;
       
       if (payrollTimer >= payrollInterval)
       {
           ProcessPayroll();
           payrollTimer = 0f;
       }
   }
   
   public void ProcessPayroll()
   {
       int totalWages = 0;
       int successfulPayments = 0;
       
       foreach (var npc in allNPCs)
       {
           if (npc != null)
           {
               int wage = CalculateNPCWage(npc);
               totalWages += wage;
               
               if (ResourceManager.Instance.GetResourceAmount(ResourceType.Gold) >= wage)
               {
                   npc.PayDailyWage();
                   successfulPayments++;
               }
               else
               {
                   // 资金不足，NPC不满意
                   npc.ChangeFavorability(-15);
                   npc.ChangeMood(-20);
                   Debug.LogWarning($"无法支付 {npc.npcName} 的薪水！");
               }
           }
       }
       
       OnPayrollProcessed?.Invoke();
       
       Debug.Log($"薪资处理完成: {successfulPayments}/{allNPCs.Count} 个NPC获得薪水，总计 {totalWages} 金币");
   }
   
   private int CalculateNPCWage(NPC npc)
   {
       float wageMod = 1f;
       
       // 根据好感度调整薪资
       switch (npc.GetFavorabilityLevel())
       {
           case FavorabilityLevel.BestFriend:
               wageMod = 0.8f;
               break;
           case FavorabilityLevel.Close:
               wageMod = 0.9f;
               break;
           case FavorabilityLevel.Cold:
               wageMod = 1.3f;
               break;
       }
       
       // 根据经验调整薪资
       wageMod += npc.experience * 0.005f; // 经验越高薪资越高
       
       return Mathf.RoundToInt(npc.dailyWage * wageMod);
   }
   
   public int GetTotalDailyWages()
   {
       int total = 0;
       foreach (var npc in allNPCs)
       {
           if (npc != null)
           {
               total += CalculateNPCWage(npc);
           }
       }
       return total;
   }
   
   // 手动发放薪资（测试用）
   public void ProcessPayrollManually()
   {
       ProcessPayroll();
   }
   
   #endregion
   
   #region 自动化NPC管理
   
   public void AutoAssignUnemployedNPCs()
   {
       var unassignedNPCs = GetUnassignedNPCs();
       var availableBuildings = GetBuildingsWithCapacity();
       
       foreach (var npc in unassignedNPCs)
       {
           var suitableBuilding = FindSuitableBuildingForNPC(npc, availableBuildings);
           if (suitableBuilding != null)
           {
               AssignNPCToBuilding(npc, suitableBuilding);
               availableBuildings.Remove(suitableBuilding);
               
               if (availableBuildings.Count == 0)
                   break;
           }
       }
   }
   
   private List<Building> GetBuildingsWithCapacity()
   {
       var buildings = new List<Building>();
       
       if (BuildingManager.Instance != null)
       {
           foreach (var building in BuildingManager.Instance.GetAllBuildings())
           {
               if (building.CanAssignNPC(null)) // 检查是否有空位
               {
                   buildings.Add(building);
               }
           }
       }
       
       return buildings;
   }
   
   private Building FindSuitableBuildingForNPC(NPC npc, List<Building> availableBuildings)
   {
       // 简单的匹配算法：优先分配到生产建筑
       Building bestBuilding = null;
       
       foreach (var building in availableBuildings)
       {
           if (building.data.isProductionBuilding)
           {
               bestBuilding = building;
               break;
           }
       }
       
       // 如果没有生产建筑，分配到任意有空位的建筑
       if (bestBuilding == null && availableBuildings.Count > 0)
       {
           bestBuilding = availableBuildings[0];
       }
       
       return bestBuilding;
   }
   
   #endregion
   
   #region 统计和分析
   
   public NPCStatistics GetNPCStatistics()
   {
       NPCStatistics stats = new NPCStatistics();
       
       stats.totalNPCs = allNPCs.Count;
       stats.assignedNPCs = allNPCs.FindAll(npc => npc.assignedBuilding != null).Count;
       stats.unassignedNPCs = stats.totalNPCs - stats.assignedNPCs;
       
       if (allNPCs.Count > 0)
       {
           float totalFavorability = 0f;
           float totalMood = 0f;
           float totalEfficiency = 0f;
           
           foreach (var npc in allNPCs)
           {
               totalFavorability += npc.favorability;
               totalMood += npc.mood;
               totalEfficiency += npc.GetWorkEfficiency();
           }
           
           stats.averageFavorability = totalFavorability / allNPCs.Count;
           stats.averageMood = totalMood / allNPCs.Count;
           stats.averageEfficiency = totalEfficiency / allNPCs.Count;
       }
       
       stats.totalDailyWages = GetTotalDailyWages();
       
       // 好感度分布
       stats.favorabilityDistribution = new int[5];
       foreach (var npc in allNPCs)
       {
           stats.favorabilityDistribution[(int)npc.GetFavorabilityLevel()]++;
       }
       
       return stats;
   }
   
   public List<NPC> GetTopPerformingNPCs(int count = 5)
   {
       var sortedNPCs = new List<NPC>(allNPCs);
       sortedNPCs.Sort((a, b) => b.GetWorkEfficiency().CompareTo(a.GetWorkEfficiency()));
       
       return sortedNPCs.GetRange(0, Mathf.Min(count, sortedNPCs.Count));
   }
   
   public List<NPC> GetLowMoraleNPCs(int moraleThreshold = 30)
   {
       return allNPCs.FindAll(npc => npc.mood < moraleThreshold || npc.favorability < moraleThreshold);
   }
   
   #endregion
   
   #region 特殊事件和互动
   
   public void TriggerRandomNPCEvent()
   {
       if (allNPCs.Count == 0) return;
       
       NPC randomNPC = allNPCs[Random.Range(0, allNPCs.Count)];
       NPCEventType eventType = (NPCEventType)Random.Range(0, System.Enum.GetValues(typeof(NPCEventType)).Length);
       
       ProcessNPCEvent(randomNPC, eventType);
   }
   
   private void ProcessNPCEvent(NPC npc, NPCEventType eventType)
   {
       switch (eventType)
       {
           case NPCEventType.GoodMood:
               npc.ChangeMood(Random.Range(10, 20));
               npc.ChangeFavorability(Random.Range(5, 10));
               Debug.Log($"{npc.npcName} 心情很好！");
               break;
               
           case NPCEventType.BadMood:
               npc.ChangeMood(Random.Range(-15, -5));
               Debug.Log($"{npc.npcName} 心情不好...");
               break;
               
           case NPCEventType.SkillImprovement:
               npc.GainExperience(Random.Range(10, 25));
               Debug.Log($"{npc.npcName} 技能有所提升！");
               break;
               
           case NPCEventType.SocialInteraction:
               // 随机选择另一个NPC进行互动
               if (allNPCs.Count > 1)
               {
                   NPC otherNPC;
                   do
                   {
                       otherNPC = allNPCs[Random.Range(0, allNPCs.Count)];
                   } while (otherNPC == npc);
                   
                   // 两个NPC都获得好感度和心情提升
                   npc.ChangeFavorability(Random.Range(2, 8));
                   npc.ChangeMood(Random.Range(5, 15));
                   otherNPC.ChangeFavorability(Random.Range(2, 8));
                   otherNPC.ChangeMood(Random.Range(5, 15));
                   
                   Debug.Log($"{npc.npcName} 和 {otherNPC.npcName} 进行了愉快的交流！");
               }
               break;
       }
   }
   
   #endregion
   
   #region 存档系统
   
   public NPCSaveData GetSaveData()
   {
       NPCSaveData saveData = new NPCSaveData();
       
       foreach (var npc in allNPCs)
       {
           if (npc != null)
           {
               NPCSaveInfo info = new NPCSaveInfo
               {
                   npcId = npc.npcId,
                   npcName = npc.npcName,
                   favorability = npc.favorability,
                   energy = npc.energy,
                   mood = npc.mood,
                   experience = npc.experience,
                   dailyWage = npc.dailyWage,
                   assignedBuildingId = npc.assignedBuilding?.GetInstanceID() ?? -1,
                   position = npc.transform.position,
                   personality = npc.personality
               };
               
               saveData.npcs.Add(info);
           }
       }
       
       saveData.payrollTimer = payrollTimer;
       
       return saveData;
   }
   
   public void LoadSaveData(NPCSaveData saveData)
   {
       // 清空现有NPC
       ClearAllNPCs();
       
       // 重新创建NPC
       foreach (var info in saveData.npcs)
       {
           GameObject npcObj = Instantiate(npcPrefab, info.position, Quaternion.identity, npcParent);
           NPC npc = npcObj.GetComponent<NPC>();
           
           if (npc == null)
           {
               npc = npcObj.AddComponent<NPC>();
           }
           
           // 恢复NPC数据
           npc.npcId = info.npcId;
           npc.npcName = info.npcName;
           npc.favorability = info.favorability;
           npc.energy = info.energy;
           npc.mood = info.mood;
           npc.experience = info.experience;
           npc.dailyWage = info.dailyWage;
           npc.personality = info.personality;
           
           RegisterNPC(npc);
           
           // 建筑分配需要在建筑系统加载后处理
       }
       
       payrollTimer = saveData.payrollTimer;
   }
   
   private void ClearAllNPCs()
   {
       var npcsToDestroy = new List<NPC>(allNPCs);
       foreach (var npc in npcsToDestroy)
       {
           FireNPC(npc);
       }
   }
   
   #endregion
}

#region 数据结构定义

[System.Serializable]
public class NPCStatistics
{
   public int totalNPCs;
   public int assignedNPCs;
   public int unassignedNPCs;
   public float averageFavorability;
   public float averageMood;
   public float averageEfficiency;
   public int totalDailyWages;
   public int[] favorabilityDistribution; // 按好感度等级分布
}

public enum NPCEventType
{
   GoodMood,           // 好心情
   BadMood,            // 坏心情
   SkillImprovement,   // 技能提升
   SocialInteraction   // 社交互动
}

[System.Serializable]
public class NPCSaveData
{
   public List<NPCSaveInfo> npcs = new List<NPCSaveInfo>();
   public float payrollTimer;
}

[System.Serializable]
public class NPCSaveInfo
{
   public int npcId;
   public string npcName;
   public int favorability;
   public int energy;
   public int mood;
   public int experience;
   public int dailyWage;
   public int assignedBuildingId;
   public Vector3 position;
   public NPCPersonality personality;
}

#endregion