using UnityEngine;

public class NPCSleepingState : NPCStateBase
{
    public NPCSleepingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "睡眠中";
        nextState = NPCState.Idle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSleepingState] {npc.data.npcName} 开始休息");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否应该结束休息
        if (!npc.IsRestTime() && !npc.ShouldRest())
        {
            // 只有在不是休息时间且不应该休息时才结束休息状态
            if (showDebugInfo)
            {
                Debug.Log($"[NPCSleepingState] {npc.data.npcName} 休息结束，准备开始新的一天");
            }
            stateMachine.ChangeState(NPCState.Idle);
        }
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        
        // 休息结束时清除AssignedBuilding
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSleepingState] {npc.data.npcName} 休息结束，清除已分配建筑");
        }
        npc.AssignedTask = (null, TaskType.None);
    }
} 