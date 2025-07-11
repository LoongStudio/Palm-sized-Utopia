using System;
using System.Collections.Generic;
using UnityEngine;
public class Farm : ProductionBuilding
{
    public override void OnUpgraded() { }
    public override void OnDestroyed() { base.OnDestroyed(); }
    
    public override void InitialSelfStorage()
    {

    }
    protected override void SetupProductionRule()
    {
        base.SetupProductionRule();
    }
}