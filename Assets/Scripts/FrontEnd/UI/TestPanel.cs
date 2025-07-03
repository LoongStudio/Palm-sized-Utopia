using UnityEngine;
using UnityEngine.UI;

public class TestPanel : BasePanel
{
    [SerializeField] private Button shopButton;
    private void Awake()
    {

    }   
    private void Start()
    {
        shopButton.onClick.AddListener(OnOpenShopPanel);
    }
    private void OnOpenShopPanel()
    {
        UIManager.Instance.OpenPanel("ShopPanel");
    }
}