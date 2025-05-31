using System;
using System.Collections.Generic;

[Serializable]
public struct ResourceProductionValue<T>
{
    public ResourceSubType subType;
    public T value;
    public void SetValue(T value) { this.value = value; }
    public ResourceProductionValue(ResourceSubType subType, T value) { this.subType = subType; this.value = value; }
}

public abstract class ProductionBuilding : BuildingBase
{
    public List<ResourceProductionValue<int>> producingResourcesSpeed;
    public List<ResourceProductionValue<int>> producingResourcesMaxAmount;
    public List<ResourceProductionValue<int>> currentResources;

    public List<ResourceProductionValue<int>> harvestableThreshold;

    private float _currentTime;
    private float _lastUpdateTime;

    public void UpdateResource()
    {
        // 如果超过一秒没更新
        if (_currentTime - _lastUpdateTime < 1)
        {
            _lastUpdateTime = _currentTime;
            for (int i = 0; i < producingResourcesSpeed.Count; i++)
            {
                currentResources[i].SetValue(
                    Math.Clamp(
                        currentResources[i].value + producingResourcesSpeed[i].value,
                        0, producingResourcesMaxAmount[i].value));
            }
        }
    }

    public void Update()
    {
        UpdateResource();
    }

    public bool IsHarvestable(ResourceSubType subType)
    {
        for (int i = 0; i < harvestableThreshold.Count; i++)
        {
            if (harvestableThreshold[i].subType == subType 
                && harvestableThreshold[i].value < currentResources[i].value)
            {
                return true;
            }
        }

        return false;
    }
}