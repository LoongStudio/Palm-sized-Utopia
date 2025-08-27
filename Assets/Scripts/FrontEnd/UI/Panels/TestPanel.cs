using UnityEngine;
using UnityEngine.UI;

public class TestPanel : BasePanel
{
    [SerializeField] private Button shopButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;


    private void OnEnable()
    {
        shopButton.onClick.AddListener(OnOpenShopPanel);
        saveButton.onClick.AddListener(OnSaveGame);
        loadButton.onClick.AddListener(OnLoadGame);
        GameEvents.OnEditModeChanged += OnEditModeChanged;
    }
    private void OnDisable()
    {
        shopButton.onClick.RemoveListener(OnOpenShopPanel);
        saveButton.onClick.RemoveListener(OnSaveGame);
        loadButton.onClick.RemoveListener(OnLoadGame);
        GameEvents.OnEditModeChanged -= OnEditModeChanged;
    }
    private void Start()
    {

    }
    private void OnOpenShopPanel()
    {
        UIManager.Instance.OpenPanel("ShopPanel");
    }
    private void OnSaveGame()
    {
        GameManager.Instance.SaveGame();
    }
    private void OnLoadGame()
    {
        GameManager.Instance.LoadGame();
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