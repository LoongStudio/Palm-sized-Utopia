using UnityEngine;

public abstract class Equipment : ISaveable
{
    public EquipmentType equipmentType;
    public string equipmentName;
    public int purchasePrice;
    public float maintenanceCost;
    public float deviceBonus;
    public Building installedBuilding;

    public abstract void ApplyEffect(Building building);
    public abstract void RemoveEffect(Building building);
    public virtual float GetMaintenanceCost() { return maintenanceCost; }
    public virtual GameSaveData GetSaveData()
    {
        // TODO: 实现Equipment的GetSaveData
        Debug.LogWarning("Equipment的GetSaveData还没有实现");
        return new EquipmentSaveData()
        {
            equipmentType = equipmentType,
        };
    }
    public virtual void LoadFromData(GameSaveData data)
    {
        var saveData = data as EquipmentSaveData;
        equipmentType = saveData.equipmentType;
        // TODO: 实现Equipment的LoadFromData
        Debug.LogWarning("Equipment的LoadFromData还没有实现");
    }
    public static Equipment CreateEquipmentFromData(EquipmentSaveData data)
    {
        // TODO: 实现Equipment的CreateEquipmentFromData
        Debug.LogWarning("Equipment的CreateEquipmentFromData还没有实现");
        return null;
    }
}