using UnityEngine;

/// <summary>
/// 编辑器可视化组件 - 让不可见的功能性GameObject在编辑器中可见
/// 适用于Manager、System等纯功能性GameObject
/// </summary>
public class EditorVisualizer : MonoBehaviour
{
    [Header("可视化设置")]
    [SerializeField] private VisualizerType visualizerType = VisualizerType.Icon;
    [SerializeField] private string displayName = "";
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private float gizmoSize = 1.0f;
    
    [Header("图标设置")]
    [SerializeField] private IconType iconType = IconType.Gear;
    [SerializeField] private bool showLabel = true;
    [SerializeField] private Vector3 labelOffset = Vector3.up;
    
    [Header("形状设置")]
    [SerializeField] private ShapeType shapeType = ShapeType.Cube;
    [SerializeField] private bool wireframe = true;
    // [SerializeField] private bool alwaysOnTop = true;
    
    [Header("动画效果")]
    [SerializeField] private bool enablePulse = false;
    [SerializeField] private float pulseSpeed = 2.0f;
    [SerializeField] private float pulseScale = 0.2f;
    
    private string cachedDisplayName;

    public enum VisualizerType
    {
        Icon,       // 图标显示
        Shape,      // 几何形状
        Both        // 图标+形状
    }
    
    public enum IconType
    {
        Gear,           // 齿轮 - 适合Manager
        Database,       // 数据库 - 适合数据管理
        Network,        // 网络 - 适合系统连接
        Controller,     // 控制器 - 适合输入管理
        Timer,          // 时钟 - 适合时间管理
        Grid,           // 网格 - 适合网格系统
        Audio,          // 音频 - 适合音频管理
        Camera,         // 摄像机 - 适合摄像机控制
        Light,          // 灯光 - 适合光照管理
        Warning,        // 警告 - 适合错误处理
        Info,           // 信息 - 适合信息显示
        Custom          // 自定义
    }
    
    public enum ShapeType
    {
        Cube,
        Sphere,
        Cylinder,
        Capsule,
        Diamond,
        Cross
    }

