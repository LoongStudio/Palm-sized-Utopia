using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buildingButton;
    [SerializeField] private Button seedButton;
    [SerializeField] private Button NPCButton;
    private void Awake()
    {

    }
    private void Start()
    {
        closeButton.onClick.AddListener(OnHidePanel);
        buildingButton.onClick.AddListener(OnBuildingButtonClick);
        seedButton.onClick.AddListener(OnSeedButtonClick);
        NPCButton.onClick.AddListener(OnNPCButtonClick);
    }
    private void OnHidePanel()
    {
        UIManager.Instance.HidePanel("ShopPanel");
    }
    private void OnBuildingButtonClick()
    {
        BuildingManager.Instance.BuyBuilding(BuildingSubType.Farm);
        UIManager.Instance.HidePanel("ShopPanel");
    }
    private void OnSeedButtonClick()
    {
        throw new System.NotImplementedException("种子购买按钮功能尚未实现");
    }
    private void OnNPCButtonClick()
    {
        NPCManager.Instance.HireNPC();
    }
}