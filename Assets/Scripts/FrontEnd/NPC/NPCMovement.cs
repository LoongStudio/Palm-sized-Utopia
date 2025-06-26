using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// NPC随机移动控制器 - 用于测试社交系统
/// 让NPC在NavMesh区域内随机移动，并在到达目标后暂停一段时间
/// </summary>
[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(NPC))]
public class NPCMovement : MonoBehaviour
{
    [Header("NPC移动配置")]
    public NPCMovementConfig npcMovementConfig;    // NPC移动配置

    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;           // 显示调试信息
    [SerializeField] private bool drawGizmos = true;              // 绘制调试线条
    [SerializeField] private Color pathColor = Color.green;       // 路径颜色
    [SerializeField] private bool isRandomMovementEnabled = true; // 是否开启随机移动

    [Header("移动设置")]
    [SerializeField] private float moveRadius = 3f;              // 移动半径
    [SerializeField] private float minWaitTime = 1f;              // 最短等待时间
    [SerializeField] private float maxWaitTime = 2f;              // 最长等待时间
    [SerializeField] private float movementSpeed = 2f;          // 移动速度
    [SerializeField] private float stoppingDistance = 0.5f;       // 到达目标的停止距离
    
    [Header("转向设置")]
    [SerializeField] public float turnSpeed = 5f;              // 转向速度
    [SerializeField] public float turnThreshold = 10f;         // 转向阈值角度
    [SerializeField] public bool enableTurnBeforeMove = true;  // 是否在移动前转向
    
    public bool isTurning = false;                             // 是否正在转向
    public Vector3 targetDirection;                            // 目标方向
    public Transform currentTarget;                            // 当前目标位置
    
    [Header("组件引用")]
    private Animator animator;
    private NavMeshAgent navAgent;
    private NPC npc;
    
    // 状态变量
    private Vector3 currentTargetPosition;
    private bool isMoving = false;
    private bool isWaiting = false;
    public bool isLanded = false;
    public bool isInPosition = false;
    private Coroutine movementCoroutine;
    
    // 统计信息
    private int totalMoves = 0;
    private float totalMoveTime = 0f;
    private Vector3 startPosition;
    

    // 属性获取
    public float StoppingDistance => stoppingDistance;
    public float MoveRadius => moveRadius;
    public float MinWaitTime => minWaitTime;
    public float MaxWaitTime => maxWaitTime;
    public float MovementSpeed => movementSpeed;
    public bool IsTurning => isTurning;
    public Transform CurrentTarget => currentTarget;



    private void Awake()
    {
        // 获取NavMeshAgent组件
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // 获取NPC组件（如果存在）
        npc = GetComponent<NPC>();
        
        // 获取Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[NPCMovement] {name} 没有Animator组件");
        }
        
