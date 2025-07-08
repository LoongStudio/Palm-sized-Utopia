using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;
using System.Linq;

public class PlaceableManager : SingletonManager<PlaceableManager>, ISaveable
{
    private List<PlaceableObject> _placeableObjects;
    private PlacementSettings _settings;
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    private void OnEnable()
    {
        SubscribeEvents();
    }
    private void OnDisable()
    {
        UnsubscribeEvents();
    }
    private void Initialize()
    {
        _settings = FindAnyObjectByType<PlacementManager>()?.Settings;
        _placeableObjects = new List<PlaceableObject>();
    }
    #region 事件订阅/注销/处理
    private void SubscribeEvents()
    {
        GameEvents.OnLandBought += OnLandBought;
        GameEvents.OnBoughtBuildingPlacedAfterDragging += OnLandPlaced;
    }
    private void UnsubscribeEvents()
    {
        GameEvents.OnLandBought -= OnLandBought;
        GameEvents.OnBoughtBuildingPlacedAfterDragging -= OnLandPlaced;
    }
    private void OnLandBought(BuildingEventArgs args){
        Debug.Log($"[PlaceableManager] 收到地皮购买事件: {args.placeableType}");
        // 必要检测
        // 1. 检测类型是否已配置
        if(!_settings.placeableDatas.Any(data => data.type == args.placeableType))
        {
            Debug.LogError($"[PlaceableManager] 类型{args.placeableType}未配置");
            return;
        }
        // 2. 检测玩家是否拥有足够资源
        if(!ResourceManager.Instance.HasEnoughResource(ResourceType.Coin, CoinSubType.Gold, _settings.GetPurchasePrice(args.placeableType)))
        {
            // TODO: 比如提示资源不足
            Debug.LogWarning($"[PlaceableManager] 玩家没有足够资源购买 {args.placeableType}，需要 {_settings.GetPurchasePrice(args.placeableType)} 金币，玩家当前有 {ResourceManager.Instance.GetResourceAmount(ResourceType.Coin, CoinSubType.Gold)} 金币");
            return;
        }
        
        // 创建地皮
        GameObject obj = CreatePlaceable(args.placeableType);

        // 吸附到鼠标位置等待放置
        if(DragHandler.Instance){
            DragHandler.Instance.StartDrag(obj.GetComponent<PlaceableObject>(), null, true, true);
        }
        else{
            Debug.LogError($"[PlaceableManager] DragHandler未找到，无法放置建筑");
            return;
        }
        
    }
    private void OnLandPlaced(BuildingEventArgs args){
        Debug.Log($"[PlaceableManager] 收到地皮放置事件: {args.placeableType}");
        // 建筑不为空，说明是建筑，不做处理
        if(args.building != null) return;

        // 地皮放置成功
        if(args.eventType == BuildingEventArgs.BuildingEventType.PlaceSuccess){
            // 注册地皮
            RegisterPlaceableObject(args.placeable as PlaceableObject);
            // 消耗资源
            var price = _settings.GetPurchasePrice(args.placeableType); 
            ResourceManager.Instance.RemoveResource(ResourceType.Coin, CoinSubType.Gold, price);
            Debug.Log($"[PlaceableManager] 地皮放置成功 {args.placeableType}，消耗资源: {price} 金币");
        }
        else if(args.eventType == BuildingEventArgs.BuildingEventType.PlaceFailed){
            Debug.Log($"[PlaceableManager] 地皮放置失败: {args.placeableType}");
            // 销毁地皮
            Destroy(args.placeable as PlaceableObject);
        }
        else{
            Debug.LogError($"[PlaceableManager] 地皮放置事件类型错误: {args.eventType}");
        }            
    }
    #endregion
    
    #region 地皮注册与注销
    public void RegisterPlaceableObject(PlaceableObject obj)
    {
        if(_placeableObjects.Contains(obj))
        {
            Debug.LogWarning($"PlaceableObject {obj.name} already registered!");
        }
        else
        {
            _placeableObjects.Add(obj);
        }
    }
    public void UnregisterPlaceableObject(PlaceableObject obj)
    {
        if(_placeableObjects.Contains(obj))
        {
            _placeableObjects.Remove(obj);
        }
        else
        {
            Debug.LogWarning($"PlaceableObject {obj.name} not found!");
        }
    }
    #endregion
    
    #region 保存与加载
    public GameSaveData GetSaveData()
    {
        // 过滤掉含Building的对象
        var saveData = new PlacaebleSaveData();
        var placeableInstances = new List<PlaceableInstanceSaveData>();
        foreach(var obj in _placeableObjects)
        {
            if(obj.HasBuilding()) continue;
            placeableInstances.Add(obj.GetSaveData() as PlaceableInstanceSaveData);
        }
        saveData.PlaceableInstances = placeableInstances;
        Debug.Log($"PlaceableManager: {saveData.PlaceableInstances.Count} 个地皮被保存");
        return saveData;
    } 
    public void LoadFromData(GameSaveData data)
    {
        var saveData = data as PlacaebleSaveData;
        foreach(var instance in saveData.PlaceableInstances)
        {
            // 获取PlaceableType并生成对应的预制体
            GameObject obj = CreatePlaceable(instance.placeableType);
            // 加载数据到PlaceableObject
            obj.GetComponent<PlaceableObject>().LoadFromData(instance);
        }
    }
    #endregion
    
    #region 创建地皮
    private GameObject CreatePlaceable(PlaceableType type)
    {
        var prefab = _settings.GetPlaceablePrefab(type);
        Vector3 farPosition = new Vector3(-99, 0, -99);
        var obj = Instantiate(prefab, farPosition, Quaternion.identity);
        return obj;
    }
    #endregion

    [Button("打印所有地皮")]
    public void PrintAllPlaceableObjects()
    {
        Debug.Log($"======================PlaceableManager: 共有{_placeableObjects.Count}个地皮======================");
        foreach(var obj in _placeableObjects)
        {
            Debug.Log($"PlaceableObject: {obj.name}");
        }
        Debug.Log($"================================================================================================");
    }
}