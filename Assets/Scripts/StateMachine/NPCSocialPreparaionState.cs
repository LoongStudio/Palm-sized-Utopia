using UnityEngine;

public class NPCSocialPreparaionState : NPCStateBase
{
    public NPCSocialPreparaionState(NPCState state, NPCStateMachine stateMachine, NPC npc) : base(NPCState.PrepareForSocial, stateMachine, npc)
    {
    }
    protected override void OnEnterState(){
        base.OnEnterState();
        animator.SetBool("isSocialPreparation", true);

        // 停止移动
        npc.movement.StopRandomMovement();

        
    }
    protected override void OnExitState(){
        base.OnExitState();
        animator.SetBool("isSocialPreparation", false);
    }
    protected override void OnUpdateState(){
        base.OnUpdateState();
    }
}