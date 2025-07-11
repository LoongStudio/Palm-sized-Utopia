using System.Collections.Generic;
using UnityEngine;

public class TradeMarket : ProductionBuilding
{
    [Header("贸易市场专属")]
    public float tradeEfficiencyMultiplier = 1.0f;
    
    public override void InitialSelfStorage()
    {

    }

    protected override void SetupProductionRule()
    {
        base.SetupProductionRule();
        // 贸易市场不需要生产规则，它只是自动售卖
    }
    
    public new void OnTryBuilt()
    {
        status = BuildingStatus.Active;
        Debug.Log($"贸易市场建造完成，位置: {string.Join(" ", positions)}");
    }
    
    public override void OnUpgraded()
    {
        tradeEfficiencyMultiplier += 0.1f;
        Debug.Log($"贸易市场升级到等级 {currentLevel}，交易效率: {tradeEfficiencyMultiplier:F1}");
    }
    
    public override void OnDestroyed()
    {
        base.OnDestroyed();
        Debug.Log($"贸易市场被摧毁，位置: {string.Join(" ", positions)}");
    }
    
    public bool SellResource(ResourceType type, int subType, int amount)
    {
        if (!ResourceManager.Instance.HasEnoughResource(type, subType, amount))
            return false;
            
        // 获取基础价格（这里应该从配置中读取）
        int basePrice = GetResourceBasePrice(type);
        int totalPrice = Mathf.RoundToInt(basePrice * amount * 1.0f); // TODO: 这里 1 之后要替换为 BuildingManager 的Buff 统计
        
        ResourceManager.Instance.RemoveResource(type, subType, amount);
        ResourceManager.Instance.AddResource(ResourceType.Coin, 0, totalPrice);
        
        Debug.Log($"出售 {amount} {type}，获得 {totalPrice} 金币");
        return true;
    }
    
    public bool BuyResource(ResourceType type, int subType, int amount)
    {
        int basePrice = GetResourceBasePrice(type);
        int totalPrice = Mathf.RoundToInt(basePrice * amount * 1.2f); // 购买价格比出售价格高20%
        
        if (!ResourceManager.Instance.HasEnoughResource(ResourceType.Coin, 0, totalPrice))
            return false;
            
        ResourceManager.Instance.RemoveResource(ResourceType.Coin, 0, totalPrice);
        ResourceManager.Instance.AddResource(type, subType, amount);
        
        Debug.Log($"购买 {amount} {type}，花费 {totalPrice} 金币");
        return true;
    }
    
    private int GetResourceBasePrice(ResourceType type)
    {
        // 简化的价格系统
        switch (type)
        {
            case ResourceType.Seed: return 5;
            case ResourceType.Crop: return 10;
            case ResourceType.Feed: return 8;
            case ResourceType.Livestock: return 50;
            case ResourceType.BreedingStock: return 100;
            default: return 1;
        }
    }
} 