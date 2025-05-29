public class GameManager : SingletonManager<GameManager>
{
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // 按顺序初始化系统
        ResourceManager.Instance.Initialize();
        BuildingManager.Instance.Initialize();
        NPCManager.Instance.Initialize();
        
        // 最后初始化报告系统，确保其他系统已经准备好
        ReportManager.Instance.Initialize();
    }
}