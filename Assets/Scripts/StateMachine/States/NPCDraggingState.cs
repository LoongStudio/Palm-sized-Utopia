using UnityEngine;

public class NPCDraggingState : NPCStateBase
{
    public NPCDraggingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "被玩家拖动中";
        nextState = NPCState.Idle;
    }
    
    protected override void OnEnterState()
    {
        base.OnEnterState();
        // TODO: 添加被拖动时的挣扎动画
        animator.SetBool("isWorking", true);
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCDraggingState] {npc.data.npcName} 正在 {npc.AssignedTask.building} 工作 {npc.AssignedTask.taskType}");
        }
        
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        // TODO: 和上面一样
        animator.SetBool("isWorking", false);
        
    }
} 