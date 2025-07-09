using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class ShopBasePanel : BasePanel{
    [SerializeField,Required] protected GridLayoutGroup content;
    [SerializeField,Required] protected Button closeButton;
    [SerializeField,Required,ReadOnly] protected GameObject shopItemPrefab;
    protected override void Awake(){
        base.Awake();
    }
    protected virtual void OnEnable(){
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }
    protected virtual void OnDisable(){
        closeButton.onClick.RemoveListener(OnCloseButtonClick);
    }
    #region 初始化
    protected override void OnInitialize()
    {
        // 加载预制体
        if (shopItemPrefab == null)
        {
            shopItemPrefab = Resources.Load<GameObject>("UI/Items/ShopItem");
            if (shopItemPrefab == null)
            {
                Debug.LogError("shopItemPrefab is null");
            }
        }
        // 根据预制体尺寸设置网格大小
        if (content != null)
        {
            Vector2 itemSize = shopItemPrefab.GetComponent<RectTransform>().rect.size;
            content.cellSize = itemSize;
        }
    }
    #endregion

    protected override void OnShow(){
        base.OnShow();
        RefreshItems();
    }
    /// <summary>
    /// 根据游戏状态刷新商店面板中的所有ShopItem组件的样式
    /// </summary>
    protected virtual void RefreshItems(){
        // 获取所有在该商店面板中的ShopItem组件
        ShopItem[] shopItems = GetComponentsInChildren<ShopItem>();
        // 遍历所有ShopItem组件，并设置样式
        foreach(var shopItem in shopItems){
            shopItem.SetBuyButton();
        }
    }

    #region 按钮事件
    /// <summary>
    /// 关闭按钮事件，默认隐藏自身并打开商店界面，子类可重写
    /// </summary>
    protected virtual void OnCloseButtonClick(){
        UIManager.Instance.OpenPanel("ShopPanel");
        Hide();
    }
    #endregion
}