        // 记录起始位置
        startPosition = transform.position;
    }
    
    private void Start()
    {
        // 延迟初始化，确保NavMeshAgent准备好
        StartCoroutine(DelayedInitialization());
    }
    
    private IEnumerator DelayedInitialization()
    {
        // 等待NavMeshAgent准备好
        yield return new WaitForEndOfFrame();
        
        // 检查NavMeshAgent是否有效且启用
        while (navAgent == null || !navAgent.enabled || !navAgent.isActiveAndEnabled)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // 等待NavMeshAgent被放置在NavMesh上
        int maxWaitAttempts = 50; // 最多等待5秒
        int attempts = 0;
        while (!navAgent.isOnNavMesh && attempts < maxWaitAttempts)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }
        
        if (!navAgent.isOnNavMesh)
        {
            Debug.LogError($"[NPCMovement] {name} NavMeshAgent在5秒后仍未就绪，停止初始化");
            enabled = false;
            yield break;
        }
        
        // 现在可以安全地初始化
        InitializeNavAgent();
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 延迟初始化完成，开始随机移动");
        // 注意：不在这里调用StartRandomMovement，让NPCSpawner控制何时开始移动
    }
    
    private void InitializeNavAgent()
    {
        // 配置NavMeshAgent
        navAgent.speed = npcMovementConfig.moveSpeed;
        navAgent.stoppingDistance = npcMovementConfig.stoppingDistance;
        navAgent.angularSpeed = npcMovementConfig.turnSpeed;
        navAgent.acceleration = npcMovementConfig.acceleration;
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        // navAgent.isStopped = true; // 默认停止移动

        // 设置移动属性
        stoppingDistance = npcMovementConfig.stoppingDistance;
        moveRadius = npcMovementConfig.moveRadius;
        minWaitTime = npcMovementConfig.minWaitTime;
        maxWaitTime = npcMovementConfig.maxWaitTime;
        movementSpeed = npcMovementConfig.moveSpeed;

        // 设置转向属性
        turnSpeed = npcMovementConfig.turnSpeed;
        turnThreshold = npcMovementConfig.turnThreshold;
        enableTurnBeforeMove = npcMovementConfig.enableTurnBeforeMove;

        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} NavMeshAgent配置完成");
    }
    
    public void StartRandomMovement()
    {
        // 检查NavMeshAgent是否就绪
        if (navAgent == null || !navAgent.enabled || !navAgent.isActiveAndEnabled || !navAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPCMovement] {name} NavMeshAgent未就绪，无法开始随机移动");
            return;
        }
        
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        // TODO: 这里需要根据NPC的配置来决定是否开启随机移动
        movementCoroutine = StartCoroutine(RandomMovementLoop());
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 开始随机移动");
    }
    
    public void StopRandomMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.ResetPath();
        }
        
        isMoving = false;
        isWaiting = false;
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 停止随机移动");
    }
    
    private IEnumerator RandomMovementLoop()
    {
        while (true)
        {
            // 检查是否开启随机移动
            if (!isRandomMovementEnabled)
            {
                navAgent.SetDestination(transform.position);
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            // 生成随机目标点
            Vector3 randomTarget = GenerateRandomTarget();
            
            if (randomTarget != Vector3.zero)
            {
                // 移动到目标点
                yield return StartCoroutine(MoveToTargetCoroutine(randomTarget));
                
                // 到达后等待
                yield return StartCoroutine(WaitAtTarget());
            }
            else
            {
                // 如果无法生成有效目标，等待一小段时间后重试
                Debug.LogWarning($"[NPCMovement] {name} 无法生成有效目标，等待重试...");
                yield return new WaitForSeconds(1f);
            }
        }
    }
    
    private Vector3 GenerateRandomTarget()
    {
        // 检查NavMeshAgent是否就绪
        if (navAgent == null || !navAgent.enabled || !navAgent.isActiveAndEnabled || !navAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPCMovement] {name} NavMeshAgent未就绪，无法生成目标点");
            return Vector3.zero;
        }
        
        int maxAttempts = 30;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // 在移动半径内生成随机点
            Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
            Vector3 randomPoint = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // 检查是否在NavMesh上
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            {
                // 确保路径可达 - 再次检查NavMeshAgent状态
                if (navAgent.enabled && navAgent.isActiveAndEnabled && navAgent.isOnNavMesh)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (navAgent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        if (showDebugInfo)
                            Debug.Log($"[NPCMovement] {name} 生成目标点: {hit.position} (尝试 {i + 1} 次)");
                        return hit.position;
                    }
                }
            }
        }
        
        Debug.LogWarning($"[NPCMovement] {name} 在 {maxAttempts} 次尝试后无法生成有效目标点");
        return Vector3.zero;
    }
    
    public void MoveToTarget(Vector3 target){
        if(navAgent == null || target == null) return;
        if(movementCoroutine != null){
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(MoveToTargetCoroutine(target));
    }
    private IEnumerator MoveToTargetCoroutine(Vector3 target, float speed = 0.5f)
    {
        currentTargetPosition = target;
        isMoving = true;
        float moveStartTime = Time.time;
        isInPosition = false;
        // 记录原速度，设置新速度，允许移动
        float previousSpeed = navAgent.speed;
        navAgent.speed = speed;
        navAgent.isStopped = false;

        // 设置NavMeshAgent目标
        navAgent.SetDestination(target);
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 开始移动到 {target}");
        
        // 等待到达目标
        while (navAgent.pathPending || navAgent.remainingDistance > stoppingDistance)
        {
            // 检查是否卡住或路径失败
            if (navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"[NPCMovement] {name} 路径无效，停止移动");
                break;
            }
            
            // 如果长时间没有移动，认为卡住了
            if (Time.time - moveStartTime > 30f)
            {
                Debug.LogWarning($"[NPCMovement] {name} 移动超时，重新生成目标");
                break;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        currentTargetPosition = Vector3.zero;
        isMoving = false;
        totalMoves++;
        totalMoveTime += Time.time - moveStartTime;
        isInPosition = true;
        // 恢复原速度,并停止移动
        navAgent.speed = previousSpeed;
        navAgent.isStopped = true;

        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 到达目标点 {target}，移动次数: {totalMoves}");
    }
    public void MoveToRandomPosition(){
        Vector3 randomTarget = GenerateRandomTarget();
        if(randomTarget != Vector3.zero){
            MoveToTarget(randomTarget);
        }
    }
    private IEnumerator WaitAtTarget()
    {
        isWaiting = true;
        
        
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 在目标点等待 {waitTime:F1} 秒");
        
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false;
    }
    
    private bool FindNearestNavMeshPosition(Vector3 position, out Vector3 result)
    {
        // 在较大范围内搜索最近的NavMesh位置
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 50f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        
        result = Vector3.zero;
        return false;
    }
    
    private void Update()
    {
        // 处理转向逻辑
        if (isTurning)
        {
            UpdateTurning();
        }
        
        // 如果NavMeshAgent在NavMesh上，则设置动画速度
        if(navAgent.isOnNavMesh){
            animator.SetFloat("Speed",navAgent.velocity.magnitude);
        }else{
            animator.SetFloat("Speed",0);
        }

        // // 监控NavMeshAgent状态
        // if (navAgent != null && !navAgent.isOnNavMesh)
        // {
        //     Debug.LogWarning($"[NPCMovement] {name} 离开了NavMesh！");
            
        //     // 尝试恢复到NavMesh上
        //     Vector3 validPosition;
        //     if (FindNearestNavMeshPosition(transform.position, out validPosition))
        //     {
        //         transform.position = validPosition;
        //     }
        // }
    }
    
    /// <summary>
    /// 更新转向逻辑
    /// </summary>
    public void UpdateTurning()
    {
        if (targetDirection == Vector3.zero) return;
        
        // 计算当前方向和目标方向的角度差
        float angle = Vector3.Angle(transform.forward, targetDirection);
        
        // 如果角度差小于阈值，停止转向
        if (angle < turnThreshold)
        {
            isTurning = false;
            OnTurnCompleted();
            return;
        }
        
        // 平滑转向目标方向
        Vector3 newDirection = Vector3.Slerp(transform.forward, targetDirection, turnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
    
    /// <summary>
    /// 转向完成回调
    /// </summary>
    public void OnTurnCompleted()
    {
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 转向完成，开始移动");
        targetDirection = Vector3.zero;
        
        // 转向完成后，开始移动
        if (currentTarget != null && navAgent != null)
        {
            navAgent.SetDestination(currentTarget.position);
        }
    }
    
    /// <summary>
    /// 转向指定方向
    /// </summary>
    /// <param name="direction">目标方向</param>
    public void TurnToDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        
        targetDirection = direction.normalized;
        isTurning = true;
        
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 开始转向目标方向");
    }
    
    /// <summary>
    /// 转向指定位置
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPosition(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 忽略Y轴，只在水平面转向
        TurnToDirection(direction);
    }
    
    /// <summary>
    /// 立即转向目标位置（不使用平滑转向）
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void TurnToPositionImmediate(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // 忽略Y轴，只在水平面转向
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            if (showDebugInfo)
                Debug.Log($"[NPCMovement] {name} 立即转向目标位置");
        }
    }
    
    /// <summary>
    /// 移动到指定Transform目标
    /// </summary>
    /// <param name="target">目标Transform</param>
    // public void MoveToTarget(Transform target) {
    //     if(navAgent == null || target == null) return;
        
    //     currentTarget = target;
        
    //     if (enableTurnBeforeMove)
    //     {
    //         // 先转向目标，转向完成后再开始移动
    //         Vector3 direction = (target.position - transform.position).normalized;
    //         direction.y = 0; // 忽略Y轴，只在水平面转向
            
    //         // 检查是否需要转向
    //         float angle = Vector3.Angle(transform.forward, direction);
    //         if (angle > turnThreshold)
    //         {
    //             Debug.Log($"[NPCMovement] {name} 需要转向 {angle:F1}度，开始转向");
    //             TurnToDirection(direction);
    //         }
    //         else
    //         {
    //             // 角度差很小，直接移动
    //             Debug.Log($"[NPCMovement] {name} 角度差较小({angle:F1}度)，直接移动");
    //             navAgent.SetDestination(target.position);
    //         }
    //     }
    //     else
    //     {
    //         // 直接移动，不转向
    //         navAgent.SetDestination(target.position);
    //     }
    // }
    
    
    /// <summary>
    /// 移动到社交位置的协程方法
    /// </summary>
    /// <param name="position">目标位置</param>
    /// <param name="socialMoveSpeed">社交移动速度</param>
    /// <returns></returns>
    public IEnumerator MoveToSocialPosition(Vector3 position, float socialMoveSpeed = 0.5f) {
        
        // 使用NavMeshAgent移动
        if (navAgent != null)
        {
            if (showDebugInfo)
                Debug.Log($"[NPCMovement] {name} 开始移动到社交位置: {position}");
            // 保存当前位置和速度
            // Vector3 previousPosition = transform.position;
            float previousSpeed = navAgent.speed;
            // 设置移动速度
            navAgent.speed = socialMoveSpeed;
            
            // 如果启用转向，先转向目标
            if (enableTurnBeforeMove)
            {
                // TODO: 这里需要优化，因为这里会立即转向，不合理
                TurnToPositionImmediate(position); // 社交移动使用立即转向，避免复杂的异步逻辑
            }
            
            // 设置目标位置
            navAgent.SetDestination(position);
            
            // 等待到达目标位置
            while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
            {
                // 检查是否卡住
                if (navAgent.velocity.magnitude < 0.1f && navAgent.remainingDistance > navAgent.stoppingDistance)
                {
                    // 可能卡住了，等待一小段时间
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            // 恢复速度
            navAgent.speed = previousSpeed;
            if (showDebugInfo)
                Debug.Log($"[NPCMovement] {name} 已到达社交位置");
        } else{
            Debug.LogError($"[NPCMovement] {name} 没有NavMeshAgent组件，无法移动");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
        // 绘制移动半径
        Gizmos.color = Color.yellow;
        GizmoExtensions.DrawWireCircle(transform.position, moveRadius);
        
        // 绘制当前目标
        if (isMoving && currentTargetPosition != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTargetPosition, 0.5f);
            
            // 绘制到目标的直线
            Gizmos.color = pathColor;
            Gizmos.DrawLine(transform.position, currentTargetPosition);
        }
        
        // 绘制NavMeshAgent路径
        if (navAgent != null && navAgent.path != null && navAgent.path.corners.Length > 1)
        {
            Gizmos.color = pathColor;
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
        
        // 绘制起始位置
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, 0.3f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // 绘制详细信息
        if (showDebugInfo)
        {
            Gizmos.color = Color.white;
            GizmoExtensions.DrawWireCircle(transform.position, stoppingDistance);
        }
    }
    
    // 公共接口
    public bool IsMoving => isMoving;
    public bool IsWaiting => isWaiting;
    public Vector3 CurrentTargetPosition => currentTargetPosition;
    public int TotalMoves => totalMoves;
    public float AverageMoveTime => totalMoves > 0 ? totalMoveTime / totalMoves : 0f;
    
    // 设置方法
    public void SetMoveRadius(float radius)
    {
        moveRadius = Mathf.Max(1f, radius);
    }
    
    public void SetWaitTime(float min, float max)
    {
        minWaitTime = Mathf.Max(0.1f, min);
        maxWaitTime = Mathf.Max(minWaitTime, max);
    }
    
    public void SetMovementSpeed(float speed)
    {
        movementSpeed = Mathf.Max(0.1f, speed);
        if (navAgent != null)
        {
            navAgent.speed = movementSpeed;
        }
    }
    
    private void OnDisable()
    {
        StopRandomMovement();
    }
    
    private void OnDestroy()
    {
        StopRandomMovement();
    }

    /// <summary>
    /// 设置当前目标Transform
    /// </summary>
    /// <param name="target">目标Transform</param>
    public void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 设置当前目标: {(target != null ? target.name : "null")}");
    }

    /// <summary>
    /// 设置当前目标位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void SetCurrentTargetPosition(Vector3 position)
    {
        currentTargetPosition = position;
        if (showDebugInfo)
            Debug.Log($"[NPCMovement] {name} 设置当前目标位置: {position}");
    }
}