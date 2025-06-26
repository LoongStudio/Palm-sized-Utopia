using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Boid群体行为算法，支持分离、对齐、聚合三种基本行为
/// </summary>
public class BoidBehavior : MonoBehaviour
{
    [Header("Boid参数")]
    public float separationRadius = 1.5f;   // 分离半径
    public float alignmentRadius = 2.5f;    // 对齐半径
    public float cohesionRadius = 2.5f;     // 聚合半径

    public float separationWeight = 2f;     // 分离权重
    public float alignmentWeight = 1f;      // 对齐权重
    public float cohesionWeight = 1f;       // 聚合权重

    public LayerMask npcLayer;              // NPC层

    /// <summary>
    /// 计算Boid三种行为的合力
    /// </summary>
    public Vector3 CalculateBoidForce()
    {
        Vector3 separation = CalculateSeparation();
        Vector3 alignment = CalculateAlignment();
        Vector3 cohesion = CalculateCohesion();
        return separation * separationWeight + alignment * alignmentWeight + cohesion * cohesionWeight;
    }

    /// <summary>
    /// 分离：远离周围太近的NPC
    /// </summary>
    private Vector3 CalculateSeparation()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius, npcLayer);
        Vector3 force = Vector3.zero;
        int count = 0;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == this.gameObject) continue;
            force += (transform.position - neighbor.transform.position).normalized / (Vector3.Distance(transform.position, neighbor.transform.position) + 0.01f);
            count++;
        }
        return count > 0 ? (force / count).normalized : Vector3.zero;
    }

    /// <summary>
    /// 对齐：朝向周围NPC的平均移动方向
    /// </summary>
    private Vector3 CalculateAlignment()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, alignmentRadius, npcLayer);
        Vector3 avgDir = Vector3.zero;
        int count = 0;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == this.gameObject) continue;
            Rigidbody rb = neighbor.GetComponent<Rigidbody>();
            if (rb != null)
            {
                avgDir += rb.linearVelocity;
                count++;
            }
        }
        return count > 0 ? (avgDir / count).normalized : Vector3.zero;
    }

    /// <summary>
    /// 聚合：靠近周围NPC的中心点
    /// </summary>
    private Vector3 CalculateCohesion()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, cohesionRadius, npcLayer);
        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == this.gameObject) continue;
            center += neighbor.transform.position;
            count++;
        }
        if (count > 0)
        {
            center /= count;
            return (center - transform.position).normalized;
        }
        return Vector3.zero;
    }
} 