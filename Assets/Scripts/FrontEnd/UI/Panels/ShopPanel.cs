using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : BasePanel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button baseBlockButton;
    [SerializeField] private Button buildingButton;
    [SerializeField] private Button seedButton;
    [SerializeField] private Button NPCButton;
    private void OnEnable()
    {
        closeButton.onClick.AddListener(Hide);
        baseBlockButton.onClick.AddListener(OnBaseBlockButtonClick);
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
        baseBlockButton.onClick.RemoveListener(OnBaseBlockButtonClick);
        buildingButton.onClick.RemoveListener(OnBuildingButtonClick);
        seedButton.onClick.RemoveListener(OnSeedButtonClick);
        NPCButton.onClick.RemoveListener(OnNPCButtonClick);
    }

    private void OnBaseBlockButtonClick()
    {
        UIManager.Instance.OpenPanel("PlaceableShopPanel");
        Hide();
    }
    private void OnBuildingButtonClick()
    {
        UIManager.Instance.OpenPanel("BuildingShopPanel");
        Hide();
    }
    private void OnSeedButtonClick()
    {
        UIManager.Instance.OpenPanel("ResourceShopPanel");
        Hide();
    }
    private void OnNPCButtonClick()
    {
        NPCManager.Instance.HireNPC();
    }

}