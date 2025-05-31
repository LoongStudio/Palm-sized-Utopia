using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingManager : SingletonManager<BuildingManager>
{
    public List<BuildingBase> registedBuildings = new();
    // public List<BuildingBase> buildings;
    
    public void FixedUpdate()
    {
        foreach (BuildingBase building in registedBuildings)
        {
            // ResourceManager.Instance.Append(); building.GetResourceData();
        }
    }
    
    
    
    
}
