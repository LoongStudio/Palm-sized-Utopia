using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    public NPCIdleState(NPC npc) : base(npc)
    {
    }

    protected override void OnEnterState(){
        base.OnEnterState();
    }
    protected override void OnExitState(){
        base.OnExitState();
    }
    protected override void OnUpdateState(){
        base.OnUpdateState();
    }
    protected override void OnFixedUpdateState(){
        base.OnFixedUpdateState();
    }
}