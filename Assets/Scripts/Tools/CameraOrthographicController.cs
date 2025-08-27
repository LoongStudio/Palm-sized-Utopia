using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 相机正交模式控制器 - 用于调整相机的正交大小在3~15之间，并提供Inspector面板控制
/// </summary>
public class CameraOrthographicController : SingletonManager<CameraOrthographicController>
{
    [Header("正交相机设置")]
    [Range(3f, 15f)]
    [Tooltip("正交相机的大小，范围3~15")]
    [SerializeField] private float orthographicSize = 5f;
    
    [Tooltip("调整大小时的平滑过渡速度")]
    [SerializeField] private float smoothSpeed = 5f;
    
    [Tooltip("目标相机，如果为null则使用主相机")]
    [SerializeField] private Camera targetCamera;
    
    [Header("相机移动设置")]
    [Range(0f, 200f)]
    [Tooltip("移动距离，范围0~200")]
    [SerializeField] private float moveDistance = 10f;
    
    [Tooltip("移动时的平滑过渡速度")]
    [SerializeField] private float moveSmoothSpeed = 5f;
    
    private Vector3 targetPosition;
    
    /// <summary>
    /// 获取或设置正交相机的大小（范围3~15）
    /// </summary>
    public float OrthographicSize
    {
        get { return orthographicSize; }
        set
        {
            orthographicSize = Mathf.Clamp(value, 3f, 15f);
        }
    }
    
    /// <summary>
    /// 获取或设置移动距离（范围0~200）
    /// </summary>
    public float MoveDistance
    {
        get { return moveDistance; }
        set
        {
            moveDistance = Mathf.Clamp(value, 0f, 200f);
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        
        // 如果没有指定相机，则使用主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogWarning("[CameraOrthographicController] 没有找到主相机，请手动指定目标相机");
                return;
            }
        }
        
        // 确保相机处于正交模式
        if (!targetCamera.orthographic)
        {
            Debug.LogWarning("[CameraOrthographicController] 目标相机不是正交模式，将自动切换为正交模式");
            targetCamera.orthographic = true;
        }
        
        // 应用初始正交大小
        targetCamera.orthographicSize = orthographicSize;
        targetPosition = targetCamera.transform.position;
    }
    
    private void Update()
    {
        // 如果相机不是正交模式，强制切换为正交模式
        if (targetCamera != null && !targetCamera.orthographic)
        {
            targetCamera.orthographic = true;
        }
        
        // 平滑调整正交大小
        if (targetCamera != null && Mathf.Abs(targetCamera.orthographicSize - orthographicSize) > 0.01f)
        {
            targetCamera.orthographicSize = Mathf.Lerp(
                targetCamera.orthographicSize, 
                orthographicSize, 
                smoothSpeed * Time.deltaTime
            );
        }
        
        // 平滑移动相机
        if (targetCamera != null && Vector3.Distance(targetCamera.transform.position, targetPosition) > 0.01f)
        {
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position, 
                targetPosition, 
                moveSmoothSpeed * Time.deltaTime
            );
        }
    }
    
    /// <summary>
    /// 立即应用正交大小，不使用平滑过渡
    /// </summary>
    [Button("应用正交大小")]
    public void ApplyOrthographicSize()
    {
        if (targetCamera == null) return;
        
        targetCamera.orthographicSize = orthographicSize;
    }
    
    /// <summary>
    /// 向前移动相机
    /// </summary>
    [Button("向前移动")]
    public void MoveForward()
    {
        if (targetCamera == null) return;
        
        targetPosition += targetCamera.transform.up * moveDistance;
    }
    
    /// <summary>
    /// 向后移动相机
    /// </summary>
    [Button("向后移动")]
    public void MoveBackward()
    {
        if (targetCamera == null) return;
        
        targetPosition -= targetCamera.transform.up * moveDistance;
    }
    
    /// <summary>
    /// 向左移动相机
    /// </summary>
    [Button("向左移动")]
    public void MoveLeft()
    {
        if (targetCamera == null) return;
        
        targetPosition -= targetCamera.transform.right * moveDistance;
    }
    
    /// <summary>
    /// 向右移动相机
    /// </summary>
    [Button("向右移动")]
    public void MoveRight()
    {
        if (targetCamera == null) return;
        
        targetPosition += targetCamera.transform.right * moveDistance;
    }
    
    
    /// <summary>
    /// 增加正交大小
    /// </summary>
    /// <param name="amount">增加的量</param>
    public void IncreaseSize(float amount)
    {
        OrthographicSize += amount;
    }
    
    /// <summary>
    /// 减小正交大小
    /// </summary>
    /// <param name="amount">减小的量</param>
    public void DecreaseSize(float amount)
    {
        OrthographicSize -= amount;
    }
    
    /// <summary>
    /// 设置目标相机
    /// </summary>
    /// <param name="camera">新的目标相机</param>
    public void SetTargetCamera(Camera camera)
    {
        if (camera == null)
        {
            Debug.LogWarning("[CameraOrthographicController] 尝试设置空相机作为目标");
            return;
        }
        
        targetCamera = camera;
        targetCamera.orthographic = true;
        targetCamera.orthographicSize = orthographicSize;
        targetPosition = targetCamera.transform.position;
    }
    
    /// <summary>
    /// 获取当前目标相机
    /// </summary>
    /// <returns>当前目标相机</returns>
    public Camera GetTargetCamera()
    {
        return targetCamera;
    }
    
    /// <summary>
    /// 检查相机是否为正交模式
    /// </summary>
    /// <returns>是否为正交模式</returns>
    public bool IsOrthographic()
    {
        return targetCamera != null && targetCamera.orthographic;
    }
    
    /// <summary>
    /// 获取正交大小的最小限制
    /// </summary>
    /// <returns>最小正交大小</returns>
    public float GetMinSize()
    {
        return 3f;
    }
    
    /// <summary>
    /// 获取正交大小的最大限制
    /// </summary>
    /// <returns>最大正交大小</returns>
    public float GetMaxSize()
    {
        return 15f;
    }
    
    /// <summary>
    /// 设置平滑过渡速度
    /// </summary>
    /// <param name="speed">平滑速度</param>
    public void SetSmoothSpeed(float speed)
    {
        smoothSpeed = Mathf.Max(0.1f, speed);
    }
    
    /// <summary>
    /// 设置移动平滑过渡速度
    /// </summary>
    /// <param name="speed">移动平滑速度</param>
    public void SetMoveSmoothSpeed(float speed)
    {
        moveSmoothSpeed = Mathf.Max(0.1f, speed);
    }
}