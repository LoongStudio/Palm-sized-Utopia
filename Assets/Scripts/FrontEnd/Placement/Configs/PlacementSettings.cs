using UnityEngine;


[CreateAssetMenu(fileName = "PlacementSettings", menuName = "Utopia/Placement Settings")]
public class PlacementSettings : ScriptableObject
{
    [Header("网格设置")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float groundSnapHeight = 0.1f;
    
    [Header("拖拽设置")]
    [SerializeField] private LayerMask dragLayer = 1;
    [SerializeField] private float dragSmoothTime = 0.1f;
    [SerializeField] private bool enableRotationSnap = false;
    [SerializeField] private float rotationSnapAngle = 90f;
    
    [Header("预览设置")]
    [SerializeField] private Material validPreviewMaterial;
    [SerializeField] private Material invalidPreviewMaterial;
    [SerializeField] private bool showGridGizmos = true;
    
    [Header("性能设置")]
    [SerializeField] private float navMeshUpdateDelay = 0.2f;
    [SerializeField] private int maxBatchSize = 50;
    [SerializeField] private bool enableAsyncOperations = true;
    
    [Header("输入设置")]
    [SerializeField] private KeyCode editModeKey = KeyCode.E;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    [SerializeField] private int dragMouseButton = 0;
    
    // 属性访问器
    public float GridSize => gridSize;
    public LayerMask GroundLayer => groundLayer;
    public float GroundSnapHeight => groundSnapHeight;
    public LayerMask DragLayer => dragLayer;
    public float DragSmoothTime => dragSmoothTime;
    public bool EnableRotationSnap => enableRotationSnap;
    public float RotationSnapAngle => rotationSnapAngle;
    public Material ValidPreviewMaterial => validPreviewMaterial;
    public Material InvalidPreviewMaterial => invalidPreviewMaterial;
    public bool ShowGridGizmos => showGridGizmos;
    public float NavMeshUpdateDelay => navMeshUpdateDelay;
    public int MaxBatchSize => maxBatchSize;
    public bool EnableAsyncOperations => enableAsyncOperations;
    public KeyCode EditModeKey => editModeKey;
    public KeyCode CancelKey => cancelKey;
    public int DragMouseButton => dragMouseButton;
}
