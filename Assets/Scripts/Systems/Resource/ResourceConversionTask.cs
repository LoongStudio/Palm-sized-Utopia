// ResourceConversionTask.cs
using System;
using UnityEngine;

[System.Serializable]
public class ResourceConversionTask
{
    [Header("任务基本信息")]
    public string taskId;                    // 唯一任务ID
    public ResourceType outputType;          // 产出资源类型
    public int outputAmount;                 // 产出数量
    public ResourceCost[] inputCosts;        // 输入成本（用于取消时回退）
    
    [Header("时间信息")]
    public float totalTime;                  // 总转化时间
    public float remainingTime;              // 剩余时间
    public DateTime startTime;               // 开始时间
    public DateTime? pauseTime;              // 暂停时间（null表示未暂停）
    
    [Header("状态信息")]
    public ConversionState state;            // 转化状态
    public int sourceInstanceId;             // 来源建筑的InstanceID
    public BuildingType sourceBuildingType;  // 来源建筑类型
    public string sourceBuildingName;        // 来源建筑名称
    
    [Header("配置信息")]
    public bool canBePaused = true;          // 是否可以暂停
    public bool canBeCanceled = true;        // 是否可以取消
    public float cancelRefundRate = 0.8f;    // 取消时的资源回退比例
    
    public float Progress => 1f - (remainingTime / totalTime);
    public bool IsCompleted => remainingTime <= 0f;
    public bool IsPaused => state == ConversionState.Paused;
    public bool IsActive => state == ConversionState.Active;
    
    public ResourceConversionTask()
    {
        taskId = System.Guid.NewGuid().ToString();
        state = ConversionState.Active;
        startTime = DateTime.Now;
    }
    
    public ResourceConversionTask(ResourceConversion conversion, Building sourceBuilding)
    {
        taskId = System.Guid.NewGuid().ToString();
        outputType = conversion.outputType;
        outputAmount = conversion.outputAmount;
        inputCosts = conversion.inputCosts;
        totalTime = conversion.conversionTime;
        remainingTime = conversion.conversionTime;
        startTime = DateTime.Now;
        state = ConversionState.Active;
        
        if (sourceBuilding != null)
        {
            sourceInstanceId = sourceBuilding.GetInstanceID();
            sourceBuildingType = sourceBuilding.type;
            sourceBuildingName = sourceBuilding.data.buildingName;
        }
    }
    
    public bool Pause()
    {
        if (canBePaused && state == ConversionState.Active)
        {
            state = ConversionState.Paused;
            pauseTime = DateTime.Now;
            return true;
        }
        return false;
    }
    
    public bool Resume()
    {
        if (state == ConversionState.Paused)
        {
            state = ConversionState.Active;
            pauseTime = null;
            return true;
        }
        return false;
    }
    
    public bool Cancel()
    {
        if (canBeCanceled && state != ConversionState.Completed)
        {
            state = ConversionState.Canceled;
            return true;
        }
        return false;
    }
    
    public void Complete()
    {
        state = ConversionState.Completed;
        remainingTime = 0f;
    }
}

public enum ConversionState
{
    Active,      // 进行中
    Paused,      // 暂停
    Completed,   // 完成
    Canceled     // 取消
}