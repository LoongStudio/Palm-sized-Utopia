using UnityEngine;

public class NPCSocialPreparaionState : NPCStateBase
{
    private bool invitationSent = false;
    private float waitStartTime;
    private const float MAX_WAIT_TIME = 5f; // 最大等待响应时间
    private SocialSystem socialSystem;
    public NPCSocialPreparaionState(NPCState state, NPCStateMachine stateMachine, NPC npc) : base(NPCState.PrepareForSocial, stateMachine, npc)
    {
    }
    protected override void OnEnterState(){
        base.OnEnterState();
        invitationSent = false;
        waitStartTime = Time.time;
        // 播放动画
        animator.SetBool("isSocialPreparation", true);

        // 停止移动
        npc.StopRandomMovement();

        socialSystem = NPCManager.Instance.socialSystem;
        // 向最近的社交伙伴发送社交邀请
        var partner = socialSystem.FindNearestSocialPartner(npc);
        if (partner != null){
            bool sent = socialSystem.SendSocialInvitation(npc, partner);
            if (sent)
            {
                invitationSent = true;
                
                // 计算并设置发送者的社交位置
                var socialPositions = socialSystem.CalculateSocialPositions(npc, partner);
                npc.socialPosition = socialPositions.npc1Position;
                
                if (showDebugInfo)
                    Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 已向 {partner.data.npcName} 发送社交邀请，设置社交位置为 {npc.socialPosition}");
            }
            else
            {
                // 发送失败，回到空闲状态
                npc.ChangeState(NPCState.Idle);
            }
        }
        else
        {
            // 没有找到合适伙伴，回到空闲状态
            npc.ChangeState(NPCState.Idle);
        }
 
    }
    protected override void OnExitState(){
        base.OnExitState();
        animator.SetBool("isSocialPreparation", false);
    }
    protected override void OnUpdateState(){
        base.OnUpdateState();
        if (!invitationSent)
            return;

        // 检查是否收到响应
        var response = socialSystem.GetInvitationResponse(npc);
        if (response != null)
        {
            if (response.accepted)
            {
                // 邀请被接受，进入移动状态
                if (showDebugInfo)
                    Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 的邀请被 {response.responder.data.npcName} 接受");
                npc.ChangeState(NPCState.MovingToSocial);
            }
            else
            {
                // 邀请被拒绝，回到空闲状态
                if (showDebugInfo)
                    Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 的邀请被 {response.responder.data.npcName} 拒绝: {response.reason}");
                npc.ChangeState(NPCState.Idle);
            }
        }
        else if (Time.time - waitStartTime > MAX_WAIT_TIME)
        {
            // 等待超时，回到空闲状态
            if (showDebugInfo)
                Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 等待响应超时");
            npc.ChangeState(NPCState.Idle);
        }
    }
    
}