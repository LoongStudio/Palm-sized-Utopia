using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPC))]
[RequireComponent(typeof(NavMeshAgent))]
public class NPCPrefabSetup : MonoBehaviour
{
    private void Awake()
    {
        // 获取或添加必要的组件
        NPC npc = GetComponent<NPC>();
        NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
        Animator animator = GetComponent<Animator>();
        
        // 如果没有Animator组件，添加一个
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
        
        // 添加状态机组件
        NPCStateMachine stateMachine = GetComponent<NPCStateMachine>();
        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<NPCStateMachine>();
        }
        
        // 配置NavMeshAgent
        navAgent.speed = 3.5f;
        navAgent.acceleration = 8f;
        navAgent.angularSpeed = 120f;
        navAgent.stoppingDistance = 0.5f;
        navAgent.autoBraking = true;
        navAgent.radius = 0.5f;
        navAgent.height = 2f;
        
        // 设置NPC的引用
        npc.navAgent = navAgent;
        
        // 销毁此组件，因为它的工作已经完成
        Destroy(this);
    }
} 