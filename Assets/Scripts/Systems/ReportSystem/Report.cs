using System.Collections.Generic;

[System.Serializable]
public class Report
{
    public System.DateTime reportDate;
    public ReportType reportType;
    public string reportTitle;
    public string reportContent;
    public List<GameStateSnapshot> dataSnapshots;
    
    public enum ReportType
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }
}