using UnityEngine;

public class NPCMovingHomeState : NPCStateBase
{
    public NPCMovingHomeState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "回家中";
        nextState = NPCState.Sleeping;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        
        if (npc.housing != null)
        {
            // 设置housing为assignedBuilding
            npc.AssignedTask = (npc.housing, TaskType.Rest);
            if (showDebugInfo)
            {
                Debug.Log($"[NPCMovingHomeState] {npc.data.npcName} 正在回家: {npc.housing.data.buildingName}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"[NPCMovingHomeState] {npc.data.npcName} 没有住所，返回空闲状态");
            }
            stateMachine.ChangeState(NPCState.Idle);
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达家
        if (npc.isInPosition)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[NPCMovingHomeState] {npc.data.npcName} 已到家，准备休息");
            }
            stateMachine.ChangeState(nextState);
        }
    }
} 