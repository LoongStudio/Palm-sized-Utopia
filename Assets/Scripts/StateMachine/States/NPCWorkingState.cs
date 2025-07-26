using UnityEngine;

public class NPCWorkingState : NPCStateBase
{
    public NPCWorkingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "工作中";
        nextState = NPCState.WorkComplete;
    }

    public float WorkingTimer = 0.0f;
    public float WorkingTimerTotal = 0.0f;
    public float WorkingMaxTimeForTemp = 12.0f;
    public float WorkingEachTime = 0.5f;
    
    protected override void OnEnterState()
    {
        base.OnEnterState();

        animator.SetBool("isWorking", true);
        
        GameEvents.TriggerNPCInWorkingPosition(new NPCEventArgs(){
            npc = npc,
            relatedBuilding = npc.AssignedTask.building
        });

        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkingState] {npc.data.npcName} 正在 {npc.AssignedTask.building} 工作 {npc.AssignedTask.taskType}");
        }
        // switch (npc.assignedTask.taskType)
        // {
        //     case TaskType.Production:
        //         // 改为在查找建筑时就assigned，该检查无效，替换为重复校验
        //         if (npc.assignedTask.building.assignedNPCs.Contains(npc))
        //         {
        //             Debug.Log($"[Work] 工作注册校验 NPC {npc.data.npcName} "
        //                       + $"成功到达目标 注册建筑 {npc.assignedTask.building.data.subType} ");
        //         }
        //         else
        //         {
        //             Debug.LogError($"[Work] 工作注册校验 NPC {npc.data.npcName} "
        //                       + $"未到达指定目标 注册建筑 {npc.assignedTask.building.data.subType} 退出");
        //             stateMachine.ChangeState(NPCState.Idle);
        //         }
        //         // if (npc.assignedTask.building.TryAssignNPC(npc))
        //         // {
        //         //     nextState = NPCState.WorkComplete;
        //         // }
        //         // else // 如果无法给当前NPC提供工作 重新进入Idle
        //         // {
        //         //     Debug.LogWarning($"[Work] 发现无法给NPC分配原先已经规划好的工作 "
        //         //                      + $"{npc.assignedTask.building.data.subType} "
        //         //                      + $"{npc.assignedTask.taskType} 情况，重新进入Idle");
        //         //     nextState = NPCState.Idle;
        //         //     stateMachine.ChangeState(NPCState.Idle);
        //         // }
        //         break;
        //     case TaskType.HandlingAccept:
        //         nextState = NPCState.Idle;
        //         break;
        //     case TaskType.HandlingDrop:
        //         nextState = NPCState.Idle;
        //         break;
        //     default:
        //         Debug.LogWarning($"[Work] 发现 {npc.name} 意料外的进入工作情况，重新进入Idle");
        //         nextState = NPCState.Idle;
        //         stateMachine.ChangeState(NPCState.Idle);
        //         break;
        // }
        
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
            if (!npc.AssignedTask.IsNone())
            {
                npc.SetPendingWork(npc.AssignedTask);
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCWorkingState] {npc.data.npcName} "
                              + $"将当前工作建筑设为PendingWork: {npc.AssignedTask.building.data.buildingName}");
                }
            }
            
            stateMachine.ChangeState(NPCState.Idle);
        }
        // 或者 建筑是否能够继续进行生产 但是不进行 pendingWork 储存
        if (npc.AssignedTask.taskType == TaskType.Production
            && npc.AssignedTask.building.data.buildingType == BuildingType.Production
            && !((ProductionBuilding)npc.AssignedTask.building).CanProduceAnyRule())
        {
            stateMachine.ChangeState(NPCState.Idle);
        }
        
        // 每个工作帧
        WorkingTimer += Time.deltaTime;
        WorkingTimerTotal += Time.deltaTime;
        if (WorkingTimer > WorkingEachTime)
        {
            WorkingTimer = 0;
            if (npc.AssignedTask.taskType == TaskType.HandlingAccept)
            {
                // 如果已经没有东西可以传输了
                if (!npc.AssignedTask.building.inventory.TransferToWithIgnore(
                    npc.inventory, 
                    npc.data.itemTakeEachTimeCapacity, 
                    npc.AssignedTask.building.AcceptResources))
                    stateMachine.ChangeState(NPCState.Idle);
            }
            if (npc.AssignedTask.taskType == TaskType.HandlingDrop)
            {
                // 如果已经没有东西可以传输了
                if (!npc.inventory.TransferToWithFilter(
                    npc.AssignedTask.building.inventory, 
                    npc.data.itemTakeEachTimeCapacity, 
                    npc.AssignedTask.building.AcceptResources))
                    stateMachine.ChangeState(NPCState.Idle);
            }
        }
        // 如果是拾取和放置
        if ((npc.AssignedTask.taskType == TaskType.HandlingAccept
            || npc.AssignedTask.taskType == TaskType.HandlingDrop) 
            &&WorkingMaxTimeForTemp < WorkingTimerTotal)
        {
            stateMachine.ChangeState(NPCState.Idle);
        }
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        animator.SetBool("isWorking", false);
        WorkingTimer = 0.0f;
        WorkingTimerTotal = 0.0f;

        if (showDebugInfo)
        {
            Debug.Log($"[Work] {npc.data.npcName} 离开工作状态，清除已分配建筑: "
                      + $"{npc.AssignedTask?.building.data.buildingName ?? "None"}");
        }

        Building relatedBuilding = npc.AssignedTask.building;
        // 清除AssignedBuilding（如果有PendingWork会在下次工作时重新分配）
        npc.AssignedTask?.building.TryRemoveNPC(npc);
        npc.AssignTask(TaskInfo.GetNone());

        GameEvents.TriggerNPCLeaveWorkingPosition(new NPCEventArgs(){
            npc = npc,
            relatedBuilding = relatedBuilding
        });
    }
} 