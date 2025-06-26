using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 新版NPC移动控制器（不依赖NavMeshAgent，支持Boid群体避障）
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(BoidBehavior))]
public class NPCPhysicsMovement : MonoBehaviour
{
    [Header("移动配置")]
    public float moveRadius = 3f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 2f;
    public float movementSpeed = 2f;
    public float stoppingDistance = 0.5f;

    [Header("调试")]
    public bool showDebugInfo = false;

    private Rigidbody rb;
    private Animator animator;
    private BoidBehavior boid;

    private Vector3[] currentPath;
    private int pathIndex = 0;
    private bool isMoving = false;
    private Coroutine movementCoroutine;

    private Transform currentTarget;
    public Transform CurrentTarget => currentTarget ? currentTarget : null;
    public bool isInPosition => Vector3.Distance(currentTarget.position, transform.position) < stoppingDistance;
    private Vector3 currentTargetPosition;

    public bool IsMoving => isMoving;
    public Vector3 CurrentTargetPosition => (currentPath != null && pathIndex < currentPath.Length) ? currentPath[pathIndex] : transform.position;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        boid = GetComponent<BoidBehavior>();
    }

    void Update()
    {
        if (animator != null)
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

    public void StartRandomMovement()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(RandomMovementLoop());
    }

    public void StopMovement()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }

    public void MoveToTarget(Vector3 target)
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToTargetCoroutine(target));
    }

    public void MoveToRandomPosition()
    {
        Vector3 randomTarget = GenerateRandomTarget();
        MoveToTarget(randomTarget);
    }

    private Vector3 GenerateRandomTarget()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 circle = Random.insideUnitCircle * moveRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, 0, circle.y);
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
                return hit.position;
        }
        return transform.position;
    }

    private IEnumerator RandomMovementLoop()
    {
        while (true)
        {
            Vector3 target = GenerateRandomTarget();
            yield return MoveToTargetCoroutine(target);
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);
        }
    }

    private IEnumerator MoveToTargetCoroutine(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path) || path.corners.Length < 2)
            yield break;

        currentPath = path.corners;
        pathIndex = 1;
        isMoving = true;

        while (pathIndex < currentPath.Length)
        {
            Vector3 waypoint = currentPath[pathIndex];
            while (Vector3.Distance(transform.position, waypoint) > stoppingDistance)
            {
                Vector3 boidForce = boid.CalculateBoidForce();
                Vector3 dir = (waypoint - transform.position).normalized + boidForce;
                dir.y = 0;
                rb.linearVelocity = dir.normalized * movementSpeed;
                yield return null;
            }
            pathIndex++;
        }
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }

    /// <summary>
    /// 立即转向目标方向（不平滑）
    /// </summary>
    public void TurnToDirection(Vector3 direction)
    {
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
            if (showDebugInfo)
                Debug.Log($"[NPCPhysicsMovement] {name} 立即转向目标方向");
        }
    }

    /// <summary>
    /// 立即转向目标位置（不平滑）
    /// </summary>
    public void TurnToPositionImmediate(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        TurnToDirection(direction);
    }

    /// <summary>
    /// 以社交速度移动到指定位置（协程）
    /// </summary>
    public IEnumerator MoveToSocialPosition(Vector3 position, float socialMoveSpeed = 0.5f)
    {
        // 计算路径
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, position, NavMesh.AllAreas, path) || path.corners.Length < 2)
            yield break;
        currentPath = path.corners;
        pathIndex = 1;
        isMoving = true;
        while (pathIndex < currentPath.Length)
        {
            Vector3 waypoint = currentPath[pathIndex];
            while (Vector3.Distance(transform.position, waypoint) > stoppingDistance)
            {
                Vector3 boidForce = boid.CalculateBoidForce();
                Vector3 dir = (waypoint - transform.position).normalized + boidForce;
                dir.y = 0;
                rb.linearVelocity = dir.normalized * socialMoveSpeed;
                yield return null;
            }
            pathIndex++;
        }
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }

    /// <summary>
    /// 停止随机移动（与StopMovement一致，便于兼容）
    /// </summary>
    public void StopRandomMovement()
    {
        StopMovement();
    }

    /// <summary>
    /// 是否已落地（可根据需要自定义判定，这里简单返回true）
    /// </summary>
    public bool isLanded = true;

    /// <summary>
    /// 设置当前目标Transform
    /// </summary>
    public void SetCurrentTarget(Transform target)
    {
        currentTarget = target;
        if (showDebugInfo)
            Debug.Log($"[NPCPhysicsMovement] {name} 设置当前目标: {(target != null ? target.name : "null")}");
    }

    /// <summary>
    /// 设置当前目标位置
    /// </summary>
    public void SetCurrentTargetPosition(Vector3 position)
    {
        currentTargetPosition = position;
        if (showDebugInfo)
            Debug.Log($"[NPCPhysicsMovement] {name} 设置当前目标位置: {position}");
    }

    /// <summary>
    /// 平滑转向目标位置（可选实现，默认立即转向）
    /// </summary>
    public void TurnToPosition(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        TurnToDirection(direction);
    }
} 