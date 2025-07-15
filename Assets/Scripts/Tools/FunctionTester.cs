using UnityEngine;
using Sirenix.OdinInspector;

public class FunctionTester : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestResourceInitialization();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            TestResourceAddRemove();
        }

        
    }
    public void TestResourceInitialization()
    {
        Debug.Log("[FunctionTester] ===============================测试资源初始化===============================");
        ResourceManager.Instance.Initialize();
    }
    public void TestResourceAddRemove()
    {
        Debug.Log("[FunctionTester] ===============================测试资源添加和移除===============================");
        ResourceManager.Instance.AddResource(ResourceType.Seed, (int)SeedSubType.Wheat, 10); // 正常添加
        ResourceManager.Instance.RemoveResource(ResourceType.Seed, (int)SeedSubType.Wheat, 10); // 正常移除
        ResourceManager.Instance.AddResource(ResourceType.Seed, (int)SeedSubType.Wheat, 9999); // 添加超过存储上限的资源
        ResourceManager.Instance.RemoveResource(ResourceType.Seed, (int)SeedSubType.Corn, 10); // 移除不存在的资源
        ResourceManager.Instance.AddResource(ResourceType.Seed, (int)SeedSubType.Corn, 10); // 添加不存在的资源
        ResourceManager.Instance.RemoveResource(ResourceType.Seed, (int)SeedSubType.Wheat, 9999); // 移除超过资源数量的资源
    }

    [ContextMenu("Test Hire NPC")]
    public void TestHireNPC(){
        // TODO: 测试雇佣NPC
        NPCManager.Instance.HireNPC();
    }
    [Button("小麦售价涨为5块1个")]
    public void TestResourcePriceChanged(){
        GameEvents.TriggerResourceSellPriceChanged(new ResourceEventArgs(){
            resourceType = ResourceType.Crop,
            subType = (int)SeedSubType.Wheat,
            price = 5,
            priceType = ResourceType.Coin,
            priceSubType = (int)CoinSubType.Gold
        });
    }
}