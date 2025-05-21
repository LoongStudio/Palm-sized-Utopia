using System;
using System.Collections.Generic;
using System.Linq;
using DataType;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public enum NPCStates
{
    Idle,
    Harvesting,
    Walking,
    Wandering,
    Sleeping,
    Storaging,
} 

public class BehaviorController : MonoBehaviour
{
    public Dictionary<Items, int> maxItemCarry = new Dictionary<Items, int>
    {
        { Items.Crops, 10 },
        { Items.Seeds, 20 },
        { Items.Rocks, 50 },
        { Items.TreePlants, 5 }
    };

    
    public Transform currentTarget;
    public NavMeshAgent agent;
    public Dictionary<Items, int> NPCbackpack = new Dictionary<Items, int>();
    
    public NPCStates currentNPCState = NPCStates.Wandering;
    private float decisionTimer = 0f;
    public float decisionInterval = 1f; // 每隔多少秒尝试一次决策
    public GameObject tempPoint;
    public GameObject checkpointPrefab;
    public 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // DEBUG
        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }
        
        switch (currentNPCState)
        {
            case NPCStates.Idle:
                agent.isStopped = true;

                // 间隔时间 + 概率判断
                decisionTimer += Time.deltaTime;
                if (decisionTimer >= decisionInterval)
                {
                    decisionTimer = 0f;

                    float rand = Random.value; // 0 ~ 1
                    if (rand < 0.3f)
                    {
                        // 30% 进入 移动
                        currentNPCState = NPCStates.Walking;
                        currentTarget = FindFirstObjectByType<CropAttributes>().transform;
                    }
                    else if (rand < 0.5f)
                    {
                        // 20% 进入 闲逛
                        EnterWanderingMode();
                    }
                    // 否则继续 Idle
                }
                break;
            case NPCStates.Harvesting:
                if (IsItemFull(Items.Crops))
                {
                    currentNPCState = NPCStates.Walking;
                    currentTarget = FindFirstObjectByType<StorageAttributes>().transform;
                    agent.isStopped = false;
                }
                else
                {
                    agent.isStopped = true;
                }
                
                break;
            case NPCStates.Wandering:
                // 如果不存在 目标点 则尝试
                if (!tempPoint)
                {
                    // EnterWanderingMode();
                    float rand = Random.value; // 0 ~ 1
                    if (rand < 0.3f)
                    {
                        // 60% 进入 移动
                        currentNPCState = NPCStates.Idle;
                        currentTarget = FindFirstObjectByType<CropAttributes>().transform;
                    }
                    else if (rand < 0.6f)
                    {
                        // 30% 进入 闲逛
                        currentNPCState = NPCStates.Walking;
                        currentTarget = FindFirstObjectByType<StorageAttributes>().transform;
                        agent.isStopped = false;
                    }
                    else
                    {
                        EnterWanderingMode();
                    }
                    
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
        return maxItemCarry[item] <= NPCbackpack[item];
    }

    public int AddItem(Items item, int amount)
    {
        if (!NPCbackpack.ContainsKey(item))
            NPCbackpack.Add(item, 0);
        int toAdd = Mathf.Min(amount, maxItemCarry[item] - NPCbackpack[item]);
        NPCbackpack[item] += toAdd;
        return toAdd;
    }
}
