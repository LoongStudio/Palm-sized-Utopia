using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    private const float REST_TIME_WEIGHT = 0.9f;    // 休息时间权重
    private const float PENDING_WORK_WEIGHT = 0.7f; // 待处理工作权重
    private const float SOCIAL_WEIGHT = 0.3f;       // 社交权重
    private const float WORK_WEIGHT = 0.4f;         // 工作权重
    private const float REST_TIME_MULTIPLIER = 2.0f; // 在休息时间段内的额外权重倍数

    public NPCIdleState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "空闲状态";
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        npc.ResetIdleWeight();
        if (showDebugInfo)
        {
            Debug.Log($"[NPCIdleState] {npc.data.npcName} 进入空闲状态");
        }
        npc.StartRandomMovement();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        npc.IncreaseIdleWeight();

        // 如果累积的权重足够高，考虑状态转换
        if (npc.CurrentIdleWeight >= 1f)
        {
            DetermineNextState();
        }
    }

    private void DetermineNextState()
    {
        // 检查是否在休息时间段内
        bool isRestTime = npc.IsRestTime();
        float restTimeWeight = isRestTime ? REST_TIME_WEIGHT * REST_TIME_MULTIPLIER : REST_TIME_WEIGHT;
        float pendingWorkWeight = npc.PendingWorkBuilding != null ? PENDING_WORK_WEIGHT : 0f;
        
        // 如果在休息时间段内，降低其他活动的权重
        float socialWeight = isRestTime ? SOCIAL_WEIGHT * 0.5f : SOCIAL_WEIGHT;
        float newWorkWeight = isRestTime ? WORK_WEIGHT * 0.3f : WORK_WEIGHT;
        
        // 计算各种状态的权重
        float homeWeight = restTimeWeight;
        float workWeight = pendingWorkWeight;

        // 根据权重选择下一个状态
        float totalWeight = homeWeight + workWeight + socialWeight + newWorkWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < homeWeight)
        {
            // 回家
            if (npc.housing != null)
            {
                if (showDebugInfo && isRestTime)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 在休息时间段内，决定回家休息");
                }
                npc.MoveToTarget(npc.housing.transform.position);
                stateMachine.ChangeState(NPCState.MovingHome);
            }
        }
        else if (randomValue < homeWeight + workWeight)
        {
            // 执行待处理的工作
            if (npc.PendingWorkBuilding != null)
            {
                npc.MoveToTarget(npc.PendingWorkBuilding.transform.position);
                stateMachine.ChangeState(NPCState.MovingToWork);
            }
        }
        else if (randomValue < homeWeight + workWeight + socialWeight)
        {
            // 社交 这里由社交系统全部接管，不进行状态切换
            // stateMachine.ChangeState(NPCState.PrepareForSocial);
        }
        else
        {
            // 在休息时间段内，优先考虑回家而不是找新工作
            if (isRestTime && npc.housing != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 本想找工作，但因为在休息时间段内，决定回家休息");
                }
                npc.MoveToTarget(npc.housing.transform.position);
                stateMachine.ChangeState(NPCState.MovingHome);
            }
            else
            {
                // 寻找新工作
                stateMachine.ChangeState(NPCState.MovingToWork);
            }
        }

        npc.ResetIdleWeight();
    }

    protected override void OnExitState()
    {
        base.OnExitState();
    }
    
    protected override void OnFixedUpdateState()
    {
        base.OnFixedUpdateState();
    }
}