using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// NPC随机移动控制器 - 用于测试社交系统
/// 让NPC在NavMesh区域内随机移动，并在到达目标后暂停一段时间
/// </summary>
public class NPCMovement : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveRadius = 3f;              // 移动半径
    [SerializeField] private float minWaitTime = 1f;              // 最短等待时间
    [SerializeField] private float maxWaitTime = 2f;              // 最长等待时间
    [SerializeField] private float movementSpeed = 2f;          // 移动速度
    [SerializeField] private float stoppingDistance = 0.5f;       // 到达目标的停止距离
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;           // 显示调试信息
    [SerializeField] private bool drawGizmos = true;              // 绘制调试线条
    [SerializeField] private Color pathColor = Color.green;       // 路径颜色
    
    // 组件引用
    private NavMeshAgent navAgent;
    private NPC npcComponent;
    
    // 状态变量
    private Vector3 currentTarget;
    private bool isMoving = false;
    private bool isWaiting = false;
    private Coroutine movementCoroutine;
    
    // 统计信息
    private int totalMoves = 0;
    private float totalMoveTime = 0f;
    private Vector3 startPosition;
    
    private void Awake()
    {
        // 获取NavMeshAgent组件
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // 获取NPC组件（如果存在）
        npcComponent = GetComponent<NPC>();
        
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
        
        Debug.Log($"[NPCMovement] {name} 延迟初始化完成，开始随机移动");
        // 注意：不在这里调用StartRandomMovement，让NPCSpawner控制何时开始移动
    }
    
    private void InitializeNavAgent()
    {
        // 配置NavMeshAgent
        navAgent.speed = movementSpeed;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.angularSpeed = 120f;
        navAgent.acceleration = 8f;
        navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        
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
        
        movementCoroutine = StartCoroutine(RandomMovementLoop());
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
        
        Debug.Log($"[NPCMovement] {name} 停止随机移动");
    }
    
    private IEnumerator RandomMovementLoop()
    {
        while (true)
        {
            // 生成随机目标点
            Vector3 randomTarget = GenerateRandomTarget();
            
            if (randomTarget != Vector3.zero)
            {
                // 移动到目标点
                yield return StartCoroutine(MoveToTarget(randomTarget));
                
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
                        {
                            Debug.Log($"[NPCMovement] {name} 生成目标点: {hit.position} (尝试 {i + 1} 次)");
                        }
                        return hit.position;
                    }
                }
            }
        }
        
        Debug.LogWarning($"[NPCMovement] {name} 在 {maxAttempts} 次尝试后无法生成有效目标点");
        return Vector3.zero;
    }
    
    private IEnumerator MoveToTarget(Vector3 target)
    {
        currentTarget = target;
        isMoving = true;
        float moveStartTime = Time.time;
        
        // 更新NPC状态（如果有NPC组件）
        if (npcComponent != null)
        {
            npcComponent.ChangeState(NPCState.MovingToSource);
        }
        
        // 设置NavMeshAgent目标
        navAgent.SetDestination(target);
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovement] {name} 开始移动到 {target}");
        }
        
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
        
        isMoving = false;
        totalMoves++;
        totalMoveTime += Time.time - moveStartTime;
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovement] {name} 到达目标点 {target}，移动次数: {totalMoves}");
        }
    }
    
    private IEnumerator WaitAtTarget()
    {
        isWaiting = true;
        
        // 更新NPC状态
        if (npcComponent != null)
        {
            npcComponent.ChangeState(NPCState.Idle);
        }
        
        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        
        if (showDebugInfo)
        {
            Debug.Log($"[NPCMovement] {name} 在目标点等待 {waitTime:F1} 秒");
        }
        
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
        // 监控NavMeshAgent状态
        if (navAgent != null && !navAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPCMovement] {name} 离开了NavMesh！");
            
            // 尝试恢复到NavMesh上
            Vector3 validPosition;
            if (FindNearestNavMeshPosition(transform.position, out validPosition))
            {
                transform.position = validPosition;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        
        // 绘制移动半径
        Gizmos.color = Color.yellow;
        GizmoExtensions.DrawWireCircle(transform.position, moveRadius);
        
        // 绘制当前目标
        if (isMoving && currentTarget != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            
            // 绘制到目标的直线
            Gizmos.color = pathColor;
            Gizmos.DrawLine(transform.position, currentTarget);
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
    public Vector3 CurrentTarget => currentTarget;
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
}