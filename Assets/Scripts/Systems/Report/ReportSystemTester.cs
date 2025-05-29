// ReportSystemTester.cs
using UnityEngine;

public class ReportSystemTester : MonoBehaviour
{
    private void Update()
    {
        // 按T键生成测试数据
        if (Input.GetKeyDown(KeyCode.T))
        {
            GenerateTestData();
        }
        
        // 按R键查看当前报告
        if (Input.GetKeyDown(KeyCode.R))
        {
            ShowCurrentReport();
        }
        
        // 按G键生成即时报告
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateInstantReport();
        }
    }
    
    private void GenerateTestData()
    {
        // 模拟一些资源变化
        ResourceManager.Instance.AddResource(ResourceType.Gold, Random.Range(10, 100));
        ResourceManager.Instance.AddResource(ResourceType.Crop, Random.Range(5, 20));
        ResourceManager.Instance.SpendResource(ResourceType.Gold, Random.Range(5, 30));
        
        Debug.Log("测试数据已生成");
    }
    
    private void ShowCurrentReport()
    {
        ReportData report = ReportManager.Instance.GetCurrentReport();
        Debug.Log($"当前报告: {report.reportTitle}");
        Debug.Log($"净收入: {report.netIncome}");
        Debug.Log($"总NPC数: {report.totalNPCs}");
    }
    
    private void GenerateInstantReport()
    {
        ReportData report = ReportManager.Instance.GenerateInstantReport();
        Debug.Log($"即时报告已生成: {report.reportTitle}");
    }
}