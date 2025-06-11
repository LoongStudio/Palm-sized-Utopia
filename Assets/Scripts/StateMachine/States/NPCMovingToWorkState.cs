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
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达工作地点
        if (Vector3.Distance(npc.transform.position, npc.currentTarget.position) < 0.1f)
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