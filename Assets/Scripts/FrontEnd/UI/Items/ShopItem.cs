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
    [SerializeField] private Button buyButton;

    
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
    }
    private void OnBuyButtonClick(){
        // 
        
    }
}