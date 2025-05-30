using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        Debug.Log("开始初始化游戏系统...");
        
        // 按顺序初始化系统
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.Initialize();
            Debug.Log("资源系统初始化完成");
        }
        
        if (BuildingManager.Instance != null)
        {
            BuildingManager.Instance.Initialize();
            Debug.Log("建筑系统初始化完成");
        }
        
        if (NPCManager.Instance != null)
        {
            NPCManager.Instance.Initialize();
            Debug.Log("NPC系统初始化完成");
        }
        
        // 最后初始化报告系统
        if (ReportManager.Instance != null)
        {
            ReportManager.Instance.Initialize();
            Debug.Log("报告系统初始化完成");
        }
        
        Debug.Log("所有系统初始化完成！");
    }
}