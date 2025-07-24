using UnityEngine;
using TMPro;

public class AmountInfo : MonoBehaviour{
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI maxAmountText;
    public void SetCurrentAmount(int amount){
        amountText.text = amount.ToString();
    }
    public void SetMaxAmount(int maxAmount){
        maxAmountText.text = maxAmount.ToString();
    }
    public void SetInfo(int amount, int maxAmount){
        SetCurrentAmount(amount);
        SetMaxAmount(maxAmount);
    }
}