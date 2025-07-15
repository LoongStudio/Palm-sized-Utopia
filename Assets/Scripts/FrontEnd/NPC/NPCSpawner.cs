using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NPC生成器 - 订阅雇佣事件并在合适的位置生成NPC
/// </summary>
public class NPCSpawner : SingletonManager<NPCSpawner>
{
    [Header("生成设置")]
    [SerializeField] private GameObject npcPrefab;                    // NPC预制体
    [SerializeField] private NPCMovementConfig npcMovementConfig;    // NPC移动配置
    [SerializeField] private float spawnRadius = 20f;                 // 生成搜索半径
    [SerializeField] private float spawnHeight = 10f;                 // 生成高度（屏幕外）
    [SerializeField] private int maxSpawnAttempts = 30;               // 最大生成尝试次数
    [SerializeField] private Transform spawnCenter;                  // 生成中心点
    [SerializeField] private Transform spawnPoint;                   // 生成点
    [SerializeField] private bool spawnAtSpawnPoint = false;         // 是否在生成点生成

    [Header("生成缓存")]
    [SerializeField] private List<NPC> npcList = new List<NPC>();
    [SerializeField] private List<GameObject> npcObjectList = new List<GameObject>();
    private int totalNPCsToSpawn = 0;
    private int totalSpawned = 0;
    private List<Vector3> lastValidSpawnPoints = new List<Vector3>();
    
    [Header("移动设置")]
    [SerializeField] private float defaultMoveSpeed = 3.5f;           // 默认移动速度
    [SerializeField] private float defaultStoppingDistance = 0.5f;    // 默认停止距离
    [SerializeField] private float defaultAcceleration = 8f;          // 默认加速度
    
    [Header("随机移动设置")]
    // TODO: 处理这些配置的使用
    // [SerializeField] private bool enableRandomMovement = true;        // 启用随机移动
    // [SerializeField] private float randomMoveRadius = 8f;             // 随机移动半径
    [SerializeField] private Vector2 randomWaitTimeRange = new Vector2(2f, 5f); // 等待时间范围
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;               // 显示调试信息
    [SerializeField] private bool drawGizmos = true;                  // 绘制调试图形
    [SerializeField] private Color spawnAreaColor = Color.yellow;     // 生成区域颜色
    [SerializeField] private Color validSpawnPointColor = Color.green; // 有效生成点颜色
    

    
    // 组件类型字典 - 用于自动挂载组件
    private Dictionary<System.Type, bool> requiredComponents = new Dictionary<System.Type, bool>
    {
        { typeof(NPC), true },
        { typeof(NavMeshAgent), true },
        { typeof(NPCMovement), true }  // 可选组件
        // TODO: 添加其他组件
    };
    
    #region Unity生命周期
    
    protected override void Awake()
    {
        base.Awake();
        // 设置生成中心点
        if (spawnCenter == null)
        {
            spawnCenter = transform;
        }
    }
    private void OnEnable()
    {
        SubscribeToEvents();
    }
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
    
    private void Start()
    {   
        ValidateSettings();
    }
    
    private void OnDestroy()
    {
    }
    
    #endregion
    
    #region 事件订阅
    
