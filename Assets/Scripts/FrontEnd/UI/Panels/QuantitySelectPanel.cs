using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuantitySelectPanel : BasePanel{
    public TMP_InputField quantityInputField;
    public Slider quantitySlider;
    public Button confirmButton;

    private int maxQuantity = 0;
    // 为要触发的资源购买事件准备的参数
    private ResourceType costType;
    private int costSubType;
    private int cost; // 单价
    private int costAmount; // 总价
    private ResourceType resourceTypeToBuy;
    private int resourceSubTypeToBuy;
    private int resourceAmountToBuy = 0;

    public void SetUpPanel(ResourceEventArgs args){

        int playerResourceAmount = ResourceManager.Instance.GetResourceAmount(args.costType, args.costSubType);
        int price = args.cost;
        this.maxQuantity = Mathf.FloorToInt(playerResourceAmount / price);

        // 设置slider的值
        quantitySlider.minValue = 1;
        quantitySlider.maxValue = maxQuantity > 0? maxQuantity : 1;
        quantitySlider.value = 1;

        // 设置inputField的值
        quantityInputField.text = "1";

        // 事件设置
        quantitySlider.onValueChanged.AddListener(OnSliderChanged);
        quantityInputField.onValueChanged.AddListener(OnInputChanged);
        confirmButton.onClick.AddListener(OnConfirm);

        // 设置购买和花费的资源类型和数量
        resourceTypeToBuy = args.resourceType;
        resourceSubTypeToBuy = args.subType;
        cost = args.cost;
        costType = args.costType;
        costSubType = args.costSubType;
    }

    private void OnSliderChanged(float value){
        quantityInputField.text = Mathf.RoundToInt(value).ToString();
    }

    private void OnInputChanged(string value){
        if(int.TryParse(value, out int quantity)){
            quantity = Mathf.Clamp(quantity, 1, maxQuantity);
            quantitySlider.SetValueWithoutNotify(quantity);
            quantityInputField.text = quantity.ToString();
        }
    }

    private void OnConfirm(){
        resourceAmountToBuy = Mathf.RoundToInt(quantitySlider.value);
        costAmount = resourceAmountToBuy * cost;
        
        // 触发购买确认事件
        Debug.Log("花费 " + costAmount + " " + costType + " " + costSubType + " 购买 " + resourceAmountToBuy + " " + resourceTypeToBuy + " " + resourceSubTypeToBuy);
        GameEvents.TriggerResourceBoughtConfirmed(new ResourceEventArgs(){
            // 想买什么
            resourceType = resourceTypeToBuy,
            subType = resourceSubTypeToBuy,
            newAmount = resourceAmountToBuy,

            // 花了多少
            cost = cost,
            costType = costType,
            costSubType = costSubType,
        });
        // 取消事件
        quantitySlider.onValueChanged.RemoveListener(OnSliderChanged);
        quantityInputField.onValueChanged.RemoveListener(OnInputChanged);
        confirmButton.onClick.RemoveListener(OnConfirm);

        // 隐藏面板
        Hide();
    }
}