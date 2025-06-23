using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    private const float REST_TIME_WEIGHT = 0.9f;    // 休息时间权重
    private const float PENDING_WORK_WEIGHT = 0.7f; // 待处理工作权重
    private const float SOCIAL_WEIGHT = 0.3f;       // 社交权重
    private const float WORK_WEIGHT = 0.4f;         // 工作权重
    private const float REST_TIME_MULTIPLIER = 2.0f;  // 在休息时间段内的回家权重倍数
    private const float OFF_WORK_PENALTY = 0.3f;      // 非工作时间的工作权重惩罚
    private const float NEAR_REST_TIME_MULTIPLIER = 1.5f; // 接近休息时间时的回家权重倍数（比如晚上）

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
        // 检查各种时间状态
        bool isRestTime = npc.IsRestTime();
        bool canWorkNow = npc.CanWorkNow();
        bool isNearRestTime = IsNearRestTime();

        // 计算基础权重
        float restTimeWeight = REST_TIME_WEIGHT;
        float pendingWorkWeight = npc.PendingWorkBuilding != null ? PENDING_WORK_WEIGHT : 0f;
        float socialWeight = SOCIAL_WEIGHT;
        float newWorkWeight = WORK_WEIGHT;

        // 根据时间调整权重
        if (isRestTime)
        {
            // 休息时间：提高回家权重，降低其他活动权重
            restTimeWeight *= REST_TIME_MULTIPLIER;
            socialWeight *= 0.5f;
            newWorkWeight *= OFF_WORK_PENALTY;
            pendingWorkWeight *= OFF_WORK_PENALTY;
        }
        else if (isNearRestTime)
        {
            // 接近休息时间：适度提高回家权重
            restTimeWeight *= NEAR_REST_TIME_MULTIPLIER;
            socialWeight *= 0.7f;
            newWorkWeight *= 0.8f;
        }
        else if (!canWorkNow)
        {
            // 非工作时间：降低工作相关权重
            newWorkWeight *= OFF_WORK_PENALTY;
            pendingWorkWeight *= OFF_WORK_PENALTY;
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
                    string reason = isRestTime ? "休息时间到了" : 
                                  isNearRestTime ? "接近休息时间" : 
                                  "随机选择";
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
            // 在休息时间段内，优先考虑回家而不是找新工作
            if ((isRestTime || isNearRestTime) && npc.housing != null)
            {
                if (showDebugInfo)
                {
                    string reason = isRestTime ? "已经是休息时间" : "快到休息时间";
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 本想找工作，但因为{reason}，决定回家休息");
                }
                npc.MoveToTarget(npc.housing.transform.position);
                stateMachine.ChangeState(NPCState.MovingHome);
            }
            else
            {
                // 寻找新工作
                if (showDebugInfo && !canWorkNow)
                {
                    Debug.Log($"[NPCIdleState] {npc.data.npcName} 虽然不是工作时间，但还是决定去找工作");
                }
                stateMachine.ChangeState(NPCState.MovingToWork);
            }
        }

        npc.ResetIdleWeight();
    }

    // 判断是否接近休息时间（比如晚上）
    private bool IsNearRestTime()
    {
        if (TimeManager.Instance == null) return false;
        
        var currentHour = TimeManager.Instance.CurrentTime.hour;
        var restStartHour = npc.data.restTimeStart;
        
        // 如果休息时间在晚上（比如22点），则提前2小时开始增加回家倾向
        return currentHour >= (restStartHour - 2) && currentHour < restStartHour;
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