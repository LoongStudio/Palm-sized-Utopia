using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffSlotItem : MonoBehaviour{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI addtionalEfficiency;
    public void SetUp(BuffEnums type, int efficiency){
        // 根据Buff类型设置图标
        icon.sprite = BuildingManager.Instance.BuildingBuffConfig.GetBuffIcon(type);
        // 设置效率
        addtionalEfficiency.text = efficiency.ToString();
        // 设置效率颜色，0以上为绿色，0以下为红色
        addtionalEfficiency.color = efficiency > 0 ? Color.green : Color.red;
    }
}
