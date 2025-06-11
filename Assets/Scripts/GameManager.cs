using System;
using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{
    [Header("系统管理器")]
    public ResourceManager resourceManager;
    public BuildingManager buildingManager;
    public NPCManager npcManager;
    public ReportManager reportManager;
    public SaveManager saveManager;
    public TimeManager timeManager;
    
    [Header("游戏状态")]
    public bool isGamePaused;
    public float gameSpeed => timeManager.currentTimeScale;
    public System.DateTime gameStartTime;
    public System.DateTime currentGameTime;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeAllSystems();
    }
    
    private void Start()
    {
        StartNewGame();
    }
    
    private void Update()
    {
        if (!isGamePaused)
        {
            UpdateGameTime();
            UpdateAllSystems();
        }
    }
    
    private void InitializeAllSystems()
    {
        resourceManager.Initialize();
        buildingManager.Initialize();
        npcManager.Initialize();
        reportManager.Initialize();
        saveManager.Initialize();
        timeManager.Initialize();
    }
    
    private void UpdateAllSystems() 
    { 
        // 社交系统由NPCManager自身负责更新，避免重复调用
        // TODO: 在这里添加其他需要集中更新的系统
    }
    private void UpdateGameTime()
    {
        currentGameTime += TimeSpan.FromSeconds(Time.deltaTime);
    }
    
    public void StartNewGame() { }
    public void LoadGame()
    {
        saveManager.LoadGame();
    }
    public void SaveGame()
    {
        saveManager.SaveGame();
    }
    private float _gameSpeedBeforePause;
    public void PauseGame()
    {
        _gameSpeedBeforePause = gameSpeed;
        SetGameSpeed(0);
    }
    public void ResumeGame()
    {
        SetGameSpeed(_gameSpeedBeforePause);
    }
    public void SetGameSpeed(float speed)
    {
        timeManager.SetTimeScale(speed);
    }
}