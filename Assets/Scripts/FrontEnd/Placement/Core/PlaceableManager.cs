using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using Sirenix.OdinInspector;

public class PlaceableManager : SingletonManager<PlaceableManager>, ISaveable
{
    private List<PlaceableObject> _placeableObjects;
    private PlacementSettings _settings;
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    
    private void Initialize()
    {
        _settings = FindAnyObjectByType<PlacementManager>()?.Settings;
        _placeableObjects = new List<PlaceableObject>();
    }
    
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
            var prefab = _settings.GetPlaceablePrefab(instance.placeableType);
            Vector3 farPosition = new Vector3(-99, 0, -99);
            var obj = Instantiate(prefab, farPosition, Quaternion.identity);
            // 加载数据到PlaceableObject
            obj.GetComponent<PlaceableObject>().LoadFromData(instance);
        }
    }

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