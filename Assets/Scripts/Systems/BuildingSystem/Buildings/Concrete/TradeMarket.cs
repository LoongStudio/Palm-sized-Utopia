using System.Collections.Generic;
using UnityEngine;

public class TradeMarket : ProductionBuilding
{
    [Header("贸易市场专属")]
    public float tradeEfficiencyMultiplier = 1.0f;
    private void OnEnable()
    {
        GameEvents.OnResourceSellPriceChanged += OnResourceSellPriceChanged;
    }
    private void OnDisable()
    {
        GameEvents.OnResourceSellPriceChanged -= OnResourceSellPriceChanged;
    }
    private void OnResourceSellPriceChanged(ResourceEventArgs args)
    {
        // 更新资源售价
        Debug.Log($"资源{args.resourceType} {args.subType}售价更新为{args.price}");
        // 找到生产规则中inputs对应的规则，修改其outputs中的对应资源的转化数量
        foreach (var rule in productionRules)
        {
            foreach (var input in rule.inputs)
            {
                if(input.resourceConfig.type == args.resourceType && input.resourceConfig.subType == args.subType){
                    Debug.Log($"找到输入为资源{input.resourceConfig.type} {input.resourceConfig.subType}的转化规则");
                    foreach (var output in rule.outputs)
                    {
                        if(output.resourceConfig.type == args.priceType && output.resourceConfig.subType == args.priceSubType){
                            output.amount = args.price;
                            Debug.Log($"找到输出为资源{output.resourceConfig.type} {output.resourceConfig.subType}的转化规则，修改其转化数量为{output.amount}");
                        }
                    }
                }
            }
        }
    }
    public override void Start()
    {
        base.Start();
        // 设置接受列表和转化规则
        SetUpAcceptResources();
    }
    public override void InitialSelfStorage()
    {

    }
    private void SetUpAcceptResources()
    {
        // 接受一切ResourceManager定义的资源中可以被出售的资源
        foreach (var resource in ResourceManager.Instance.ResourceSettings)
        {
            if(resource.canBeSold){
                AcceptResources.Add(resource.resourceConfig);
            }
        }
    }
    protected override void SetupProductionRule()
    {
        base.SetupProductionRule();
        // 此处将自动售卖用生产规则实现
        // 将ResourceManager中可以被出售的资源添加到生产规则中，并根据售价设置生产数量
        foreach (var resource in ResourceManager.Instance.ResourceSettings)
        {
            if(resource.canBeSold){
                productionRules.Add(new ConversionRule(){
                    inputs = new List<ResourceStack>(){
                        new ResourceStack(resource.resourceConfig, 1, 1)
                    },
                    outputs = new List<ResourceStack>(){
                        new ResourceStack(ResourceManager.Instance.Gold, 1, 1)
                    }
                });
            }
        }
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