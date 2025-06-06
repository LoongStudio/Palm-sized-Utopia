using UnityEngine;

/// <summary>
/// 放置系统测试脚本
/// </summary>
public class PlacementSystemTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private GameObject testPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int testObjectCount = 5;
    
    private PlacementManager placementManager;
    
    private void Start()
    {
        placementManager = FindAnyObjectByType<PlacementManager>();
        
        if (placementManager == null)
        {
            Debug.LogError("[PlacementSystemTester] PlacementManager not found!");
        }
    }
    
    private void Update()
    {
        // 测试快捷键
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SpawnTestObject();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ClearAllTestObjects();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestGridSystem();
        }
    }
    
    [ContextMenu("Spawn Test Object")]
    private void SpawnTestObject()
    {
        if (testPrefab == null || placementManager == null) return;
        
        var spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        var testObj = Instantiate(testPrefab, spawnPos, Quaternion.identity);
        
        // 确保有PlaceableObject组件
        var placeable = testObj.GetComponent<PlaceableObject>();
        if (placeable == null)
        {
            placeable = testObj.AddComponent<PlaceableObject>();
        }
        
        Debug.Log($"[PlacementSystemTester] Spawned test object: {testObj.name}");
    }
    
    [ContextMenu("Clear All Test Objects")]
    private void ClearAllTestObjects()
    {
        if (placementManager == null) return;
        
        placementManager.ClearAllPlacements();
        Debug.Log("[PlacementSystemTester] Cleared all test objects");
    }
    
    [ContextMenu("Test Grid System")]
    private void TestGridSystem()
    {
        if (placementManager?.GridSystem == null) return;
        
        var gridSystem = placementManager.GridSystem;
        
        // 测试坐标转换
        var worldPos = new Vector3(5.5f, 0, 3.2f);
        var gridPos = gridSystem.WorldToGrid(worldPos);
        var backToWorld = gridSystem.GridToWorld(gridPos);
        
        Debug.Log($"[PlacementSystemTester] Coordinate conversion test:");
        Debug.Log($"World: {worldPos} -> Grid: {gridPos} -> World: {backToWorld}");
        
        // 测试占用检查
        Debug.Log($"Grid position {gridPos} is occupied: {gridSystem.IsOccupied(gridPos)}");
    }
    
    [ContextMenu("Spawn Multiple Test Objects")]
    private void SpawnMultipleTestObjects()
    {
        for (int i = 0; i < testObjectCount; i++)
        {
            var offset = new Vector3(i * 2, 0, 0);
            var spawnPos = (spawnPoint != null ? spawnPoint.position : transform.position) + offset;
            
            var testObj = Instantiate(testPrefab, spawnPos, Quaternion.identity);
            testObj.name = $"TestObject_{i}";
            
            var placeable = testObj.GetComponent<PlaceableObject>();
            if (placeable == null)
            {
                placeable = testObj.AddComponent<PlaceableObject>();
            }
        }
        
        Debug.Log($"[PlacementSystemTester] Spawned {testObjectCount} test objects");
    }
}