using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ResourceShopPanel : ShopBasePanel{
    private void Start(){
        GenerateResourceItems();
    }
    protected override void OnClose(){
        base.OnClose();
        UIManager.Instance.ClosePanel("QuantitySelectPanel");
    }

    protected override void OnHide(){
        base.OnHide();
        UIManager.Instance.ClosePanel("QuantitySelectPanel");
    }

    protected override void OnShow(){
        base.OnShow();
        GenerateResourceItems();
    }

    #region 生成资源购买项
    private void GenerateResourceItems(){
        // 清空content
        foreach(Transform child in content.transform){
            Destroy(child.gameObject);
        }

        // 有效性检测
        if(ResourceManager.Instance == null){
            Debug.LogError("ResourceManager.Instance is null");
            return;
        }

        if(ResourceManager.Instance.ResourceSettings == null){
            Debug.LogError("ResourceManager.Instance.ResourceSettings is null");
            return;
        }

        // 从ResourceManager中获取资源数据
        List<ResourceStack> resourceStacks = ResourceManager.Instance.Resources;

        // 只生成资源设置中设置为可购买的资源
        foreach(var resourceStack in resourceStacks){
            if(resourceStack.resourceConfig.canBePurchased){
                GenerateOneResourceItem(resourceStack);
            }
        }
        // 根据资源数据生成资源购买项
    }

    private ShopItem GenerateOneResourceItem(ResourceStack resourceStack){
        // 生成物品组件预制体并放到content中
        var item = Instantiate(shopItemPrefab, content.transform);
        
        // 数据准备
        string name = resourceStack.resourceConfig.displayName;
        string description = resourceStack.resourceConfig.description;
        ItemType itemType = ItemType.Resource;
        int price = resourceStack.purchasePrice;
        
        // TODO: 默认用金钱购买，但后续可能出现其他方式购买的资源
        ResourceType priceType = ResourceType.Coin;
        int priceSubType = 0;
        Sprite icon = resourceStack.resourceConfig.icon;
        ShopItemData shopItemData = new ShopItemData(name, description, itemType, price, priceType, priceSubType, icon);
        // 资源数据的特殊设置
        shopItemData.SetUpItem(resourceStack.resourceConfig.type, resourceStack.resourceConfig.subType, 1);
        
        // 设置物品组件数据
        ShopItem shopItem = item.GetComponent<ShopItem>();
        if(shopItem != null){
            shopItem.SetUp(this, shopItemData);
        }else{
            Debug.LogError("ShopItem预制体没有ShopItem脚本组件");
            return null;
        }
        return shopItem;
    }
    #endregion
}