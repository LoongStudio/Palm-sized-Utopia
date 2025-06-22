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
        if (!npc.PendingWorkTarget.HasValue)
        {
            var targetBuilding = BuildingManager.Instance.GetBestWorkBuildingForNPC(npc);
            if (targetBuilding != null)
            {
                // 使用NPCMovement的MoveToTarget方法，而不是直接设置currentTarget
                npc.MoveToTarget(targetBuilding.transform.position);
                npc.SetPendingWork(targetBuilding.transform.position, targetBuilding);
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
            npc.MoveToTarget(npc.PendingWorkTarget.Value);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点: {npc.AssignedBuilding?.data.buildingName ?? "未知"}");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达工作地点 - 使用NPCMovement的isInPosition属性
        if (npc.isInPosition)
        {
            // 如果这是待处理的工作，清除待处理标记
            if (npc.PendingWorkTarget.HasValue)
            {
                npc.ClearPendingWork();
            }
            
            // 进入工作状态
            stateMachine.ChangeState(nextState);
        }
    }
} 