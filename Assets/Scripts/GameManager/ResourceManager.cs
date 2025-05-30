using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ResourceManager: SingletonManager<ResourceManager>
{
	[Header("资源存储")]
	public Dictionary<ResourceSubType, int> currentResources;
	public Dictionary<ResourceSubType, int> resourceLimits;
        
	[Header("转化任务")]
	public List<TransformationTask> activeTasks;
	public List<TransformationTask> completedTasks;
        
	[Header("资源配置")]
	public List<ResourceData> resourceConfigs;
	
	protected override void Awake()
	{
		base.Awake();
		// 初始化资源字典
	}
	
	void Start()
	{
		// 加载资源配置
	}
	
	void Update()
	{
		// 更新转化任务
	}
	
	// 添加资源
	public bool AddResource(ResourceSubType type, int amount) { return false; }
        
	// 消耗资源
	public bool ConsumeResource(ResourceSubType type, int amount) { return false; }
        
	// 获取资源数量
	public int GetResourceAmount(ResourceSubType type) { return 0; }
        
	// 检查资源是否足够
	public bool HasEnoughResource(ResourceSubType type, int amount) { return false; }
        
	// 开始资源转化任务
	public TransformationTask StartTransformation(ResourceSubType input, int inputAmount, 
		ResourceSubType output, int outputAmount, 
		float duration, int buildingId) { return null; }
        
	// 暂停转化任务
	public void PauseTask(int taskId) { }
        
	// 恢复转化任务
	public void ResumeTask(int taskId) { }
        
	// 取消转化任务
	public void CancelTask(int taskId) { }
        
	// 完成转化任务
	public void CompleteTask(int taskId) { }
        
	// 获取资源存储上限
	public int GetResourceLimit(ResourceSubType type) { return 0; }
        
	// 设置资源存储上限
	public void SetResourceLimit(ResourceSubType type, int limit) { }
}
