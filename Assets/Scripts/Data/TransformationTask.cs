using UnityEngine;


[System.Serializable]
public class TransformationTask
{
    [Header("任务基本信息")]
    public int taskId;
    public TransformationTaskState state;
    public float startTime;
    public float duration;
    public float currentProgress;
    
    [Header("资源信息")]
    public ResourceCost[] inputResources;
    public ResourceCost[] outputResources;
    
    [Header("建筑关联")]
    public int buildingId;
    public BuildingSubType buildingType;
    
    [Header("效率修正")]
    public float efficiencyMultiplier = 1f;
    
    // 构造函数
    public TransformationTask(int id, ResourceSubType[] inputs, int[] inputAmts,
        ResourceSubType[] outputs, int[] outputAmts,
        float dur, int bldId)
    {
        // 初始化任务数据
    }
    
    // 更新任务进度
    public void UpdateProgress(float deltaTime) { }
    
    // 暂停任务
    public void Pause() { }
    
    // 恢复任务
    public void Resume() { }
    
    // 取消任务
    public void Cancel() { }
    
    // 完成任务
    public void Complete() { }
    
    // 获取剩余时间
    public float GetRemainingTime() { return 0f; }
    
    // 获取进度百分比
    public float GetProgressPercentage() { return 0f; }
}
