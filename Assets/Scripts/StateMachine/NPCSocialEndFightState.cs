using UnityEngine;

public class NPCSocialEndFightState : NPCStateBase
{    
    public override float stateExitTime => 2f;
    public override bool exitStateWhenTimeOut => true;
    public override NPCState nextState => NPCState.Idle;
    public NPCSocialEndFightState(NPCStateMachine stateMachine, NPC npc) : base(stateMachine, npc)
    {
    }
    protected override void OnEnterState()
    {
        base.OnEnterState();
        animator.SetBool("isSocialHappy", false);
    }
    protected override void OnExitState()
    {
        base.OnExitState();
        animator.SetTrigger("Idle");
    }
    protected override void OnUpdateState()
    {
        base.OnUpdateState();
    }
    protected override void OnFixedUpdateState()
    {
        base.OnFixedUpdateState();
    }
}