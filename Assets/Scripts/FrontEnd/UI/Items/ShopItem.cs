using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour{
    // 数据
    private ShopItemData shopItemData;
    // 父面板
    private BasePanel panel;

    // UI组件
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Button buyButton;

    // 其它设置
    [SerializeField] private Color normalColor = new Color(219, 200, 114, 255);
    
    [SerializeField] private Color insufficientColor = new Color(165, 165, 165, 255);
    
    private void OnEnable(){
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }
    private void OnDisable(){
        buyButton.onClick.RemoveListener(OnBuyButtonClick);
    }
    
    public void SetUp(BasePanel panel, ShopItemData shopItemData){
        this.shopItemData = shopItemData;
        this.panel = panel;
        itemIcon.sprite = shopItemData.icon;
        itemName.text = shopItemData.name;
        this.price.text = shopItemData.price.ToString();

        SetBuyButton();
    }
    public void SetBuyButton(){
        // 根据是否有足够资源购买设置样式
        if(ResourceManager.Instance){
            // 如果资源不足，则禁用按钮，调整颜色
            if(!ResourceManager.Instance.HasEnoughResource(shopItemData.priceType, shopItemData.priceSubType, shopItemData.price)){
                buyButton.interactable = false;
                buttonImage.color = insufficientColor;
                canvasGroup.alpha = 0.25f;
            }
            else{
                // 如果资源足够，则启用按钮，调整颜色
                buyButton.interactable = true;
                buttonImage.color = normalColor;
                canvasGroup.alpha = 1;
            }
        }
    }
    private void OnBuyButtonClick(){
        // TODO：购买逻辑
        // 总体逻辑：根据shopItemData执行不同的购买逻辑
        // 1. 如果是建筑，则购买建筑
        if(shopItemData.itemType == ItemType.Building){
            TriggerBuildingBought();
        }
        // 2. 如果是土地，则购买土地
        else if(shopItemData.itemType == ItemType.Land){
            TriggerLandBought();
        }
        // 3. 如果是NPC，则雇佣NPC
        else if(shopItemData.itemType == ItemType.NPC){
            TriggerNPCBought();
        }
        // 4. 如果是资源，则购买资源
        else if(shopItemData.itemType == ItemType.Resource){
            TriggerResourceBought();
        }
        // 5. TODO: 其他购买逻辑，比如奖励券
        
        // 关闭所在商店面板
        panel.Hide();
    }

    private void TriggerBuildingBought(){
        BuildingManager.Instance.BuyBuilding(shopItemData.buildingSubType);
    }
    private void TriggerLandBought(){
        var eventArgs = new BuildingEventArgs(){
            placeableType = shopItemData.placeableType,
            eventType = BuildingEventArgs.BuildingEventType.LandBought,
        };
        GameEvents.TriggerLandBought(eventArgs);
    }
    private void TriggerNPCBought(){

    }
    private void TriggerResourceBought(){

    }
}

public enum ItemType{
    Building,
    Land,
    NPC,
    Resource,
}