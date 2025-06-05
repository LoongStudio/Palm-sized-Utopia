public abstract class Equipment
{
    public EquipmentType equipmentType;
    public string equipmentName;
    public int purchasePrice;
    public float maintenanceCost;
    public Building installedBuilding;
    
    public abstract void ApplyEffect(Building building);
    public abstract void RemoveEffect(Building building);
    public virtual float GetMaintenanceCost() { return maintenanceCost; }
}