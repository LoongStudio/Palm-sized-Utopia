using UnityEngine;

public class NPCSocialEndHappyState : NPCStateBase
{
    public override float stateExitTime => 2f;
    public override bool exitStateWhenTimeOut => true;
    public override NPCState nextState => NPCState.Idle;
    public NPCSocialEndHappyState(NPCStateMachine stateMachine, NPC npc) : base(stateMachine, npc)
    {
    }
    protected override void OnEnterState()
    {
        base.OnEnterState();
        animator.SetBool("isSocialHappy", true);
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