using UnityEngine;
using System.Collections.Generic;

public class ReportManager : SingletonManager<ReportManager>
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = false;
    
    private DataCollector dataCollector;
    private List<Report> historicalReports;
    protected override void Awake()
    {
        base.Awake();
    }
    public void Initialize() 
    { 
        // 初始化数据收集器
        if (dataCollector == null)
            dataCollector = new DataCollector();
    }
    
    public void GenerateDailyReport() { }
    public void GenerateWeeklyReport() { }
    public void GenerateCustomReport(System.DateTime startDate, System.DateTime endDate) { }
    
    public List<Report> GetHistoricalReports() { return null; }
    public Report GetLatestReport() { return null; }
    
    private void Start()
    {
        // TODO: 订阅各种游戏事件
        // ResourceManager.OnResourceChanged += dataCollector.RecordResourceChange;
        // BuildingManager.OnBuildingBuilt += dataCollector.RecordBuildingEvent;
        // NPCManager.OnNPCHired += dataCollector.RecordNPCEvent;
    }
}