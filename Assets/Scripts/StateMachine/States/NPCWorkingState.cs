using UnityEngine;

public class NPCWorkingState : NPCStateBase
{
    public NPCWorkingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "工作中";
        nextState = NPCState.Transporting;
    }

    public float WorkingTimer = 0.0f;
    public float WorkingEachTime = 0.5f;
    
    protected override void OnEnterState()
    {
        base.OnEnterState();
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkingState] {npc.data.npcName} 正在 {npc.assignedTask.building} 工作 {npc.assignedTask.taskType}");
        }
        switch (npc.assignedTask.taskType)
        {
            case TaskType.Production:
                if (npc.assignedTask.building.TryAssignNPC(npc))
                {
                    nextState = NPCState.WorkComplete;    
                }
                else // 如果无法给当前NPC提供工作 重新进入Idle
                {
                    Debug.LogWarning($"[Work] 发现无法给NPC分配原先已经规划好的工作 "
                                     + $"{npc.assignedTask.building.data.subType} "
                                     + $"{npc.assignedTask.taskType} 情况，重新进入Idle");
                    nextState = NPCState.Idle;
                    stateMachine.ChangeState(NPCState.Idle);
                }
                break;
            case TaskType.HandlingAccept:
                nextState = NPCState.Transporting;
                break;
            default:
                Debug.LogWarning($"[Work] 发现意料外的进入工作情况 "
                                 + $"{npc.assignedTask.building.data.subType} "
                                 + $"{npc.assignedTask.taskType}，重新进入Idle");
                nextState = NPCState.Idle;
                stateMachine.ChangeState(NPCState.Idle);
                break;
        }
        
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到了下班时间
        if (!npc.CanWorkNow() || npc.ShouldRest())
        {
            if (showDebugInfo)
            {
                Debug.Log($"[NPCWorkingState] {npc.data.npcName} 工作时间结束，准备下班");
            }
            // 将当前AssignedBuilding设为PendingWork，方便下次继续
            if (!npc.assignedTask.Equals(TaskInfo.GetNone()))
            {
                npc.SetPendingWork(new TaskInfo(npc.assignedTask.building, npc.assignedTask.taskType));
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCWorkingState] {npc.data.npcName} "
                              + $"将当前工作建筑设为PendingWork: {npc.assignedTask.building.data.buildingName}");
                }
            }
            
            stateMachine.ChangeState(NPCState.Idle);
        }
        
        // 每个工作帧
        WorkingTimer += Time.deltaTime;
        if (WorkingTimer > WorkingEachTime)
        {
            WorkingTimer = 0;
            if (npc.assignedTask.taskType == TaskType.HandlingAccept)
            {
                // 如果已经没有东西可以传输了
                if (!npc.assignedTask.building.inventory.TransferToWithIgnore(
                    npc.inventory, 
                    npc.data.itemTakeEachTimeCapacity, 
                    npc.assignedTask.building.AcceptResources))
                    stateMachine.ChangeState(NPCState.Transporting);
            }
        }
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        
        // 清除AssignedBuilding（如果有PendingWork会在下次工作时重新分配）
        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkingState] {npc.data.npcName} 离开工作状态，清除已分配建筑: "
                      + $"{npc.assignedTask.building.data.buildingName}");
        }
        npc.assignedTask = TaskInfo.GetNone();
    }
} 