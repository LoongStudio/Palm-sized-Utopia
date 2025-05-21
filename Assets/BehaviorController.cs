using System;
using System.Collections.Generic;
using System.Linq;
using DataType;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum NPCStates
{
    Idle,           // 0
    Harvesting,     // 1
    Walking,        // 2
    Wandering,      // 3
    Sleeping,       // 4
    Storaging,      // 5
} 

// [Serializable]
// public struct StateWeight
// {
//     public NPCStates state;
//     public int weights;
// }
// [Serializable]
// public struct StateWeights
// {
//     public NPCStates state;
//     public StateWeight[] stateWeights;
// }

public class BehaviorController : MonoBehaviour
{
    public Dictionary<Items, int> MaxItemCarry = new Dictionary<Items, int>
    {
        { Items.Crops, 10 },
        { Items.Seeds, 20 },
        { Items.Rocks, 50 },
        { Items.TreePlants, 5 }
    };

    // public StateWeights[] stateWeights;
    
    public Transform currentTarget;
    public NavMeshAgent agent;
    public Dictionary<Items, int> NPCbackpack = new Dictionary<Items, int>();
    
    public NPCStates currentNPCState = NPCStates.Wandering;
    // private Dictionary<NPCStates, Dictionary<NPCStates, float>> _stateProbabilitiesDict;
    private float _decisionTimer = 0f;
    public float decisionInterval = 1f; // 每隔多少秒尝试一次决策
    public GameObject tempPoint;
    public GameObject checkpointPrefab;
    
    // DEBUG
    public bool debugDraw = true;

    private void OnDrawGizmos()
    {
        if (!debugDraw) return;
        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTarget.position, 0.05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // // 初始化 有序列化的Dictionary
        // _stateProbabilitiesDict = new Dictionary<NPCStates, Dictionary<NPCStates, float>>();
        // foreach (var states in stateWeights)
        // {
        //     Dictionary<NPCStates, int> stateWeightDict = new Dictionary<NPCStates, int>();
        //     int totalWeight = 0;
        //     foreach (var stateWeight in states.stateWeights)
        //     {
        //         stateWeightDict.Add(stateWeight.state, stateWeight.weights);
        //         totalWeight += stateWeight.weights;
        //     }
        //     Dictionary<NPCStates, float> stateAccumulateProbabilityDict = new Dictionary<NPCStates, float>();
        //     float accumulateProbability = 0f;
        //     foreach (var npcState in Enum.GetValues(typeof(NPCStates)).Cast<NPCStates>())
        //     {
        //         if (stateWeightDict.TryGetValue(npcState, out int weight))
        //             accumulateProbability += weight / totalWeight;
        //         else
        //             accumulateProbability += 0;
        //         
        //         stateAccumulateProbabilityDict.Add(npcState, accumulateProbability);
        //     }
        //     _stateProbabilitiesDict.Add(states.state, stateAccumulateProbabilityDict);
        // }
        // // 补充不存在的定义 为0
        // Dictionary<NPCStates, float> emptyMap = new Dictionary<NPCStates, float>();
        // foreach (var npcState in Enum.GetValues(typeof(NPCStates)).Cast<NPCStates>())
        // {
        //     emptyMap.Add(npcState, 0f);
        // }
        // foreach (var npcState in Enum.GetValues(typeof(NPCStates)).Cast<NPCStates>())
        // {
        //     if (!_stateProbabilitiesDict.ContainsKey(npcState))
        //     {
        //         _stateProbabilitiesDict.Add(npcState, new Dictionary<NPCStates, float>(emptyMap));
        //     }
        // }
        // //
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
        switch (currentNPCState)
        {
            case NPCStates.Idle:
                // 间隔时间 + 概率判断
                _decisionTimer += Time.deltaTime;
                if (_decisionTimer >= decisionInterval)
                {
                    _decisionTimer = 0f;
                    float rand = Random.value; // 0 ~ 1
                    if (rand < 0.3f)
                        EnterWalkingTowardsMode(FindFirstObjectByType<CropAttributes>().transform);
                    else if (rand < 0.6f)
                        EnterWanderingMode();
                }
                break;
            case NPCStates.Harvesting:
                if (IsItemFull(Items.Crops))
                    EnterWalkingTowardsMode(FindFirstObjectByType<StorageAttributes>().transform);
                else
                    agent.isStopped = true;
                
                break;
            case NPCStates.Wandering:
                // 如果不存在 目标点 则尝试
                if (!tempPoint)
                {
                    // EnterWanderingMode();
                    float rand = Random.value; // 0 ~ 1
                    if (rand < 0.3f)
                        EnterIdleMode();
                    else if (rand < 0.6f)
                        EnterWalkingTowardsMode(FindFirstObjectByType<StorageAttributes>().transform);
                    else
                        EnterWanderingMode();
                }
                break;
            case NPCStates.Storaging:
                agent.isStopped = true;
                bool isEmpty = true;
                
                foreach (Items item in Enum.GetValues(typeof(Items)))
                {
                    int carried = GetItemAmount(item);
                    if (carried > 0) { isEmpty = false; break; }
                }
                
                if (isEmpty)
                {
                    Debug.Log("已清空，开始随机游荡");
                    EnterWanderingMode();
                }
                break;
            default:
                agent.isStopped = false;
                if (currentTarget != null)
                    agent.destination = currentTarget.position;
                break;
        }
        
        agent.destination = currentTarget.position;
    }
    
