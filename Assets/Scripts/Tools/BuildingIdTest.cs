using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 测试Building ID系统的脚本
/// </summary>
public class BuildingIdTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool runTestOnStart = true;
    [SerializeField] private bool showDebugInfo = true;

    private void Start()
    {
        if (runTestOnStart)
        {
            RunBuildingIdTest();
        }
    }

    [ContextMenu("运行Building ID测试")]
    public void RunBuildingIdTest()
    {
        if (BuildingManager.Instance == null)
        {
            Debug.LogError("[BuildingIdTest] BuildingManager实例不存在，无法运行测试");
            return;
        }

        Debug.Log("[BuildingIdTest] 开始测试Building ID系统...");

        // 获取所有建筑
        var allBuildings = BuildingManager.Instance.GetAllBuildings();
        
        if (allBuildings.Count == 0)
        {
            Debug.LogWarning("[BuildingIdTest] 没有找到任何建筑，跳过测试");
            return;
        }

        Debug.Log($"[BuildingIdTest] 找到 {allBuildings.Count} 个建筑");

        // 测试1: 检查每个建筑都有唯一ID
        HashSet<string> buildingIds = new HashSet<string>();
        foreach (var building in allBuildings)
        {
            string buildingId = building.BuildingId;
            
            if (string.IsNullOrEmpty(buildingId))
            {
                Debug.LogError($"[BuildingIdTest] 建筑 {building.name} 没有ID!");
                continue;
            }

            if (buildingIds.Contains(buildingId))
            {
                Debug.LogError($"[BuildingIdTest] 发现重复的建筑ID: {buildingId}");
            }
            else
            {
                buildingIds.Add(buildingId);
                if (showDebugInfo)
                    Debug.Log($"[BuildingIdTest] 建筑 {building.name} ID: {buildingId}");
            }
        }

        // 测试2: 通过ID查找建筑
        foreach (var building in allBuildings)
        {
            string buildingId = building.BuildingId;
            Building foundBuilding = BuildingManager.Instance.GetBuildingById(buildingId);
            
            if (foundBuilding == null)
            {
                Debug.LogError($"[BuildingIdTest] 无法通过ID {buildingId} 找到建筑 {building.name}");
            }
            else if (foundBuilding != building)
            {
                Debug.LogError($"[BuildingIdTest] 通过ID {buildingId} 找到的建筑不是预期的建筑");
            }
            else if (showDebugInfo)
            {
                Debug.Log($"[BuildingIdTest] 成功通过ID {buildingId} 找到建筑 {building.name}");
            }
        }

        // 测试3: 检查不存在的ID
        string nonExistentId = "non-existent-id";
        Building nonExistentBuilding = BuildingManager.Instance.GetBuildingById(nonExistentId);
        if (nonExistentBuilding != null)
        {
            Debug.LogError($"[BuildingIdTest] 通过不存在的ID {nonExistentId} 意外找到了建筑");
        }
        else if (showDebugInfo)
        {
            Debug.Log($"[BuildingIdTest] 正确返回null对于不存在的ID: {nonExistentId}");
        }

        // 测试4: 检查HasBuildingWithId方法
        foreach (var building in allBuildings)
        {
            string buildingId = building.BuildingId;
            bool hasBuilding = BuildingManager.Instance.HasBuildingWithId(buildingId);
            
            if (!hasBuilding)
            {
                Debug.LogError($"[BuildingIdTest] HasBuildingWithId返回false对于存在的ID: {buildingId}");
            }
            else if (showDebugInfo)
            {
                Debug.Log($"[BuildingIdTest] HasBuildingWithId正确返回true对于ID: {buildingId}");
            }
        }

        bool hasNonExistent = BuildingManager.Instance.HasBuildingWithId(nonExistentId);
        if (hasNonExistent)
        {
            Debug.LogError($"[BuildingIdTest] HasBuildingWithId返回true对于不存在的ID: {nonExistentId}");
        }
        else if (showDebugInfo)
        {
            Debug.Log($"[BuildingIdTest] HasBuildingWithId正确返回false对于不存在的ID: {nonExistentId}");
        }

        Debug.Log("[BuildingIdTest] Building ID系统测试完成!");
    }

    [ContextMenu("打印所有建筑信息")]
    public void PrintAllBuildingInfo()
    {
        if (BuildingManager.Instance == null)
        {
            Debug.LogError("[BuildingIdTest] BuildingManager实例不存在");
            return;
        }

        var allBuildings = BuildingManager.Instance.GetAllBuildings();
        Debug.Log($"[BuildingIdTest] 当前共有 {allBuildings.Count} 个建筑:");

        foreach (var building in allBuildings)
        {
            building.PrintBuildingInfo();
        }
    }
} 