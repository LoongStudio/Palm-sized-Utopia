using UnityEngine;
using System.Collections.Generic;

public class NPCManager : SingletonManager<NPCManager>
{
    public static NPCManager Instance { get; private set; }
    
    [Header("NPC管理")]
    public List<NPCInstance> allNPCs;
    public Dictionary<int, NPCInstance> npcDict;
    public int nextNPCId = 1;
    
    [Header("社交系统")]
    public NPCSocialSystem socialSystem;
    
    [Header("雇佣设置")]
    public int hireCost = 100;
    public float dailyWageTime = 24f; // 每24小时发一次工资
    
    void Awake()
    {
        // 初始化NPC字典
    }
    
    void Start()
    {
        // 初始化社交系统
    }
    
    void Update()
    {
        // 更新所有NPC状态和社交系统
    }
    
    // 雇佣NPC
    public NPCInstance HireNPC(NPCData npcData) { return null; }
    
    // 解雇NPC
    public bool FireNPC(int npcId) { return false; }
    
    // 分配NPC工作
    public bool AssignWork(int npcId, int buildingId) { return false; }
    
    // 安排NPC休息
    public bool AssignRest(int npcId, Vector3 restLocation) { return false; }
    
    // 设置NPC空闲
    public bool SetIdle(int npcId) { return false; }
    
    // 发放工资
    public void PayWages() { }
    
    // 获取NPC实例
    public NPCInstance GetNPC(int npcId) { return null; }
    
    // 获取空闲的NPC
    public List<NPCInstance> GetIdleNPCs() { return null; }
    
    // 获取在指定建筑工作的NPC
    public List<NPCInstance> GetNPCsInBuilding(int buildingId) { return null; }
    
    // 检查是否有足够金币支付工资
    public bool CanPayWages() { return false; }
}
