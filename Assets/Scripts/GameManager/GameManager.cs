using System;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
	[Header("游戏状态")]
	public bool isGamePaused;
	public float gameSpeed = 1f;
        
	[Header("系统引用")]
	public ResourceManager resourceManager;
	public BuildingManager buildingManager;
	public NPCManager npcManager;
	// public ReportManager reportManager;
	// public UnlockSystem unlockSystem;
	
	protected override void Awake()
	{
		base.Awake();
	}

	private void Start()
	{
		
	}

	private void Update()
	{
		
	}

	public GameManager()
	{
		// GridManager.Instance.SetOccupied(new Vector2Int(0, 0), );
	}

	/// <summary>
	/// 每帧检测一次当前状态
	/// 更新建筑状态： 遍历所有建筑
	///		加成类建筑结算 -> 读取加成变动表，是否存在新的加成更新，否则延用老的加成
	///			水塔 -> 含水区域存在作物生长速度的加成 （需要用一个加成map吗？）
	///		生产类建筑结算 -> 每个建筑的对应资源增长
	///		住房类更新 -> 判断是否存在 实体 需要出门，刷新尝试
	///		功能性建筑更新 -> 各自存在独立逻辑
	///			市场 -> 更新价格曲线
	/// 更新NPC状态
	///		词条加成结算： 同样存在词条更新器，如果有更新则重新计算加成权重
	///			
	///		工作状态检测，是否该继续工作：昼夜循环检测，同种类工作时长检测，建筑提示已经达到可工作上限
	///			工作中：
	///				继续维持、结束判定
	///			工作结束等待结算中： （如何判断）
	///				是否存在指定任务方向，记录工作链
	///				连续三个高强度工作触发轻松工作 （种植、仓储、收割） 触发 （闲逛）
	///				选择工作时依据词条倾向产生的选择权重 随机抽取	
	///			移动向目标中： 
	///				
	/// </summary>
	/// <param name="deltaTime"></param>
	public void Tick(float deltaTime)
	{
		// BuildingManager.Instance.UpdateBuildings();
		// NPCManager.Instance.UpdateNPCs();
		
	}

	private void FixedUpdate()
	{
		Tick(Time.deltaTime);
	}

	// 暂停/恢复游戏
	public void PauseGame() { }
	public void ResumeGame() { }
        
	// 设置游戏速度
	public void SetGameSpeed(float speed) { }
        
	// 保存游戏
	public void SaveGame() { }
        
	// 加载游戏
	public void LoadGame() { }
	
}