    private void Awake()
    {
        // 自动设置显示名称
        if (string.IsNullOrEmpty(displayName))
        {
            cachedDisplayName = gameObject.name;
        }
        else
        {
            cachedDisplayName = displayName;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawVisualizer(false);
    }
    
    private void OnDrawGizmosSelected()
    {
        DrawVisualizer(true);
    }
    
    private void DrawVisualizer(bool isSelected)
    {
        Vector3 position = transform.position;
        
        // 计算动画效果
        float animationScale = 1.0f;
        if (enablePulse)
        {
            animationScale = 1.0f + Mathf.Sin(Time.realtimeSinceStartup * pulseSpeed) * pulseScale;
        }
        
        // 设置颜色
        Color currentColor = isSelected ? Color.yellow : gizmoColor;
        currentColor.a = isSelected ? 1.0f : 0.7f;
        
        // 绘制可视化元素
        switch (visualizerType)
        {
            case VisualizerType.Icon:
                DrawIcon(position, currentColor, animationScale);
                break;
            case VisualizerType.Shape:
                DrawShape(position, currentColor, animationScale);
                break;
            case VisualizerType.Both:
                DrawIcon(position, currentColor, animationScale);
                DrawShape(position + Vector3.up * gizmoSize * 0.5f, currentColor * 0.5f, animationScale * 0.8f);
                break;
        }
        
        // 绘制标签
        if (showLabel && !string.IsNullOrEmpty(cachedDisplayName))
        {
            DrawLabel(position + labelOffset, cachedDisplayName, currentColor);
        }
    }
    
    private void DrawIcon(Vector3 position, Color color, float scale)
    {
        Gizmos.color = color;
        float size = gizmoSize * scale;
        
        switch (iconType)
        {
            case IconType.Gear:
                DrawGearIcon(position, size);
                break;
            case IconType.Database:
                DrawDatabaseIcon(position, size);
                break;
            case IconType.Network:
                DrawNetworkIcon(position, size);
                break;
            case IconType.Controller:
                DrawControllerIcon(position, size);
                break;
            case IconType.Timer:
                DrawTimerIcon(position, size);
                break;
            case IconType.Grid:
                DrawGridIcon(position, size);
                break;
            case IconType.Audio:
                DrawAudioIcon(position, size);
                break;
            case IconType.Camera:
                DrawCameraIcon(position, size);
                break;
            case IconType.Light:
                DrawLightIcon(position, size);
                break;
            case IconType.Warning:
                DrawWarningIcon(position, size);
                break;
            case IconType.Info:
                DrawInfoIcon(position, size);
                break;
            default:
                DrawGearIcon(position, size);
                break;
        }
    }
    
    private void DrawShape(Vector3 position, Color color, float scale)
    {
        Gizmos.color = color;
        float size = gizmoSize * scale;
        
        switch (shapeType)
        {
            case ShapeType.Cube:
                if (wireframe)
                    Gizmos.DrawWireCube(position, Vector3.one * size);
                else
                    Gizmos.DrawCube(position, Vector3.one * size);
                break;
            case ShapeType.Sphere:
                if (wireframe)
                    Gizmos.DrawWireSphere(position, size * 0.5f);
                else
                    Gizmos.DrawSphere(position, size * 0.5f);
                break;
            case ShapeType.Cylinder:
                DrawCylinder(position, size, wireframe);
                break;
            case ShapeType.Capsule:
                DrawCapsule(position, size, wireframe);
                break;
            case ShapeType.Diamond:
                DrawDiamond(position, size);
                break;
            case ShapeType.Cross:
                DrawCross(position, size);
                break;
        }
    }
    
    private void DrawLabel(Vector3 position, string text, Color color)
    {
        var style = new GUIStyle();
        style.normal.textColor = color;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        
        UnityEditor.Handles.Label(position, text, style);
    }
    
    #region 图标绘制方法
    private void DrawGearIcon(Vector3 position, float size)
    {
        float radius = size * 0.4f;
        int teeth = 8;
        
        // 绘制齿轮外圈
        for (int i = 0; i < teeth; i++)
        {
            float angle1 = (float)i / teeth * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / teeth * Mathf.PI * 2;
            
            Vector3 outer1 = position + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 outer2 = position + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            
            Gizmos.DrawLine(outer1, outer2);
        }
        
        // 绘制内圈
        Gizmos.DrawWireSphere(position, radius * 0.3f);
    }
    
    private void DrawDatabaseIcon(Vector3 position, float size)
    {
        float width = size * 0.6f;
        float height = size * 0.8f;
        
        // 绘制数据库圆柱体
        Vector3 top = position + Vector3.up * height * 0.5f;
        Vector3 bottom = position - Vector3.up * height * 0.5f;
        
        // 上下椭圆
        DrawEllipse(top, width, width * 0.3f);
        DrawEllipse(bottom, width, width * 0.3f);
        
        // 连接线
        Vector3 frontTop = top + Vector3.forward * width * 0.5f;
        Vector3 frontBottom = bottom + Vector3.forward * width * 0.5f;
        Vector3 backTop = top - Vector3.forward * width * 0.5f;
        Vector3 backBottom = bottom - Vector3.forward * width * 0.5f;
        
        Gizmos.DrawLine(frontTop, frontBottom);
        Gizmos.DrawLine(backTop, backBottom);
    }
    
    private void DrawNetworkIcon(Vector3 position, float size)
    {
        float nodeRadius = size * 0.1f;
        float connectionRadius = size * 0.4f;
        
        // 绘制中心节点
        Gizmos.DrawWireSphere(position, nodeRadius);
        
        // 绘制周围节点和连接线
        for (int i = 0; i < 6; i++)
        {
            float angle = (float)i / 6 * Mathf.PI * 2;
            Vector3 nodePos = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * connectionRadius;
            
            Gizmos.DrawWireSphere(nodePos, nodeRadius * 0.7f);
            Gizmos.DrawLine(position, nodePos);
        }
    }
    
    private void DrawControllerIcon(Vector3 position, float size)
    {
        float width = size * 0.8f;
        float height = size * 0.4f;
        
        // 绘制控制器主体
        Gizmos.DrawWireCube(position, new Vector3(width, height * 0.5f, height));
        
        // 绘制摇杆
        Vector3 leftStick = position + Vector3.left * width * 0.2f;
        Vector3 rightStick = position + Vector3.right * width * 0.2f;
        
        Gizmos.DrawWireSphere(leftStick, height * 0.2f);
        Gizmos.DrawWireSphere(rightStick, height * 0.2f);
    }
    
    private void DrawTimerIcon(Vector3 position, float size)
    {
        float radius = size * 0.4f;
        
        // 绘制表盘
        Gizmos.DrawWireSphere(position, radius);
        
        // 绘制指针
        float time = Time.realtimeSinceStartup;
        Vector3 hourHand = position + new Vector3(Mathf.Cos(time * 0.1f), 0, Mathf.Sin(time * 0.1f)) * radius * 0.5f;
        Vector3 minuteHand = position + new Vector3(Mathf.Cos(time), 0, Mathf.Sin(time)) * radius * 0.8f;
        
        Gizmos.DrawLine(position, hourHand);
        Gizmos.DrawLine(position, minuteHand);
        
        // 绘制表盘刻度
        for (int i = 0; i < 12; i++)
        {
            float angle = (float)i / 12 * Mathf.PI * 2;
            Vector3 tickStart = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius * 0.9f;
            Vector3 tickEnd = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(tickStart, tickEnd);
        }
    }
    
    private void DrawGridIcon(Vector3 position, float size)
    {
        float gridSize = size / 4;
        int gridCount = 4;
        
        // 绘制网格
        for (int i = 0; i <= gridCount; i++)
        {
            Vector3 start = position + Vector3.left * size * 0.5f + Vector3.forward * (i * gridSize - size * 0.5f);
            Vector3 end = position + Vector3.right * size * 0.5f + Vector3.forward * (i * gridSize - size * 0.5f);
            Gizmos.DrawLine(start, end);
            
            start = position + Vector3.forward * size * 0.5f + Vector3.right * (i * gridSize - size * 0.5f);
            end = position - Vector3.forward * size * 0.5f + Vector3.right * (i * gridSize - size * 0.5f);
            Gizmos.DrawLine(start, end);
        }
    }
    
    private void DrawAudioIcon(Vector3 position, float size)
    {
        float speakerWidth = size * 0.3f;
        float speakerHeight = size * 0.4f;
        
        // 绘制扬声器
        Gizmos.DrawWireCube(position, new Vector3(speakerWidth, speakerHeight, speakerWidth * 0.5f));
        
        // 绘制声波
        for (int i = 1; i <= 3; i++)
        {
            float waveRadius = size * 0.2f * i;
            Vector3 waveCenter = position + Vector3.right * speakerWidth * 0.5f;
            
            // 绘制弧形声波
            for (int j = 0; j < 20; j++)
            {
                float angle1 = (float)j / 20 * Mathf.PI - Mathf.PI * 0.5f;
                float angle2 = (float)(j + 1) / 20 * Mathf.PI - Mathf.PI * 0.5f;
                
                Vector3 point1 = waveCenter + new Vector3(0, Mathf.Cos(angle1), Mathf.Sin(angle1)) * waveRadius;
                Vector3 point2 = waveCenter + new Vector3(0, Mathf.Cos(angle2), Mathf.Sin(angle2)) * waveRadius;
                
                Gizmos.DrawLine(point1, point2);
            }
        }
    }
    
    private void DrawCameraIcon(Vector3 position, float size)
    {
        float bodySize = size * 0.6f;
        float lensRadius = size * 0.2f;
        
        // 绘制相机主体
        Gizmos.DrawWireCube(position, Vector3.one * bodySize);
        
        // 绘制镜头
        Vector3 lensPos = position + Vector3.forward * bodySize * 0.5f;
        Gizmos.DrawWireSphere(lensPos, lensRadius);
        
        // 绘制取景框
        Vector3 viewfinderPos = position + Vector3.up * bodySize * 0.3f;
        Gizmos.DrawWireCube(viewfinderPos, new Vector3(bodySize * 0.3f, bodySize * 0.2f, bodySize * 0.1f));
    }
    
    private void DrawLightIcon(Vector3 position, float size)
    {
        float bulbRadius = size * 0.3f;
        
        // 绘制灯泡
        Gizmos.DrawWireSphere(position, bulbRadius);
        
        // 绘制光线
        for (int i = 0; i < 8; i++)
        {
            float angle = (float)i / 8 * Mathf.PI * 2;
            Vector3 rayStart = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * bulbRadius;
            Vector3 rayEnd = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * size * 0.6f;
            
            Gizmos.DrawLine(rayStart, rayEnd);
        }
    }
    
    private void DrawWarningIcon(Vector3 position, float size)
    {
        float triangleSize = size * 0.8f;
        float height = triangleSize * 0.866f; // 等边三角形高度
        
        // 绘制三角形警告标志
        Vector3 top = position + Vector3.up * height * 0.5f;
        Vector3 bottomLeft = position - Vector3.up * height * 0.5f - Vector3.right * triangleSize * 0.5f;
        Vector3 bottomRight = position - Vector3.up * height * 0.5f + Vector3.right * triangleSize * 0.5f;
        
        Gizmos.DrawLine(top, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, top);
        
        // 绘制感叹号
        Vector3 exclamationTop = position + Vector3.up * height * 0.2f;
        Vector3 exclamationBottom = position - Vector3.up * height * 0.1f;
        Vector3 dot = position - Vector3.up * height * 0.3f;
        
        Gizmos.DrawLine(exclamationTop, exclamationBottom);
        Gizmos.DrawWireSphere(dot, size * 0.05f);
    }
    
    private void DrawInfoIcon(Vector3 position, float size)
    {
        float radius = size * 0.4f;
        
        // 绘制圆形
        Gizmos.DrawWireSphere(position, radius);
        
        // 绘制字母 i
        Vector3 dotPos = position + Vector3.up * radius * 0.3f;
        Vector3 lineTop = position + Vector3.up * radius * 0.1f;
        Vector3 lineBottom = position - Vector3.up * radius * 0.3f;
        
        Gizmos.DrawWireSphere(dotPos, radius * 0.1f);
        Gizmos.DrawLine(lineTop, lineBottom);
    }
    #endregion
    
    #region 形状绘制方法
    private void DrawEllipse(Vector3 center, float radiusX, float radiusZ)
    {
        int segments = 32;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)i / segments * Mathf.PI * 2;
            float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;
            
            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radiusX, 0, Mathf.Sin(angle1) * radiusZ);
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radiusX, 0, Mathf.Sin(angle2) * radiusZ);
            
            Gizmos.DrawLine(point1, point2);
        }
    }
    
    private void DrawCylinder(Vector3 position, float size, bool wireframe)
    {
        float radius = size * 0.5f;
        float height = size;
        
        Vector3 top = position + Vector3.up * height * 0.5f;
        Vector3 bottom = position - Vector3.up * height * 0.5f;
        
        // 绘制顶部和底部圆
        DrawEllipse(top, radius, radius);
        DrawEllipse(bottom, radius, radius);
        
        // 绘制连接线
        for (int i = 0; i < 4; i++)
        {
            float angle = (float)i / 4 * Mathf.PI * 2;
            Vector3 topPoint = top + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 bottomPoint = bottom + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(topPoint, bottomPoint);
        }
    }
    
    private void DrawCapsule(Vector3 position, float size, bool wireframe)
    {
        float radius = size * 0.3f;
        float height = size;
        
        // 绘制中间圆柱部分
        DrawCylinder(position, height - radius * 2, wireframe);
        
        // 绘制顶部和底部半球
        Vector3 topSphere = position + Vector3.up * (height * 0.5f - radius);
        Vector3 bottomSphere = position - Vector3.up * (height * 0.5f - radius);
        
        if (wireframe)
        {
            Gizmos.DrawWireSphere(topSphere, radius);
            Gizmos.DrawWireSphere(bottomSphere, radius);
        }
        else
        {
            Gizmos.DrawSphere(topSphere, radius);
            Gizmos.DrawSphere(bottomSphere, radius);
        }
    }
    
    private void DrawDiamond(Vector3 position, float size)
    {
        float halfSize = size * 0.5f;
        
        Vector3 top = position + Vector3.up * halfSize;
        Vector3 bottom = position - Vector3.up * halfSize;
        Vector3 front = position + Vector3.forward * halfSize;
        Vector3 back = position - Vector3.forward * halfSize;
        Vector3 right = position + Vector3.right * halfSize;
        Vector3 left = position - Vector3.right * halfSize;
        
        // 绘制菱形的12条边
        Gizmos.DrawLine(top, front);
        Gizmos.DrawLine(top, back);
        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(top, left);
        
        Gizmos.DrawLine(bottom, front);
        Gizmos.DrawLine(bottom, back);
        Gizmos.DrawLine(bottom, right);
        Gizmos.DrawLine(bottom, left);
        
        Gizmos.DrawLine(front, right);
        Gizmos.DrawLine(right, back);
        Gizmos.DrawLine(back, left);
        Gizmos.DrawLine(left, front);
    }
    
    private void DrawCross(Vector3 position, float size)
    {
        float halfSize = size * 0.5f;
        
        // X轴
        Gizmos.DrawLine(position - Vector3.right * halfSize, position + Vector3.right * halfSize);
        // Y轴
        Gizmos.DrawLine(position - Vector3.up * halfSize, position + Vector3.up * halfSize);
        // Z轴
        Gizmos.DrawLine(position - Vector3.forward * halfSize, position + Vector3.forward * halfSize);
    }
    #endregion
    
    /// <summary>
    /// 编辑器中的快速设置按钮
    /// </summary>
    [ContextMenu("设置为管理器样式")]
    public void SetAsManagerStyle()
    {
        visualizerType = VisualizerType.Icon;
        iconType = IconType.Gear;
        gizmoColor = Color.cyan;
        gizmoSize = 1.5f;
        showLabel = true;
        enablePulse = false;
    }
    
    [ContextMenu("设置为系统样式")]
    public void SetAsSystemStyle()
    {
        visualizerType = VisualizerType.Both;
        iconType = IconType.Network;
        shapeType = ShapeType.Cube;
        gizmoColor = Color.green;
        gizmoSize = 1.0f;
        showLabel = true;
        wireframe = true;
        enablePulse = true;
    }
    
    [ContextMenu("设置为数据样式")]
    public void SetAsDataStyle()
    {
        visualizerType = VisualizerType.Icon;
        iconType = IconType.Database;
        gizmoColor = Color.blue;
        gizmoSize = 1.2f;
        showLabel = true;
        enablePulse = false;
    }
#endif
}