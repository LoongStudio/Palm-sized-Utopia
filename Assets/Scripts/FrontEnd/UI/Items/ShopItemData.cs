using UnityEngine;

public class ShopItemData{
    public string name;
    public string description;
    public ItemType itemType;
    public int price;
    public ResourceType priceType;
    public int priceSubType;
    public Sprite icon;

    public ShopItemData(string name, string description, ItemType itemType, int price, ResourceType priceType, int priceSubType, Sprite icon){
        this.name = name;
        this.description = description;
        this.itemType = itemType;
        this.price = price;
        this.priceType = priceType;
        this.priceSubType = priceSubType;
        this.icon = icon;
    }
}