    public void EnterIdleMode()
    {
        currentNPCState = NPCStates.Idle;
        currentTarget = FindFirstObjectByType<CropAttributes>().transform;
        agent.isStopped = true;
    }
    public void EnterWalkingTowardsMode(Transform target)
    {
        currentNPCState = NPCStates.Walking;
        currentTarget = target;
        agent.isStopped = false;
    }
    
    public void EnterWanderingMode()
    {
        Debug.Log("========= [BehaviorController] EnterWanderingMode =========");
        if (!tempPoint)
        {
            List<BlockProperties> blocks = FindObjectsByType<BlockProperties>(FindObjectsSortMode.None).ToList();
            BlockProperties randomBlock = blocks[Random.Range(0, blocks.Count)];
            Debug.Log("随机游走目标方块: " + randomBlock.name);
            Tuple<Vector3, bool> result = GetRandomPointOnTopOfBlock(randomBlock);  
            int maxAttempts = 10;
            int attempts = 0;
            while (!result.Item2 && attempts < maxAttempts)
            {
                result = GetRandomPointOnTopOfBlock(randomBlock);
                attempts++;
            }
            if (result != null && result.Item2)
            {
                tempPoint = Instantiate(
                    checkpointPrefab,
                    result.Item1,
                    Quaternion.identity);
                currentTarget = tempPoint.transform;
                currentNPCState = NPCStates.Wandering;
                agent.isStopped = false;
            }
            else
            {
                Debug.LogWarning("无法在 block 顶部找到有效随机点，放弃当前漫游尝试。");
                currentTarget = gameObject.transform;
                currentNPCState = NPCStates.Idle;
                agent.isStopped = false;
            }
        }
    }
    Tuple<Vector3, bool> GetRandomPointOnTopOfBlock(BlockProperties block)
    {
        // 获取 Collider（必须存在）
        Collider col = block.GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("Block has no collider.");
            return Tuple.Create(block.transform.position, false);
        }

        Bounds bounds = col.bounds;

        float topY = bounds.max.y;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 topPoint = new Vector3(randomX, topY + 0.1f, randomZ); // 稍微高出一点避免 NavMesh 碰撞问题
        Debug.Log("尝试生成随机游走坐标：" + topPoint);
        // 可选：检测是否 NavMesh 可行走
        NavMeshHit hit;
        if (NavMesh.SamplePosition(topPoint, out hit, 1.0f, NavMesh.AllAreas))
        {
            return Tuple.Create(hit.position, true);
        }
        else
        {
            Debug.LogWarning("Random point on top is not on NavMesh.");
            return Tuple.Create(block.transform.position, false); // 回退
        }
    }
    public int GetItemAmount(Items item)
    {
        if (!NPCbackpack.ContainsKey(item))
            NPCbackpack.Add(item, 0);
        return NPCbackpack[item];
    }
    public void RemoveItem(Items item, int amount)
    {
        Assert.IsTrue(NPCbackpack[item] >= amount);
        NPCbackpack[item] -= amount;
    }
    public bool IsItemFull(Items item)
    {
        if (!NPCbackpack.ContainsKey(item))
        {
            NPCbackpack.Add(item, 0);
        }
        return MaxItemCarry[item] <= NPCbackpack[item];
    }

    public int AddItem(Items item, int amount)
    {
        if (!NPCbackpack.ContainsKey(item))
            NPCbackpack.Add(item, 0);
        int toAdd = Mathf.Min(amount, MaxItemCarry[item] - NPCbackpack[item]);
        NPCbackpack[item] += toAdd;
        return toAdd;
    }
}
