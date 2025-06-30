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
        
        // 如果没有待处理的工作，寻找新的工作
        if (!npc.HasPendingWork())
        {
            var nextWork = BuildingManager.Instance.GetBestWorkBuildingWorkForNPC(npc);
            Debug.Log($"[Work] 查找最适合NPC的建筑 {nextWork.building.data.subType}");
            npc.assignedTask = nextWork;
            // 改为找到时立刻注册
            if (nextWork.building != null && nextWork.building.TryAssignNPC(npc))
            {
                Debug.Log($"[Work] 找到目标工作 {nextWork.building.data.subType}");
                // 使用NPCMovement的MoveToTarget方法，而不是直接设置currentTarget
                npc.MoveToTarget(nextWork.building.transform.position);
                
            }
            else
            {
                Debug.Log($"[Work] 找不到目标工作 进入 Idle");
                npc.assignedTask = null;
                // 如果没有找到工作，返回空闲状态
                stateMachine.ChangeState(NPCState.Idle);
                return;
            }
        }
        else
        {
            // 有待处理的工作，直接移动到目标位置 正常来说在上一个状态应该已经设置了 Assigned
            npc.MoveToTarget(npc.assignedTask.building.transform.position);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 正在前往工作地点: {npc.GetPendingWork()?.building?.data.buildingName ?? "未知"}");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到达工作地点 - 使用NPCMovement的isInPosition属性
        if (npc.isInPosition)
        {
            // 如果这是待处理的工作，设置为已分配建筑
            if (npc.HasPendingWork())
            {
                npc.assignedTask = new TaskInfo(
                    npc.GetPendingWork()?.building, 
                    npc.GetPendingWork()?.taskType ?? TaskType.None);
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCMovingToWorkState] {npc.data.npcName} 已分配到建筑: {npc.assignedTask.building.data.subType}");
                }
                npc.ClearPendingWork();
            }
            
            // 进入工作状态
            stateMachine.ChangeState(nextState);
        }
    }
} 