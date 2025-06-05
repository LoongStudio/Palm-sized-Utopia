public class ResourceEventArgs 
{
    public ResourceType resourceType;
    public int subType;
    public int oldAmount;
    public int newAmount;
    public int changeAmount;
    public string changeReason; // "生产", "消耗", "购买", "出售"等
    public System.DateTime timestamp;
    
    public ResourceEventArgs(ResourceType type, int subType, int oldAmount, int newAmount, string reason) 
    {
        this.resourceType = type;
        this.subType = subType;
        this.oldAmount = oldAmount;
        this.newAmount = newAmount;
        this.changeAmount = newAmount - oldAmount;
        this.changeReason = reason;
        this.timestamp = System.DateTime.Now;
    }
}