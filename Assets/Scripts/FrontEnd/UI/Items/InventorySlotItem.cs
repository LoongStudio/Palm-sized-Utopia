using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class InventorySlotItem : MonoBehaviour{
    [SerializeField] private Image icon;
    [SerializeField] private AmountInfo amountInfo;

    private void Awake(){
        icon = GetComponentInChildren<Image>();
        if(icon == null){
            Debug.LogError("[InventorySlotItem] 缺少Icon组件");
        }
        amountInfo = GetComponentInChildren<AmountInfo>();
        if(amountInfo == null){
            Debug.LogError("[InventorySlotItem] 缺少AmountInfo组件");
        }
    }

    public void SetUp(ResourceStack stack){
        icon.sprite = stack.resourceConfig.icon;
        amountInfo.SetInfo(stack.amount, stack.storageLimit);
    }
}