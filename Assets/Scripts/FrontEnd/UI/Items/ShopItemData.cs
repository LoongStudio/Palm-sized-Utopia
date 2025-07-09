using UnityEngine;

public class ShopItemData{
    public string name;
    public string description;
    public int price;
    public ResourceType priceType;
    public Sprite icon;

    public ShopItemData(string name, string description, int price, ResourceType priceType, Sprite icon){
        this.name = name;
        this.description = description;
        this.price = price;
        this.priceType = priceType;
        this.icon = icon;
    }
}
