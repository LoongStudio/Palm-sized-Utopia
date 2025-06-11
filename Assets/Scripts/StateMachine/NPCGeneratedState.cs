using UnityEngine;
using System.Collections.Generic;

public class NPCGeneratedState : NPCStateBase
{
    private float searchTimeout = 5f; // 搜索超时时间
    private float searchTimer = 0f;

    public NPCGeneratedState(NPCStateMachine stateMachine, NPC npc) : base(stateMachine, npc)
    {
        stateDescription = "NPC生成状态 - 寻找住房";
        exitStateWhenTimeOut = true;
        stateExitTime = searchTimeout;
        nextState = NPCState.Idle;
    }

    protected override void OnEnterState()
    {
        base.OnEnterState();
        searchTimer = 0f;
        if (showDebugInfo)
        {
            Debug.Log($"[NPCGeneratedState] {npc.data.npcName} 开始寻找住房");
        }
        SearchForHousing();
    }

    protected override void OnUpdateState()
    {
        base.OnUpdateState();
    }

    private void SearchForHousing()
    {
        // 查找场景中所有的HousingBuilding
        HousingBuilding[] housingBuildings = Object.FindObjectsOfType<HousingBuilding>();
        
        if (housingBuildings.Length == 0)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[NPCGeneratedState] {npc.data.npcName} 没有找到任何住房建筑，将被销毁");
            }
            Object.Destroy(npc.gameObject);
            return;
        }

        // 尝试找到一个有空位的住房
        foreach (var building in housingBuildings)
        {
            if (building.RegisterLivingNPC(npc))
            {
                npc.housing = building;
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCGeneratedState] {npc.data.npcName} 成功注册到住房 {building.name}");
                }
                return;
            }
        }

        // 如果所有住房都满了
        if (showDebugInfo)
        {
            Debug.LogWarning($"[NPCGeneratedState] {npc.data.npcName} 所有住房都已满，将被销毁");
        }
        Object.Destroy(npc.gameObject);
    }

    protected override void OnExitState()
    {
        base.OnExitState();
        if (npc.housing == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"[NPCGeneratedState] {npc.data.npcName} 未能找到住房，将被销毁");
            }
            Object.Destroy(npc.gameObject);
        }
    }
}