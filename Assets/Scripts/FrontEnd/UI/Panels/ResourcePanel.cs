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
        coinAmountText.text = ResourceManager.Instance.GetResourceAmount(ResourceManager.Instance.Gold).ToString();
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
        // 如果资源类型是金币，则更新金币数量
        if(args.resourceType == ResourceManager.Instance.Gold.type){
            coinAmountText.text = args.newAmount.ToString();
        }
        // TODO: 如有必要的话，更新Ticket数量
    }
}