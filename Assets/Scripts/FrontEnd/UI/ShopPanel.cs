using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buildingButton;
    [SerializeField] private Button seedButton;
    [SerializeField] private Button NPCButton;
    private void OnEnable()
    {
        closeButton.onClick.AddListener(Hide);
        buildingButton.onClick.AddListener(OnBuildingButtonClick);
        seedButton.onClick.AddListener(OnSeedButtonClick);
        NPCButton.onClick.AddListener(OnNPCButtonClick);
    }
    private void Start()
    {
    }
    private void OnDisable()
    {
        closeButton.onClick.RemoveListener(Hide);
        buildingButton.onClick.RemoveListener(OnBuildingButtonClick);
        seedButton.onClick.RemoveListener(OnSeedButtonClick);
        NPCButton.onClick.RemoveListener(OnNPCButtonClick);
    }

    private void OnBuildingButtonClick()
    {
        BuildingManager.Instance.BuyBuilding(BuildingSubType.Farm);
        Hide();
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