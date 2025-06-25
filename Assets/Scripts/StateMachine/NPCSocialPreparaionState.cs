using System.Linq;
using UnityEngine;

public class NPCSocialPreparaionState : NPCStateBase
{
    private bool invitationSent = false;
    private int sentInvitationId = -1;
    private float waitStartTime;
    private const float MAX_WAIT_TIME = 5f; // 最大等待响应时间
    private SocialSystem socialSystem;
    public NPCSocialPreparaionState(NPCState state, NPCStateMachine stateMachine, NPC npc) : base(NPCState.PrepareForSocial, stateMachine, npc)
    {
    }
    protected override void OnEnterState()
    {
        base.OnEnterState();
        invitationSent = false;
        sentInvitationId = -1;
        waitStartTime = Time.time;
        // 播放动画
        animator.SetBool("isSocialPreparation", true);

        // 停止移动
        npc.StopRandomMovement();

        // 首先检查是否已有待处理的邀请
        CheckForExistingInvitations();

        // 如果没有处理邀请，则发送新邀请
        if (npc.currentState == NPCState.PrepareForSocial) // 确保状态没有在CheckForExistingInvitations中改变
        {
            SendNewInvitation();
        }

    }
        /// <summary>
    /// 检查是否已有待处理的邀请
    /// </summary>
    private void CheckForExistingInvitations()
    {
        var socialSystem = NPCManager.Instance.socialSystem;
        var existingInvitation = socialSystem.GetPendingInvitation(npc);
        
        if (existingInvitation != null)
        {
            // 有待处理邀请，直接处理
            ProcessIncomingInvitation(existingInvitation);
        }
    }
    /// <summary>
    /// 检查新收到的邀请
    /// </summary>
    private void CheckForIncomingInvitations()
    {
        var socialSystem = NPCManager.Instance.socialSystem;
        var newInvitation = socialSystem.GetPendingInvitation(npc);
        
        if (newInvitation != null)
        {
            ProcessIncomingInvitation(newInvitation);
        }
    }

    /// <summary>
    /// 处理收到的邀请
    /// </summary>
    private void ProcessIncomingInvitation(SocialInvitation invitation)
    {
        bool shouldAccept = ShouldAcceptInvitationInPrepareState(invitation);
        string reason = "";
        
        if (shouldAccept)
        {
            // 如果已经发送了邀请，需要先取消
            if (invitationSent)
            {
                CancelMyInvitation("接受了更好的邀请");
            }
            
            reason = "在准备社交时接受邀请";
        }
        else
        {
            reason = DetermineDeclineReason(invitation);
        }
        
        // 响应邀请
        var socialSystem = NPCManager.Instance.socialSystem;
        socialSystem.RespondToInvitation(invitation.invitationId, npc, shouldAccept, reason);
        
        if (shouldAccept)
        {
            // 接受邀请时添加社交伙伴对
            NPCManager.Instance.socialSystem.AddSocialPair(npc, invitation.sender);
            
            if (showDebugInfo)
                Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 在准备社交时接受了 {invitation.sender.data.npcName} 的邀请");
            npc.ChangeState(NPCState.MovingToSocial);
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 在准备社交时拒绝了 {invitation.sender.data.npcName} 的邀请: {reason}");
        }
    }

    /// <summary>
    /// 在PrepareForSocial状态下判断是否接受邀请
    /// </summary>
    private bool ShouldAcceptInvitationInPrepareState(SocialInvitation invitation)
    {
        // 1. 基本条件检查
        if (npc.IsRestTime())
            return false;
        
        // 3. 如果已经发送了邀请，比较优先级
        if (invitationSent)
        {
            var socialSystem = NPCManager.Instance.socialSystem;
            var myInvitation = socialSystem.activeInvitations.Values
                .FirstOrDefault(inv => inv.invitationId == sentInvitationId);
            
            if (myInvitation != null)
            {
                // 比较优先级，如果新邀请的优先级更高，则接受
                return invitation.priority > myInvitation.priority;
            }
        }
        
        return true;
    }
    /// <summary>
    /// 取消自己发送的邀请
    /// </summary>
    private void CancelMyInvitation(string reason)
    {
        if (invitationSent && sentInvitationId != -1)
        {
            var socialSystem = NPCManager.Instance.socialSystem;
            socialSystem.CancelInvitation(sentInvitationId, reason);
            
            invitationSent = false;
            sentInvitationId = -1;
            
            if (showDebugInfo)
                Debug.Log($"[NPCSocialPrepareState] {npc.data.npcName} 取消了自己的邀请: {reason}");
        }
    }
    private void SendNewInvitation()
    {
        socialSystem = NPCManager.Instance.socialSystem;
        // 向最近的社交伙伴发送社交邀请
        var partner = socialSystem.FindNearestSocialPartner(npc);
        if (partner != null)
        {
            bool sent = socialSystem.SendSocialInvitation(npc, partner);
            if (sent)
            {
                invitationSent = true;
                // 记录发送的邀请ID，用于后续取消
                var sentInvitation = socialSystem.activeInvitations.Values
                    .FirstOrDefault(inv => inv.sender == npc && inv.receiver == partner && inv.IsValid);
                
                if (sentInvitation != null)
                {
                    sentInvitationId = sentInvitation.invitationId;
                }

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
    protected override void OnUpdateState()
    {
        base.OnUpdateState();
        // 1. 优先检查是否收到新的社交邀请
        CheckForIncomingInvitations();

        // 2. 检查已发送邀请的响应
        if (invitationSent)
        {
            CheckForInvitationResponse();
        }

    }

    private void CheckForInvitationResponse()
    {
        var response = socialSystem.GetInvitationResponse(npc);
        if (response != null)
        {
                    if (response.accepted)
        {
            // 邀请被接受，添加社交伙伴对
            socialSystem.AddSocialPair(npc, response.responder);
            
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
    private string DetermineDeclineReason(SocialInvitation invitation)
    {
        if (npc.IsRestTime())
            return "休息时间";
        
        if (invitationSent)
            return "已经向其他人发出邀请";
        
        return "其他原因";
    }
}