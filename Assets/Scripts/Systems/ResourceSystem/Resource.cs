[System.Serializable]
public class Resource
{
    public ResourceType type;
    public int subType;
    public int amount;
    public ResourceData data;
    
    public Resource(ResourceType type, int subType, int amount) { }
    
    public bool CanStackWith(Resource other) { return false; }
    public void AddAmount(int amount) { }
    public bool RemoveAmount(int amount) { return false; }
}