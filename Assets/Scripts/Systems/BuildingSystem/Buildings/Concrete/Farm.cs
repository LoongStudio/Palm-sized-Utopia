using UnityEngine;
public class Farm : ProductionBuilding
{
    [Header("农田专属")]
    public CropSubType cropType;
    
    public new void OnTryBuilt() { }
    public override void OnUpgraded() { }
    public override void OnDestroyed() { }
    
    public override bool CanProduce()
    {
        // 检查是否有种子资源
        return false;
    }
    
    public override void ProduceResources()
    {
        // 消耗种子，生产作物和饲料
    }
}