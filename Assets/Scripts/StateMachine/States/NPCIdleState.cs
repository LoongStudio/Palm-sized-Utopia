using UnityEngine;

public class NPCIdleState : NPCStateBase
{
    private const float REST_TIME_WEIGHT = 0.9f;    // 休息时间权重
    private const float PENDING_WORK_WEIGHT = 0.7f; // 待处理工作权重
    private const float SOCIAL_WEIGHT = 0.3f;       // 社交权重
    private const float WORK_WEIGHT = 0.4f;         // 工作权重

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
        float restTimeWeight = npc.IsRestTime() ? REST_TIME_WEIGHT : 0f;
        float pendingWorkWeight = npc.PendingWorkBuilding != null ? PENDING_WORK_WEIGHT : 0f;
        
        // 计算各种状态的权重
        float homeWeight = restTimeWeight;
        float workWeight = pendingWorkWeight;
        float socialWeight = SOCIAL_WEIGHT;
        float newWorkWeight = WORK_WEIGHT;

        // 根据权重选择下一个状态
        float totalWeight = homeWeight + workWeight + socialWeight + newWorkWeight;
        float randomValue = Random.Range(0f, totalWeight);

        if (randomValue < homeWeight)
        {
            // 回家
            if (npc.housing != null)
            {
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
            // 社交
            stateMachine.ChangeState(NPCState.PrepareForSocial);
        }
        else
        {
            // 寻找新工作
            // TODO: 实现寻找新工作的逻辑
            stateMachine.ChangeState(NPCState.MovingToWork);
        }

        npc.ResetIdleWeight();
    }

    protected override void OnExitState(){
        base.OnExitState();
    }
    protected override void OnFixedUpdateState(){
        base.OnFixedUpdateState();
    }
}