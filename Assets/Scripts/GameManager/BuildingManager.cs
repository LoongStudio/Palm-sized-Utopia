using UnityEngine;
using System.Collections; 

public class BuildingManager : SingletonManager<BuildingManager>
{
    public List<BuildingBase> registedBuildings = new();

    public void UpdateBuildings()
    {
        foreach (var building in registedBuildings)
        {
            building.UpdateState();
        }
    }
}
