// BuildingManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : SingletonManager<BuildingManager>
{
    [Header("建筑配置")]
    [SerializeField] private BuildingData[] buildingDataArray;
    [SerializeField] private Transform buildingParent;
    
    [Header("网格系统")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 1f;
    
    [Header("当前建筑")]
    private List<Building> allBuildings;
    private Dictionary<BuildingType, BuildingData> buildingDataDict;
    private bool[,] gridOccupancy; // 网格占用状态
    
    // 事件
    public event System.Action<Building, BuildingAction> OnBuildingChanged;
    public event System.Action<Building> OnBuildingBuilt;
    public event System.Action<Building> OnBuildingUpgraded;
    public event System.Action<Building> OnBuildingDemolished;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeBuildingSystem();
    }
    
    public void Initialize()
    {
        LoadBuildingData();
        InitializeGrid();
        Debug.Log("建筑系统初始化完成");
    }
    
    private void InitializeBuildingSystem()
    {
        allBuildings = new List<Building>();
        buildingDataDict = new Dictionary<BuildingType, BuildingData>();
        
        if (buildingParent == null)
        {
            GameObject parent = new GameObject("Buildings");
            buildingParent = parent.transform;
        }
    }
    
    private void LoadBuildingData()
    {
        buildingDataDict.Clear();
        
        foreach (var data in buildingDataArray)
        {
            if (data != null)
            {
                buildingDataDict[data.buildingType] = data;
            }
        }
        
        Debug.Log($"加载了 {buildingDataDict.Count} 种建筑配置");
    }
    
    private void InitializeGrid()
    {
        gridOccupancy = new bool[gridWidth, gridHeight];
        
        // 清空网格
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                gridOccupancy[x, y] = false;
            }
        }
    }
    
    #region 建筑建造
    
    public bool CanPlaceBuilding(BuildingType type, Vector2Int position)
    {
        BuildingData data = GetBuildingData(type);
        if (data == null) return false;
        
        // 检查资源
        if (!ResourceManager.Instance.HasEnoughResources(data.buildCosts))
        {
            return false;
        }
        
        // 检查网格空间
        return IsGridSpaceAvailable(position, data.size);
    }
    
    public Building PlaceBuilding(BuildingType type, Vector2Int position)
    {
        if (!CanPlaceBuilding(type, position))
        {
            Debug.LogWarning($"无法在位置 {position} 放置建筑 {type}");
            return null;
        }
        
        BuildingData data = GetBuildingData(type);
        
        // 消耗资源
        if (!ResourceManager.Instance.SpendResources(data.buildCosts))
        {
            Debug.LogWarning($"资源不足，无法建造 {data.buildingName}");
            return null;
        }
        
        // 创建建筑
        GameObject buildingObj = Instantiate(data.prefab, GridToWorldPosition(position), Quaternion.identity, buildingParent);
        Building building = buildingObj.GetComponent<Building>();
        
        if (building == null)
        {
            building = buildingObj.AddComponent<Building>();
        }
        
        // 初始化建筑
        building.type = type;
        building.data = data;
        building.gridPosition = position;
        building.level = 1;
        
        // 占用网格
        OccupyGrid(position, data.size, true);
        
        // 注册建筑
        RegisterBuilding(building);
        
        // 触发事件
        OnBuildingBuilt?.Invoke(building);
        OnBuildingChanged?.Invoke(building, BuildingAction.Built);
        
        Debug.Log($"建造了 {data.buildingName} 在位置 {position}");
        return building;
    }
    
    private bool IsGridSpaceAvailable(Vector2Int position, Vector2Int size)
    {
        // 检查边界
        if (position.x < 0 || position.y < 0 || 
            position.x + size.x > gridWidth || position.y + size.y > gridHeight)
        {
            return false;
        }
        
        // 检查占用
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                if (gridOccupancy[x, y])
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void OccupyGrid(Vector2Int position, Vector2Int size, bool occupy)
    {
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    gridOccupancy[x, y] = occupy;
                }
            }
        }
    }
    
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
    }
    
    #endregion
    
    #region 建筑管理
    
    public void RegisterBuilding(Building building)
   {
       if (!allBuildings.Contains(building))
       {
           allBuildings.Add(building);
           
           // 订阅建筑事件
           building.OnBuildingUpgraded += HandleBuildingUpgraded;
           building.OnResourceProduced += HandleResourceProduced;
       }
   }
   
   public void UnregisterBuilding(Building building)
   {
       if (allBuildings.Remove(building))
       {
           // 取消订阅事件
           building.OnBuildingUpgraded -= HandleBuildingUpgraded;
           building.OnResourceProduced -= HandleResourceProduced;
           
           // 释放网格
           if (building.data != null)
           {
               OccupyGrid(building.gridPosition, building.data.size, false);
           }
       }
   }
   
   private void HandleBuildingUpgraded(Building building)
   {
       OnBuildingUpgraded?.Invoke(building);
       OnBuildingChanged?.Invoke(building, BuildingAction.Upgraded);

       // 通知资源管理器建筑升级
        ResourceManager.Instance?.HandleBuildingUpgraded(building);
   }
   
   private void HandleResourceProduced(Building building, ResourceType resourceType, int amount)
   {
       // 可以在这里添加全局生产统计
       OnBuildingChanged?.Invoke(building, BuildingAction.Produced);
   }
   
   public bool DemolishBuilding(Building building)
   {
       if (building == null || !allBuildings.Contains(building))
           return false;
       
        // 处理关联的转化任务
        ResourceManager.Instance?.HandleBuildingDestroyed(building);

       // 收集剩余资源
       building.CollectResources();
       
       // 释放分配的NPC
       var assignedNPCs = new List<NPC>(building.assignedNPCs);
       foreach (var npc in assignedNPCs)
       {
           building.RemoveNPC(npc);
       }
       
       // 触发事件
       OnBuildingDemolished?.Invoke(building);
       OnBuildingChanged?.Invoke(building, BuildingAction.Demolished);
       
       // 注销并销毁
       UnregisterBuilding(building);
       Destroy(building.gameObject);
       
       Debug.Log($"拆除了建筑: {building.data.buildingName}");
       return true;
   }
   
   #endregion
   
   #region 查询方法
   
   public List<Building> GetAllBuildings()
   {
       return new List<Building>(allBuildings);
   }
   
   public List<Building> GetBuildingsOfType(BuildingType type)
   {
       return allBuildings.FindAll(b => b.type == type);
   }
   
   public Building GetBuildingAtPosition(Vector2Int position)
   {
       return allBuildings.Find(b => b.gridPosition == position);
   }
   
   public BuildingData GetBuildingData(BuildingType type)
   {
       return buildingDataDict.ContainsKey(type) ? buildingDataDict[type] : null;
   }
   
   public int GetBuildingCount(BuildingType type)
   {
       return GetBuildingsOfType(type).Count;
   }
   
   public List<Building> GetProductionBuildings()
   {
       return allBuildings.FindAll(b => b.data.isProductionBuilding);
   }
   
   public List<Building> GetDecorativeBuildings()
   {
       return allBuildings.FindAll(b => b.data.isDecorative);
   }
   
   #endregion
   
   #region 自动化操作
   
   public void CollectAllResources()
   {
       foreach (var building in allBuildings)
       {
           if (building.data.isProductionBuilding)
           {
               var collected = building.CollectResources();
               if (collected.Count > 0)
               {
                   Debug.Log($"从 {building.data.buildingName} 收集了资源");
               }
           }
       }
   }
   
   public float GetTotalBeautyValue()
   {
       float total = 0f;
       foreach (var building in allBuildings)
       {
           if (building.data.isDecorative)
           {
               total += building.data.beautyValue;
           }
       }
       return total;
   }
   
   public float GetTotalNPCMoodBonus()
   {
       float total = 0f;
       foreach (var building in allBuildings)
       {
           if (building.data.isDecorative)
           {
               total += building.data.npcMoodBonus;
           }
       }
       return total;
   }
   
   #endregion
   
   #region 存档数据
   
   public BuildingSaveData GetSaveData()
   {
       BuildingSaveData saveData = new BuildingSaveData();
       
       foreach (var building in allBuildings)
       {
           BuildingSaveInfo info = new BuildingSaveInfo
           {
               buildingType = building.type,
               level = building.level,
               gridPosition = building.gridPosition,
               storedResources = building.storedResources,
               assignedNPCIds = new List<int>()
           };
           
           // 保存分配的NPC ID
           foreach (var npc in building.assignedNPCs)
           {
               if (npc != null)
               {
                   info.assignedNPCIds.Add(npc.GetInstanceID());
               }
           }
           
           saveData.buildings.Add(info);
       }
       
       return saveData;
   }
   
   public void LoadSaveData(BuildingSaveData saveData)
   {
       // 清空现有建筑
       ClearAllBuildings();
       
       // 重新创建建筑
       foreach (var info in saveData.buildings)
       {
           Building building = PlaceBuilding(info.buildingType, info.gridPosition);
           if (building != null)
           {
               building.level = info.level;
               building.storedResources = info.storedResources;
               // NPC分配需要在NPC系统加载后处理
           }
       }
   }
   
   private void ClearAllBuildings()
   {
       var buildingsToDestroy = new List<Building>(allBuildings);
       foreach (var building in buildingsToDestroy)
       {
           DemolishBuilding(building);
       }
   }
   
   #endregion
}

[System.Serializable]
public class BuildingSaveData
{
   public List<BuildingSaveInfo> buildings = new List<BuildingSaveInfo>();
}

[System.Serializable]
public class BuildingSaveInfo
{
   public BuildingType buildingType;
   public int level;
   public Vector2Int gridPosition;
   public Dictionary<ResourceType, int> storedResources;
   public List<int> assignedNPCIds;
}