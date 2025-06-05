using UnityEngine;
using System.Collections.Generic;

public class NPCManager : SingletonManager<NPCManager>
{
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
    public bool FireNPC(NPC npc) { return false; }
    public bool AssignNPCToBuilding(NPC npc, Building building) { return false; }
    public void RemoveNPCFromBuilding(NPC npc) { }
    
    // 查询方法
    public List<NPC> GetAllNPCs() { return null; }
    public List<NPC> GetAvailableNPCs() { return null; }
    public List<NPC> GetWorkingNPCs() { return null; }
    
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