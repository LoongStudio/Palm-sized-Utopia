using UnityEngine;

public class NPCMovingToWorkState : NPCStateBase
{
    public NPCMovingToWorkState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "前往工作地点";
        nextState = NPCState.Working;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        
        // 如果存在锁定的工作
        if (npc.IsLocked)
        {
            npc.AssignTask(npc.lockedTask);
            npc.MoveToTarget(npc.AssignedTask.building.transform.position);
            return;
        }
        
        // 如果没有待处理的工作，寻找新的工作
        if (!npc.HasPendingTask())
        {
            var nextWork = BuildingManager.Instance.GetBestWorkBuildingWorkForNPC(npc);
            // Debug.Log($"[Work] 查找最适合NPC的建筑 {nextWork.building.data.subType}");
            npc.AssignTask(nextWork);
            // 改为找到时立刻注册
            if (nextWork.building != null && nextWork.building.TryAssignNPC(npc))
            {
                Debug.Log($"[Work] {npc.data.npcName} 找到目标工作 {nextWork.building.data.subType}");
                // 使用NPCMovement的MoveToTarget方法，而不是直接设置currentTarget
                npc.MoveToTarget(nextWork.building.transform.position);
                
            }
            else
            {
                Debug.Log($"[Work] {npc.data.npcName} 找不到目标工作 进入 Idle");
                npc.AssignTask(TaskInfo.GetNone());
                // 如果没有找到工作，返回空闲状态
                stateMachine.ChangeState(NPCState.Idle);
                return;
            }
        }
        // 如果存在待处理的工作
        else
        {
            npc.AssignTask(npc.pendingTask);
            // 但是分配失败，则清除待处理工作，并返回空闲状态
            if(!npc.AssignedTask.building.TryAssignNPC(npc)){
                npc.ClearPendingWork();
                npc.AssignTask(TaskInfo.GetNone());
                stateMachine.ChangeState(NPCState.Idle);
                return;
            }
            // 成功分配待处理的工作，直接移动到目标位置 正常来说在上一个状态应该已经设置了 Assigned
            npc.MoveToTarget(npc.AssignedTask.building.transform.position);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点: {npc.GetPendingTask()?.building?.data.buildingName ?? "未知"}");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达工作地点 - 使用NPCMovement的isInPosition属性
        if (npc.isInPosition)
        {
            // 如果这是待处理的工作，移除待处理
            if (npc.pendingTask != null)
                npc.pendingTask = null;
            // 进入工作状态
            stateMachine.ChangeState(nextState);
        }
    }
} 