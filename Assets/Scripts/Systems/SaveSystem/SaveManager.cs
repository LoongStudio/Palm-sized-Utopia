using UnityEngine;
using Sirenix.OdinInspector;

public class SaveManager : SingletonManager<SaveManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    private string saveFilePath;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Initialize() { }
    [Button("保存游戏")]
    public void SaveGame() {
        if(showDebugInfo) Debug.Log("获取保存游戏所需数据");
        // 收集所有需要保存的数据
        CoreSaveData saveData = CollectSaveData();
        // 使用ES3将数据保存到文件
        ES3.Save("saveData", saveData);
        if(showDebugInfo) Debug.Log("游戏数据保存成功");
    }
    
    [Button("加载游戏")]
    public bool LoadGame() {
        if(showDebugInfo) Debug.Log("加载游戏数据");
        if(ES3.KeyExists("saveData")) {
            // 从文件中加载数据
            CoreSaveData saveData = ES3.Load<CoreSaveData>("saveData");
            // 应用保存的数据
            ApplySaveData(saveData);
            if(showDebugInfo) Debug.Log("游戏数据加载成功");
            return true;
        }        
        if(showDebugInfo) Debug.LogError("没有保存数据");
        return false;
    }
    public void DeleteSave() { }
    public bool HasSaveFile() { return false; }

    private CoreSaveData CollectSaveData()
    {
        return new CoreSaveData
        {
            npcSaveData = NPCManager.Instance?.GetSaveData() as NPCSaveData,
            buildingSaveData = BuildingManager.Instance?.GetSaveData() as BuildingSaveData,
            placeableSaveData = PlaceableManager.Instance?.GetSaveData() as PlacaebleSaveData
        };
    }
    private void ApplySaveData(CoreSaveData saveData) {
        NPCManager.Instance.LoadFromData(saveData.npcSaveData);
        BuildingManager.Instance.LoadFromData(saveData.buildingSaveData);
        PlaceableManager.Instance.LoadFromData(saveData.placeableSaveData);
    }
}