using UnityEngine;

public class NPCMovingToWorkState : NPCStateBase
{
    public NPCMovingToWorkState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "前往工作地点";
        nextState = NPCState.Working;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        
        // 如果没有待处理的工作，寻找新的工作
        if (npc.PendingWorkBuilding == null)
        {
            var targetBuilding = BuildingManager.Instance.GetBestWorkBuildingForNPC(npc);
            if (targetBuilding != null)
            {
                // 使用NPCMovement的MoveToTarget方法，而不是直接设置currentTarget
                npc.MoveToTarget(targetBuilding.transform.position);
                npc.SetPendingWork(targetBuilding);
            }
            else
            {
                // 如果没有找到工作，返回空闲状态
                stateMachine.ChangeState(NPCState.Idle);
                return;
            }
        }
        else
        {
            // 有待处理的工作，直接移动到目标位置
            npc.MoveToTarget(npc.PendingWorkBuilding.transform.position);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点: {npc.PendingWorkBuilding?.data.buildingName ?? "未知"}");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达工作地点 - 使用NPCMovement的isInPosition属性
        if (npc.isInPosition)
        {
            // 如果这是待处理的工作，设置为已分配建筑
            if (npc.PendingWorkBuilding != null)
            {
                npc.AssignedBuilding = npc.PendingWorkBuilding;
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 已分配到建筑: {npc.AssignedBuilding.data.buildingName}");
                }
                npc.ClearPendingWork();
            }
            
            // 进入工作状态
            stateMachine.ChangeState(nextState);
        }
    }
} 