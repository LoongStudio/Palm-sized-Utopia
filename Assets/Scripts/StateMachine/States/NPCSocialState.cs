using UnityEngine;

public class NPCSocialState : NPCStateBase
{
    public NPCSocialState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "进行社交活动";
        // nextState = NPCState.SocialSettle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSocializingState] {npc.data.npcName} 正在进行社交活动");
        }
        // 控制动画
        animator.SetBool("isSocial", true);

        // 转向伙伴
        var partner = NPCManager.Instance.socialSystem.GetSocialPartner(npc);
        npc.TurnToPosition(partner.transform.position);

        // 开始进行社交互动
        GameEvents.TriggerNPCSocialInteractionStarted(new NPCEventArgs{npc = npc, otherNPC = partner});

        // 注册事件
        GameEvents.OnNPCSocialInteractionEnded += HandleSocialInteractionEnded;
    }
    protected override void OnExitState()
    {
        base.OnExitState();
        animator.SetBool("isSocial", false);
    }
    protected override void OnUpdateState()
    {
        base.OnUpdateState();

    }

    private void HandleSocialInteractionEnded(NPCEventArgs args){
        // 如果npc是互动的参与者，并且互动结果是社交结束
        if((args.npc == npc || args.otherNPC == npc) && 
        (args.shouldChangeStateTo == NPCState.SocialEndHappy 
        || args.shouldChangeStateTo == NPCState.SocialEndFight)){
            npc.ChangeState(args.shouldChangeStateTo);
        }
    }
} 