    private void SubscribeToEvents()
    {
        GameEvents.OnNPCHired += OnNPCHired;
        GameEvents.OnNPCLoadedFromData += OnNPCLoadedFromData;
        if (showDebugInfo)
        {
            Debug.Log("[NPCSpawner] 已订阅NPC事件");
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnNPCHired -= OnNPCHired;
        GameEvents.OnNPCLoadedFromData -= OnNPCLoadedFromData;
    }
    
    #endregion
    
    #region NPC生成
    
    /// <summary>
    /// 响应NPC雇佣事件
    /// </summary>
    private void OnNPCHired(NPCEventArgs args)
    {
        if (args.npcData == null)
        {
            Debug.LogError("[NPCSpawner] 收到空的NPC雇佣事件");
            return;
        }
        
        StartCoroutine(SpawnNPCCoroutine(args.npcData, args.inventorySaveData));
    }

    private void OnNPCLoadedFromData(NPCEventArgs args)
    {
        // 事件验证
        if(args.eventType != NPCEventArgs.NPCEventType.LoadedFromData)
        {
            Debug.LogWarning("[NPCSpawner] 收到的事件类型不正确");
            return;
        }
        // 数据验证
        var npcInstancesList = args.npcInstancesList;
        if (npcInstancesList == null || npcInstancesList.Count == 0)
        {
            Debug.LogWarning("[NPCSpawner] 收到空的NPC存储数据加载事件");
            return;
        }
        // 清空缓存
        ClearSpawnCache();
        totalNPCsToSpawn = npcInstancesList.Count;
        // 开始生成NPC
        StartCoroutine(SpawnNPCsFromLoadList(npcInstancesList));

    }
    private IEnumerator SpawnNPCsFromLoadList(List<NPCInstanceSaveData> npcInstancesList)
    {
        foreach (var npcInstance in npcInstancesList)
        {
            SpawnNPC(npcInstance.npcData, npcInstance.inventorySaveData);
        }

        // 等待所有NPC生成完成
        while (totalSpawned < totalNPCsToSpawn)
        {
            yield return new WaitForSeconds(0.1f);
        }
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSpawner] 所有NPC生成完成，总生成数: {totalSpawned}");
        }
        // 触发NPC生成完成事件
        GameEvents.TriggerNPCCreatedFromList(new NPCEventArgs()
        {
            npcList = npcList,
            eventType = NPCEventArgs.NPCEventType.CreatedFromList,
            timestamp = System.DateTime.Now
        });
        // 清空缓存
        ClearSpawnCache();
    }
    public void SpawnNPC(NPCData npcData, InventorySaveData inventorySaveData)
    {
        StartCoroutine(SpawnNPCCoroutine(npcData, inventorySaveData));
    }
    
    /// <summary>
    /// 生成NPC的协程
    /// </summary>
    private IEnumerator SpawnNPCCoroutine(NPCData npcData, InventorySaveData inventorySaveData)
    {
        Vector3 spawnPosition;
        if (spawnAtSpawnPoint)
        {
            spawnPosition = spawnPoint.position + Vector3.up * spawnHeight;
        }
        else
        {
            spawnPosition = FindValidSpawnPosition();
        }

        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("[NPCSpawner] 无法找到有效的生成位置！");
            yield break;
        }

        // 创建NPC GameObject
        GameObject npcObject = CreateNPCGameObject(npcData, inventorySaveData, spawnPosition);

        if (npcObject != null)
        {
            // 等待一帧确保所有组件都已初始化
            yield return new WaitForEndOfFrame();

            // TODO:这段无效，让NPC降落到地面
            yield return StartCoroutine(DropNPCToGround(npcObject));

            totalSpawned++;

            if (showDebugInfo)
            {
                Debug.Log($"[NPCSpawner] 成功生成NPC: {npcData.npcName}，总生成数: {totalSpawned}");
            }
            npcList.Add(npcObject.GetComponent<NPC>());
            npcObjectList.Add(npcObject);
        }
    }

    private void ClearSpawnCache()
    {
        npcList.Clear();
        npcObjectList.Clear();
        totalNPCsToSpawn = 0;
        totalSpawned = 0;
        lastValidSpawnPoints.Clear();
    }
    
    /// <summary>
    /// 寻找有效的生成位置
    /// </summary>
    private Vector3 FindValidSpawnPosition()
    {
        lastValidSpawnPoints.Clear();

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // 在生成半径内生成随机点
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 groundPosition = spawnCenter.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // 假设 "Walkable" 是你的可行走区域名称
            int walkableAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
            // 仅采样 Walkable 区域
            if (NavMesh.SamplePosition(groundPosition, out NavMeshHit hit, 2f, walkableAreaMask))
            {
                // 无需额外检查，该位置必定在 Walkable 区域内
                Vector3 spawnPosition = hit.position + Vector3.up * spawnHeight;
                lastValidSpawnPoints.Add(hit.position);
    
                if (showDebugInfo)
                {
                    Debug.Log($"[NPCSpawner] 找到有效生成位置: {hit.position} (尝试 {i + 1} 次)");
                }
    
                return spawnPosition;
            }
        }

        Debug.LogWarning($"[NPCSpawner] 在 {maxSpawnAttempts} 次尝试后无法找到有效生成位置");
        return Vector3.zero;
    }
    
    /// <summary>
    /// 验证位置是否适合导航
    /// </summary>
    private bool IsPositionNavigable(Vector3 position)
    {
        // 创建临时NavMeshAgent测试导航能力
        GameObject testObject = new GameObject("NavTest");
        testObject.transform.position = position;
        
        NavMeshAgent testAgent = testObject.AddComponent<NavMeshAgent>();
        
        bool isNavigable = false;
        
        try
        {
            // 检查Agent是否在NavMesh上
            if (testAgent.isOnNavMesh)
            {
                // 尝试计算到附近几个点的路径
                Vector3[] testDestinations = {
                    position + Vector3.forward * 2f,
                    position + Vector3.back * 2f,
                    position + Vector3.left * 2f,
                    position + Vector3.right * 2f
                };
                
                int validPaths = 0;
                foreach (var destination in testDestinations)
                {
                    if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    {
                        NavMeshPath path = new NavMeshPath();
                        if (testAgent.CalculatePath(hit.position, path) && 
                            path.status == NavMeshPathStatus.PathComplete)
                        {
                            validPaths++;
                        }
                    }
                }
                
                // 如果至少有一半的路径是有效的，认为这个位置可导航
                isNavigable = validPaths >= testDestinations.Length / 2;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NPCSpawner] 测试导航位置时出错: {e.Message}");
        }
        finally
        {
            // 清理测试对象
            DestroyImmediate(testObject);
        }
        
        return isNavigable;
    }
    
    /// <summary>
    /// 创建NPC GameObject
    /// </summary>
    private GameObject CreateNPCGameObject(NPCData npcData, InventorySaveData inventorySaveData, Vector3 spawnPosition)
    {
        GameObject npcObject;
        
        // 如果有预制体，使用预制体实例化
        if (npcPrefab != null)
        {
            npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
            npcObject.name = $"NPC_{npcData.npcName}";
        }
        else
        {
            // 如果没有预制体，创建基础GameObject
            npcObject = new GameObject($"NPC_{npcData.npcName}");
            npcObject.transform.position = spawnPosition;
            
            // 创建基础视觉表示
            CreateBasicNPCVisual(npcObject);
        }
        
        // 挂载必要的组件
        SetupNPCComponents(npcObject, npcData);

        // 触发NPC实例化事件
        Debug.Log($"[NPCSpawner] 触发NPC实例化事件: {npcObject.name}");
        
        NPCEventArgs eventArgs = new NPCEventArgs(){
            npc = npcObject.GetComponent<NPC>(),
            inventorySaveData = inventorySaveData,
            eventType = NPCEventArgs.NPCEventType.Instantiated,
            timestamp = System.DateTime.Now
        };
        GameEvents.TriggerNPCInstantiated(eventArgs);

        
        return npcObject;
    }
    
    /// <summary>
    /// 创建基础NPC视觉表示
    /// </summary>
    private void CreateBasicNPCVisual(GameObject npcObject)
    {
        // 创建胶囊体作为视觉表示
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(npcObject.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        visual.name = "Visual";
        
        // 设置随机颜色
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f)
            );
            renderer.material = material;
        }
        
        // 移除视觉对象的碰撞器（会与父对象的碰撞器冲突）
        var visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            DestroyImmediate(visualCollider);
        }
        
        // 为主对象添加碰撞器
        var mainCollider = npcObject.AddComponent<CapsuleCollider>();
        mainCollider.height = 2f;
        mainCollider.radius = 0.4f;
        mainCollider.center = Vector3.up;
    }
    
    /// <summary>
    /// 设置NPC组件
    /// </summary>
    private void SetupNPCComponents(GameObject npcObject, NPCData npcData)
    {
        // 1. 设置NPC组件
        NPC npcComponent = npcObject.GetComponent<NPC>();
        if (npcComponent == null)
        {
            npcComponent = npcObject.AddComponent<NPC>();
        }
        
        // 复制原始NPC的数据
        npcComponent.SetData(npcData);

        
        // // 2. 设置NavMeshAgent
        // NavMeshAgent navAgent = npcObject.GetComponent<NavMeshAgent>();
        // if (navAgent == null)
        // {
        //     navAgent = npcObject.AddComponent<NavMeshAgent>();
        //     Debug.Log($"[NPCSpawner] 添加NavMeshAgent组件: {npcObject.name}");
        // }
        

        // // 3. 设置NPCMovement
        // NPCMovement movement = npcObject.GetComponent<NPCMovement>();
        // if (movement == null)
        // {
        //     movement = npcObject.AddComponent<NPCMovement>();
        //     Debug.Log($"[NPCSpawner] 添加NPCMovement组件: {npcObject.name}");
        // }
        // // TODO: 设置NPCMovement
        // movement.npcMovementConfig = npcMovementConfig;

        // 4. 添加可选组件
        SetupOptionalComponents(npcObject);
        
    }
    
    /// <summary>
    /// 配置NavMeshAgent
    /// </summary>
    private void ConfigureNavMeshAgent(NavMeshAgent navAgent)
    {
        navAgent.speed = defaultMoveSpeed;
        navAgent.stoppingDistance = defaultStoppingDistance;
        navAgent.acceleration = defaultAcceleration;
        navAgent.angularSpeed = 120f;
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        navAgent.radius = 0.4f;
        navAgent.height = 2f;
        navAgent.baseOffset = 0f;
    }
    
    /// <summary>
    /// 设置可选组件
    /// </summary>
    private void SetupOptionalComponents(GameObject npcObject)
    {
        // TODO: 设置可选组件


    }
    
    /// <summary>
    /// 让NPC从空中降落到地面
    /// </summary>
    private IEnumerator DropNPCToGround(GameObject npcObject)
    {
        if(npcObject == null)
        {
            yield break;
        }
        var navAgent = npcObject.GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            yield break;
        }
        
        // 暂时禁用NavMeshAgent避免干扰降落
        navAgent.enabled = false;
        
        // 计算目标地面位置
        Vector3 groundPosition = npcObject.transform.position;
        groundPosition.y = 0;
        int walkableAreaMask = 1 << NavMesh.GetAreaFromName("Walkable");
        if (NavMesh.SamplePosition(groundPosition, out NavMeshHit hit, spawnHeight, walkableAreaMask))
        {
            groundPosition = hit.position;
        }
        
        // 执行降落动画
        float dropDuration = 1f;
        Vector3 startPosition = npcObject.transform.position;
        float elapsed = 0f;
        
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            
            // 使用缓动函数使降落更自然
            t = Mathf.SmoothStep(0f, 1f, t);
            
            npcObject.transform.position = Vector3.Lerp(startPosition, groundPosition, t);
            yield return null;
        }
        
        // 确保最终位置正确
        npcObject.transform.position = groundPosition;
        
        // 重新启用NavMeshAgent
        navAgent.enabled = true;

        // 设置NPC为落地状态
        var npc = npcObject.GetComponent<NPC>();
        if (npc != null)
        {
            npc.SetLanded(true);
        }
        
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCSpawner] NPC {npcObject.name} 已降落到地面位置 {groundPosition}");
        }
    }
    
    #endregion
    
    #region 设置验证
    
    private void ValidateSettings()
    {
        if (spawnRadius <= 0)
        {
            Debug.LogWarning("[NPCSpawner] 生成半径必须大于0");
            spawnRadius = 20f;
        }
        
        if (spawnHeight <= 0)
        {
            Debug.LogWarning("[NPCSpawner] 生成高度必须大于0");
            spawnHeight = 10f;
        }
        
        if (maxSpawnAttempts <= 0)
        {
            Debug.LogWarning("[NPCSpawner] 最大尝试次数必须大于0");
            maxSpawnAttempts = 30;
        }
        
        if (randomWaitTimeRange.x >= randomWaitTimeRange.y)
        {
            Debug.LogWarning("[NPCSpawner] 等待时间范围设置不正确");
            randomWaitTimeRange = new Vector2(2f, 5f);
        }
    }
    
    #endregion
    
    #region 调试方法
    
    [ContextMenu("测试生成位置")]
    public void TestSpawnPosition()
    {
        Vector3 testPosition = FindValidSpawnPosition();
        if (testPosition != Vector3.zero)
        {
            Debug.Log($"[NPCSpawner] 测试生成位置: {testPosition}");
            
            // 在Scene视图中创建临时标记
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = testPosition;
            marker.name = "SpawnTest_Marker";
            marker.GetComponent<Renderer>().material.color = Color.red;
            
            // 5秒后自动删除标记
            StartCoroutine(DestroyMarkerAfterDelay(marker, 5f));
        }
        else
        {
            Debug.LogError("[NPCSpawner] 无法找到测试生成位置");
        }
    }
    
    private IEnumerator DestroyMarkerAfterDelay(GameObject marker, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (marker != null)
        {
            DestroyImmediate(marker);
        }
    }
    
    [ContextMenu("显示生成统计")]
    public void ShowSpawnStatistics()
    {
        Debug.Log("=== NPC生成器统计 ===");
        Debug.Log($"总生成数量: {totalSpawned}");
        Debug.Log($"最后有效生成点数量: {lastValidSpawnPoints.Count}");
        Debug.Log($"生成半径: {spawnRadius}m");
        Debug.Log($"生成高度: {spawnHeight}m");
    }
    
    #endregion
    
    #region Gizmos绘制
    
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
        // 绘制生成区域
        Gizmos.color = spawnAreaColor;
        if (spawnCenter != null)
        {
            GizmoExtensions.DrawWireCircle(spawnCenter.position, spawnRadius);
        }
        else
        {
            GizmoExtensions.DrawWireCircle(transform.position, spawnRadius);
        }
        
        // 绘制最后的有效生成点
        Gizmos.color = validSpawnPointColor;
        foreach (var point in lastValidSpawnPoints)
        {
            Gizmos.DrawWireSphere(point, 0.5f);
            Gizmos.DrawLine(point, point + Vector3.up * spawnHeight);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 选中时绘制更详细的信息
        if (spawnCenter != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(spawnCenter.position, Vector3.one);
        }
    }
    
    #endregion
    
    #region 公共接口
    
    /// <summary>
    /// 设置生成半径
    /// </summary>
    public void SetSpawnRadius(float radius)
    {
        spawnRadius = Mathf.Max(1f, radius);
    }
    
    /// <summary>
    /// 设置生成高度
    /// </summary>
    public void SetSpawnHeight(float height)
    {
        spawnHeight = Mathf.Max(1f, height);
    }
    
    /// <summary>
    /// 设置生成中心点
    /// </summary>
    public void SetSpawnCenter(Transform center)
    {
        spawnCenter = center;
    }
    
    /// <summary>
    /// 获取生成统计信息
    /// </summary>
    public int GetTotalSpawned()
    {
        return totalSpawned;
    }
    
    #endregion
}