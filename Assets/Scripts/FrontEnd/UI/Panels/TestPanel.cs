using UnityEngine;
using UnityEngine.UI;

public class TestPanel : BasePanel
{
    [SerializeField] private Button shopButton;

    private void OnEnable()
    {
        shopButton.onClick.AddListener(OnOpenShopPanel);
        GameEvents.OnEditModeChanged += OnEditModeChanged;
    }
    private void OnDisable()
    {
        shopButton.onClick.RemoveListener(OnOpenShopPanel);
        GameEvents.OnEditModeChanged -= OnEditModeChanged;
    }
    private void Start()
    {

    }
    private void OnOpenShopPanel()
    {
        UIManager.Instance.OpenPanel("ShopPanel");
    }
    protected override void OnHide()
    {
        // 自身隐藏时，商店界面也隐藏
        UIManager.Instance.HidePanel("ShopPanel");
    }
    private void OnEditModeChanged(bool isEditMode)
    {
        // 编辑模式下，隐藏自身
        if (isEditMode)
        {
            Hide();
        }
        else
        {
            // 非编辑模式下，显示自身
            Show();
        }
    }
}