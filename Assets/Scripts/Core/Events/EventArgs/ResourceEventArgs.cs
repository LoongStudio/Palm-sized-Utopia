public class ResourceEventArgs 
{
    public ResourceType resourceType;
    public int subType;
    public int oldAmount;
    public int newAmount;
    public int changeAmount;
    public string changeReason; // "生产", "消耗", "购买", "出售"等
    public System.DateTime timestamp;
    
}