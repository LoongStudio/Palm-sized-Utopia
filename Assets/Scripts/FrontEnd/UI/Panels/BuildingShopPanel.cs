using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingShopPanel : ShopBasePanel{
    
    private void Start(){
        GenerateBuildingItems();
    }
    #region 生成建筑购买项
    /// <summary>
    /// 从配置表中获取建筑数据，并在自己的content中生成建筑购买项
    /// </summary>
    private void GenerateBuildingItems(){
        // 有效性检测
        if(BuildingManager.Instance == null){
            Debug.LogError("BuildingManager.Instance is null");
            return;
        }
        if(BuildingManager.Instance.BuildingConfig == null){
            Debug.LogError("BuildingManager.Instance.buildingConfig is null");
            return; 
        }

        // 从BuildingManager中获取建筑数据
        List<BuildingPrefabData> buildingPrefabDatas;
        buildingPrefabDatas = BuildingManager.Instance.BuildingConfig.buildingPrefabDatas;

        // 根据建筑数据生成建筑购买项
        foreach(var buildingPrefabData in buildingPrefabDatas){
            GenerateOneBuildingItems(buildingPrefabData);
        }   
    }
    private ShopItem GenerateOneBuildingItems(BuildingPrefabData buildingPrefabData){
        // 生成物品组件预制体并放到content中
        var item = Instantiate(shopItemPrefab, content.transform);
        // 数据准备
        string name = buildingPrefabData.buildingDatas.buildingName;
        string description = buildingPrefabData.buildingDatas.description;
        ItemType itemType = ItemType.Building;
        int price = buildingPrefabData.buildingDatas.purchasePrice;
        ResourceType priceType = ResourceType.Coin;
        int priceSubType = 0;
        Sprite icon = buildingPrefabData.buildingDatas.icon;
        ShopItemData shopItemData = new ShopItemData(name, description, itemType, price, priceType, priceSubType, icon);
        shopItemData.SetUpItem(buildingPrefabData.subType);

        // 设置item的数据
        ShopItem shopItem = item.GetComponent<ShopItem>();
        if(shopItem != null){
            shopItem.SetUp(this, shopItemData);
        }else{
            Debug.LogError("ShopItem预制体没有ShopItem脚本组件");
            return null;
        }
        return shopItem;
    }

    [Button("生成建筑购买项")]
    private void GenerateOneBuildingItems(){
        // 从配置表中获取建筑数据
        Instantiate(shopItemPrefab, content.transform);
    }
    #endregion


}