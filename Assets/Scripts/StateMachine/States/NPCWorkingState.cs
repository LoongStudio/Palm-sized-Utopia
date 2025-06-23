using UnityEngine;

public class NPCWorkingState : NPCStateBase
{
    public NPCWorkingState(NPCState stateType, NPCStateMachine stateMachine, NPC npc) 
        : base(stateType, stateMachine, npc)
    {
        stateDescription = "工作中";
        nextState = NPCState.Transporting;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        // TODO: 在此查找transferTo下家建筑（有需求且有容量），如找到则切换为运输类工作，否则为普通工作
        if (showDebugInfo)
        {
            Debug.Log($"[NPCWorkingState] {npc.data.npcName} 正在工作");
        }
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        // 检查是否到了下班时间
        if (!npc.CanWorkNow() || npc.ShouldRest())
        {
            if (showDebugInfo)
            {
                Debug.Log($"[NPCWorkingState] {npc.data.npcName} 工作时间结束，准备下班");
            }
            // 将当前AssignedBuilding设为PendingWork，方便下次继续
            if (npc.AssignedBuilding != null)
            {
                npc.SetPendingWork(npc.AssignedBuilding);
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCWorkingState] {npc.data.npcName} 将当前工作建筑设为PendingWork: {npc.AssignedBuilding.data.buildingName}");
                }
            }
            stateMachine.ChangeState(NPCState.Idle);
        }
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        
        // 清除AssignedBuilding（如果有PendingWork会在下次工作时重新分配）
        if (npc.AssignedBuilding != null)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[NPCWorkingState] {npc.data.npcName} 离开工作状态，清除已分配建筑: {npc.AssignedBuilding.data.buildingName}");
            }
            npc.AssignedBuilding = null;
        }
    }
} 