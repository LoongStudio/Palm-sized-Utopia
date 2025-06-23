using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    private const float REST_TIME_WEIGHT = 0.9f;    // 休息时间权重
    private const float PENDING_WORK_WEIGHT = 0.7f; // 待处理工作权重
    private const float SOCIAL_WEIGHT = 0.3f;       // 社交权重
    private const float WORK_WEIGHT = 0.4f;         // 工作权重
    private const float REST_TIME_MULTIPLIER = 2.0f;  // 在休息时间段内的回家权重倍数

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
            // DetermineNextState();
        }
    }

    private void DetermineNextState()
    {
        // 检查各种时间状态
        bool isRestTime = npc.IsRestTime();
        bool canWorkNow = npc.CanWorkNow();

        // 计算基础权重
        float restTimeWeight = isRestTime ? REST_TIME_WEIGHT * REST_TIME_MULTIPLIER : 0f;  // 只在休息时间有回家权重
        float pendingWorkWeight = (canWorkNow && npc.PendingWorkBuilding != null) ? PENDING_WORK_WEIGHT : 0f;
        float socialWeight = SOCIAL_WEIGHT;
        float newWorkWeight = canWorkNow ? WORK_WEIGHT : 0f;

        // 只有在工作时间才有工作权重
        if (!canWorkNow)
        {
            newWorkWeight = 0f;
            pendingWorkWeight = 0f;
        }

        // 计算最终权重
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
                if (showDebugInfo)
                {
                    string reason = isRestTime ? "休息时间到了" : "随机选择";
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 决定回家休息，原因：{reason}");
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
                if (showDebugInfo && !canWorkNow)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 虽然不是工作时间，但还是决定去处理待完成的工作");
                }
                npc.MoveToTarget(npc.PendingWorkBuilding.transform.position);
                stateMachine.ChangeState(NPCState.MovingToWork);
            }
        }
        else if (randomValue < homeWeight + workWeight + socialWeight)
        {
            // 社交 这里由社交系统全部接管，不进行状态切换
            if (showDebugInfo)
            {
                Debug.Log($"[NPCIdleState] {npc.data.npcName} 决定进行社交活动");
            }
        }
        else
        {
            // 只有在工作时间才会考虑找新工作
            if (canWorkNow)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 决定去找新工作");
                }
                stateMachine.ChangeState(NPCState.MovingToWork);
            }
            else if (npc.housing != null && isRestTime)  // 只在休息时间才会因为没事做而回家
            {
                // 非工作时间且是休息时间，优先回家
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 非工作时间且是休息时间，优先回家");
                }
                npc.MoveToTarget(npc.housing.transform.position);
                stateMachine.ChangeState(NPCState.MovingHome);
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