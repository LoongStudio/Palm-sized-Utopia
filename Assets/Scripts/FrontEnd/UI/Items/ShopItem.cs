using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour{
    // 数据
    private ShopItemData shopItemData;

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
    
    public void SetUp(ShopItemData shopItemData){
        this.shopItemData = shopItemData;
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
        Debug.Log("购买物品：" + shopItemData.name);
        
    }
}

public enum ItemType{
    Building,
    Land,
    NPC,
    Resource,
}