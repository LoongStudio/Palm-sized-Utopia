public class ResourceEventArgs 
{
    public ResourceType resourceType;
    public int subType;
    public int oldAmount;
    public int newAmount;
    public int changeAmount;
    public string changeReason; // "生产", "消耗", "购买", "出售"等
    // 以下是购买资源时需要的信息
    public int cost;
    public ResourceType costType;
    public int costSubType;

    // 以下是出售资源时需要的信息，默认出售获得金币
    public int price;
    public ResourceType priceType = ResourceType.Coin;
    public int priceSubType = 0;
    // inventory相关
    public Inventory relatedBuildingInventory;
    public Inventory relatedNPCInventory;
    
    public System.DateTime timestamp;
    
}