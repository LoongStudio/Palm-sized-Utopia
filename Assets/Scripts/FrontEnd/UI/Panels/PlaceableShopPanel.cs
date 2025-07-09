using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class PlaceableShopPanel : ShopBasePanel
{
    private void Start(){
        GeneratePlaceableItems();
    }
    #region 生成土地购买项
    private void GeneratePlaceableItems(){
        // 有效性检测
        if(PlacementManager.Instance == null){
            Debug.LogError("PlaceableManager.Instance is null");
            return;
        }
        if(PlacementManager.Instance.Settings == null){
            Debug.LogError("PlacementManager.Instance.Settings is null");
            return;
        }

        // 从PlacementManager中获取土地数据
        List<PlaceableData> placeableDatas = PlacementManager.Instance.Settings.placeableDatas;

        // 根据土地数据生成土地购买项
        foreach(var placeableData in placeableDatas){
            GenerateOnePlaceableItems(placeableData);
        }
    }
    private ShopItem GenerateOnePlaceableItems(PlaceableData placeableData){
        // 生成一个土地购买项
        GameObject placeableItem = Instantiate(shopItemPrefab, content.transform);
        // 数据准备
        string name = placeableData.name;
        string description = placeableData.description;
        ItemType itemType = ItemType.Land;
        int price = placeableData.purchasePrice;
        ResourceType priceType = ResourceType.Coin;
        int priceSubType = 0;
        Sprite icon = placeableData.icon;
        ShopItemData shopItemData = new ShopItemData(name, description, itemType, price, priceType, priceSubType, icon);
        shopItemData.SetUpItem(placeableData.type);

        // 设置item的数据
        ShopItem shopItem = placeableItem.GetComponent<ShopItem>();
        if(shopItem != null){
            shopItem.SetUp(this, shopItemData);
        }else{
            Debug.LogError("ShopItem预制体没有ShopItem脚本组件");
            return null;
        }
        return shopItem;
    }
    [Button("生成一个土地购买项")]
    private void GenerateOnePlaceableItems(){
        // 从配置表中获取土地数据
        Instantiate(shopItemPrefab, content.transform);
    }
    #endregion
}