using UnityEngine;
using System.Collections;

public class SystemTester : MonoBehaviour
{
    [Header("测试配置")]
    public bool enableAutoTesting = false;
    public float testInterval = 5f;
    
    private void Start()
    {
        if (enableAutoTesting)
        {
            StartCoroutine(AutoTestLoop());
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // 资源系统测试
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestResourceSystem();
        }
        
        // 建筑系统测试
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestBuildingSystem();
        }
        
        // NPC系统测试
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestNPCSystem();
        }
        
        // 报告系统测试
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestReportSystem();
        }
        
        // 综合测试
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestIntegratedSystems();
        }

        // 转化系统测试
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestConversionSystem();
        }
        
        // 自动收集资源
        if (Input.GetKeyDown(KeyCode.C))
        {
            BuildingManager.Instance.CollectAllResources();
        }
        
        // 手动发薪
        if (Input.GetKeyDown(KeyCode.P))
        {
            NPCManager.Instance.ProcessPayrollManually();
        }
    }
    
    private void TestResourceSystem()
    {
        Debug.Log("=== 测试资源系统 ===");
        
        // 添加资源
        ResourceManager.Instance.AddResource(ResourceType.Gold, 100);
        ResourceManager.Instance.AddResource(ResourceType.Seed, 50);
        
        // 消耗资源
        ResourceManager.Instance.SpendResource(ResourceType.Gold, 30);
        
        // 购买资源
        ResourceManager.Instance.BuyResource(ResourceType.Feed, 10);
        
        // 出售资源
        ResourceManager.Instance.SellResource(ResourceType.Crop, 5);
        
        // 显示当前资源
        var resources = ResourceManager.Instance.GetAllResources();
        foreach (var kvp in resources)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value}");
        }
    }
    
    private void TestBuildingSystem()
    {
        Debug.Log("=== 测试建筑系统 ===");
        
        // 建造农田
        Building farm = BuildingManager.Instance.PlaceBuilding(BuildingType.Farm, new Vector2Int(0, 0));
        if (farm != null)
        {
            Debug.Log($"成功建造: {farm.data.buildingName}");
        }
        
        // 建造牧场
        Building ranch = BuildingManager.Instance.PlaceBuilding(BuildingType.Ranch, new Vector2Int(2, 0));
        if (ranch != null)
        {
            Debug.Log($"成功建造: {ranch.data.buildingName}");
        }
        
        // 显示建筑统计
        Debug.Log($"总建筑数量: {BuildingManager.Instance.GetAllBuildings().Count}");
        Debug.Log($"生产建筑数量: {BuildingManager.Instance.GetProductionBuildings().Count}");
    }
    
    private void TestNPCSystem()
    {
        Debug.Log("=== 测试NPC系统 ===");
        
        // 雇佣NPC
        NPC npc1 = NPCManager.Instance.HireNPC();
        if (npc1 != null)
        {
            Debug.Log($"雇佣了NPC: {npc1.npcName}");
        }
        
        NPC npc2 = NPCManager.Instance.HireNPC();
        if (npc2 != null)
        {
            Debug.Log($"雇佣了NPC: {npc2.npcName}");
        }
        
        // 自动分配NPC到建筑
        NPCManager.Instance.AutoAssignUnemployedNPCs();
        
        // 显示NPC统计
        NPCStatistics stats = NPCManager.Instance.GetNPCStatistics();
        Debug.Log($"总NPC数量: {stats.totalNPCs}");
        Debug.Log($"已分配NPC: {stats.assignedNPCs}");
        Debug.Log($"平均好感度: {stats.averageFavorability:F1}");
        Debug.Log($"平均效率: {stats.averageEfficiency:F2}");
    }
    
    private void TestReportSystem()
    {
        Debug.Log("=== 测试报告系统 ===");
        
        // 生成即时报告
        ReportData report = ReportManager.Instance.GenerateInstantReport();
        Debug.Log($"报告标题: {report.reportTitle}");
        Debug.Log($"净收入: {report.netIncome}");
        Debug.Log($"总NPC数: {report.totalNPCs}");
        Debug.Log($"平均好感度: {report.averageFavorability:F1}");
        
        // 显示资源变化
        foreach (var kvp in report.resourcesNet)
        {
            if (kvp.Value != 0)
            {
                Debug.Log($"{kvp.Key} 净变化: {kvp.Value}");
            }
        }
    }
    
    private void TestIntegratedSystems()
    {
        Debug.Log("=== 综合系统测试 ===");
        
        // 模拟完整的游戏流程
        StartCoroutine(IntegratedTestSequence());
    }
    
    private void TestConversionSystem()
    {
        Debug.Log("=== 测试转化系统 ===");
        
        // 创建一个简单的转化配方
        ResourceConversion conversion = new ResourceConversion
        {
            outputType = ResourceType.Crop,
            outputAmount = 5,
            conversionTime = 10f,
            inputCosts = new ResourceCost[] 
            {
                new ResourceCost { resourceType = ResourceType.Seed, amount = 2 }
            }
        };
        
        // 开始转化
        var task = ResourceManager.Instance.StartResourceConversion(ResourceType.Seed, conversion);
        if (task != null)
        {
            Debug.Log($"开始转化任务: {task.taskId}");
            
            // 测试暂停
            StartCoroutine(TestConversionControls(task.taskId));
        }
    }

    private IEnumerator TestConversionControls(string taskId)
    {
        yield return new WaitForSeconds(3f);
        
        // 暂停任务
        Debug.Log("暂停转化任务");
        ResourceManager.Instance.PauseConversion(taskId);
        
        yield return new WaitForSeconds(2f);
        
        // 恢复任务
        Debug.Log("恢复转化任务");
        ResourceManager.Instance.ResumeConversion(taskId);
        
        // 或者测试取消
        // yield return new WaitForSeconds(2f);
        // Debug.Log("取消转化任务");
        // ResourceManager.Instance.CancelConversion(taskId, true);
    }

    private IEnumerator IntegratedTestSequence()
    {
        Debug.Log("开始综合测试序列...");
        
        // 1. 添加初始资源
        ResourceManager.Instance.AddResource(ResourceType.Gold, 500);
        ResourceManager.Instance.AddResource(ResourceType.Seed, 20);
        yield return new WaitForSeconds(1f);
        
        // 2. 建造基础设施
        BuildingManager.Instance.PlaceBuilding(BuildingType.Farm, new Vector2Int(0, 0));
        BuildingManager.Instance.PlaceBuilding(BuildingType.Ranch, new Vector2Int(2, 0));
        BuildingManager.Instance.PlaceBuilding(BuildingType.Warehouse, new Vector2Int(0, 2));
        yield return new WaitForSeconds(1f);
        
        // 3. 雇佣NPC
        NPCManager.Instance.HireNPC();
        NPCManager.Instance.HireNPC();
        NPCManager.Instance.HireNPC();
        yield return new WaitForSeconds(1f);
        
        // 4. 分配NPC到建筑
        NPCManager.Instance.AutoAssignUnemployedNPCs();
        yield return new WaitForSeconds(1f);
        
        // 5. 等待一段时间让系统运行
        Debug.Log("等待系统运行...");
        yield return new WaitForSeconds(10f);
        
        // 6. 收集资源
        BuildingManager.Instance.CollectAllResources();
        yield return new WaitForSeconds(1f);
        
        // 7. 生成报告
        ReportData finalReport = ReportManager.Instance.GenerateInstantReport();
        Debug.Log("=== 综合测试报告 ===");
        Debug.Log($"净收入: {finalReport.netIncome}");
        Debug.Log($"建筑活动: {finalReport.buildingActivities.Count}");
        Debug.Log($"NPC生产力评分: {finalReport.npcProductivityScore}");
        
        Debug.Log("综合测试完成！");
    }
    
    private IEnumerator AutoTestLoop()
    {
        while (enableAutoTesting)
        {
            yield return new WaitForSeconds(testInterval);
            
            // 随机执行测试
            int testType = Random.Range(1, 6);
            switch (testType)
            {
                case 1:
                    TestResourceSystem();
                    break;
                case 2:
                    TestBuildingSystem();
                    break;
                case 3:
                    TestNPCSystem();
                    break;
                case 4:
                    TestReportSystem();
                    break;
                case 5:
                    TestIntegratedSystems();
                    break;
            }
        }
    }
    
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("系统测试面板", GUI.skin.box);
        
        if (GUILayout.Button("1 - 测试资源系统"))
        {
            TestResourceSystem();
        }
        
        if (GUILayout.Button("2 - 测试建筑系统"))
        {
            TestBuildingSystem();
        }
        
        if (GUILayout.Button("3 - 测试NPC系统"))
        {
            TestNPCSystem();
        }
        
        if (GUILayout.Button("4 - 测试报告系统"))
        {
            TestReportSystem();
        }
        
        if (GUILayout.Button("5 - 综合测试"))
        {
            TestIntegratedSystems();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("收集所有资源"))
        {
            BuildingManager.Instance.CollectAllResources();
        }
        
        if (GUILayout.Button("处理薪资"))
        {
            NPCManager.Instance.ProcessPayrollManually();
        }
        
        GUILayout.EndArea();
    }
}