using UnityEngine;
using System.Collections.Generic;

public class ReportManager : SingletonManager<ReportManager>
{
    private DataCollector dataCollector;
    private List<Report> historicalReports;
    
    public void Initialize() { }
    
    public void GenerateDailyReport() { }
    public void GenerateWeeklyReport() { }
    public void GenerateCustomReport(System.DateTime startDate, System.DateTime endDate) { }
    
    public List<Report> GetHistoricalReports() { return null; }
    public Report GetLatestReport() { return null; }
    
    private void Start()
    {
        // 订阅各种游戏事件
        ResourceManager.OnResourceChanged += dataCollector.RecordResourceChange;
        BuildingManager.OnBuildingBuilt += dataCollector.RecordBuildingEvent;
        NPCManager.OnNPCHired += dataCollector.RecordNPCEvent;
    }
}