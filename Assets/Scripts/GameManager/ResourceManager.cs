using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ResourceManager: SingletonManager<ResourceManager>
{

	[Header("资源存储")]
	public Dictionary<ResourceSubType, int> currentResources; // 当前资源量
	public Dictionary<ResourceSubType, int> resourceLimits; // 资源堆叠上限
        
	[Header("转化任务")]
	public List<TransformationTask> activeTasks; // 正在进行的转化任务
	public List<TransformationTask> completedTasks; // 已经完成的转化任务
        
	[Header("资源配置")]
	public List<ResourceData> resourceConfigs; // 资源配置
	private Dictionary<ResourceSubType, ResourceData> _resourceDataDict; // 各资源类型对应的数据，从inspector端配置

	[Header("初始资源量设置")] 
	[SerializeField] private Dictionary<ResourceSubType, int> _initialResources; // 用于设置初始资源量，从inspector端配置
	
	protected override void Awake()
	{
		base.Awake();
		// 初始化各种资源字典、列表为空
		InitializeResources();
	}
	void Start()
	{
		// 加载资源配置
		Initialize();
	}
	
	#region 初始化
	private void InitializeResources()
	{
		currentResources = new Dictionary<ResourceSubType, int>();
		resourceLimits = new Dictionary<ResourceSubType, int>();
		
		activeTasks = new List<TransformationTask>();
		completedTasks = new List<TransformationTask>();
	}
	
	public void Initialize()
	{
		// 加载资源数据
		LoadResourceData();
		// 设置初始资源量
		SetInitialResources();
		Debug.Log("资源系统初始化完成");
	}

	private void SetInitialResources()
	{
		foreach (var resource in _initialResources.Keys)
		{
			currentResources[resource] = _initialResources[resource];
		}
	}

	private void LoadResourceData()
	{
		// 清空资源类型-数据字典
		_resourceDataDict.Clear();
		
		// 从配置中加载资源
		foreach (var data in resourceConfigs)
		{
			if (data != null)
			{
				_resourceDataDict[data.resourceType] = data;
			}
		}
		
		Debug.Log($"加载了 {_resourceDataDict.Count} 种资源配置");
	}
	

	#endregion
	
	void Update()
	{
		// 更新转化任务
	}
	
	// 添加资源
	public bool AddResource(ResourceSubType type, int amount)
	{
		if (amount <= 0) return false;
		
		int oldAmount = currentResources[type];

		if (currentResources.ContainsKey(type))
		{
			currentResources[type] += amount;
		}
		else
		{
			currentResources[type] = amount;
		}

		// 检查堆叠限制,不能超出上限
		int stackLimit = resourceLimits[type];
		if (currentResources[type] > stackLimit)
		{
			currentResources[type] = stackLimit;
		}
		
		return false;
	}
        
	// 消耗资源
	public bool ConsumeResource(ResourceSubType type, int amount) { return false; }
        
	// 获取资源数量
	public int GetResourceAmount(ResourceSubType type)
	{
		return currentResources.ContainsKey(type) ? currentResources[type] : 0;
	}
        
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
