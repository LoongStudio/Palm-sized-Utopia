using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;


public class ResourcePanel : BasePanel{
    [Required("金币数量文本不能为空")]
    [SerializeField] private TextMeshProUGUI coinAmountText;
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // 显示初始金币数量
        coinAmountText.text = ResourceManager.Instance.GetResourceAmount(ResourceType.Coin, CoinSubType.Gold).ToString();
    }
    private void OnEnable()
    {
        GameEvents.OnResourceChanged += OnResourceChanged;
    }
    private void OnDisable()
    {
        GameEvents.OnResourceChanged -= OnResourceChanged;
    }
    // 资源变化时更新UI
    private void OnResourceChanged(ResourceEventArgs args)
    {
        coinAmountText.text = args.newAmount.ToString();
    }
}