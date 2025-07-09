using UnityEngine;
[System.Serializable]
public class ShopItemData{
    public string name;
    public string description;
    public ItemType itemType;
    public int price;
    public ResourceType priceType;
    public int priceSubType;
    public Sprite icon;

    // 建筑相关
    public BuildingSubType buildingSubType;
    // 土地相关
    public PlaceableType placeableType;
    // TODO: NPC相关, 如果有不同NPC的话可以考虑写一下

    // 资源相关
    public ResourceType resourceType;
    public int resourceSubType;
    public int amount;

    public ShopItemData(string name, string description, ItemType itemType, int price, ResourceType priceType, int priceSubType, Sprite icon){
        this.name = name;
        this.description = description;
        this.itemType = itemType;
        this.price = price;
        this.priceType = priceType;
        this.priceSubType = priceSubType;
        this.icon = icon;
    }
    public void SetUpItem(BuildingSubType buildingSubType){
        this.buildingSubType = buildingSubType;
    }
    public void SetUpItem(PlaceableType placeableType){
        this.placeableType = placeableType;
    }
    public void SetUpItem(ResourceType resourceType, int resourceSubType, int amount){
        this.resourceType = resourceType;
        this.resourceSubType = resourceSubType;
        this.amount = amount;
    }
}
