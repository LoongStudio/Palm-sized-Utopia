using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class UIManager : SingletonManager<UIManager>
{
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;

    private Transform _root;
    private Dictionary<string, string> pathDict;

    [Header("缓存字典")]
    private Dictionary<string, GameObject> prefabDict;  // 预制件
    private Dictionary<string, BasePanel> panelDict;  // 已打开的面板

    public Transform UIRoot
    {
        get
        {
            if (_root == null)
            {
                if (GameObject.Find("Canvas"))
                {
                    _root = GameObject.Find("Canvas").transform;
                }
                else
                {
                    _root = new GameObject("Canvas").transform;
                }
            }
            return _root;
        }
    }
    protected override void Awake()
    {
        base.Awake();
        InitDicts();
    }
    private void OnEnable()
    {
        RegisterEvents();
    }
    private void OnDisable()
    {
        UnregisterEvents();
    }

    #region 事件与处理
    private void RegisterEvents()
    {
        GameEvents.OnResourceInsufficient += OnResourceInsufficient;
        GameEvents.OnResourceBoughtClicked += OnResourceBoughtClicked;
        GameEvents.OnBuildingSelected += OnBuildingSelected;
        GameEvents.OnNPCSelected += OnNPCSelected;
    }
    private void UnregisterEvents()
    {
        GameEvents.OnResourceInsufficient -= OnResourceInsufficient;
        GameEvents.OnResourceBoughtClicked -= OnResourceBoughtClicked;
        GameEvents.OnBuildingSelected -= OnBuildingSelected;
        GameEvents.OnNPCSelected -= OnNPCSelected;
    }
    private void OnResourceInsufficient(ResourceEventArgs args)
    {
        OpenPanel("InsufficientResourcePanel");
        // 设置资源不足文本
        InsufficientResourcePanel panel = panelDict["InsufficientResourcePanel"] as InsufficientResourcePanel;
        if(panel != null)
        {
            panel.SetInsufficientResourceText(args.newAmount + " " + args.resourceType.ToString());
        }
    }
    private void OnResourceBoughtClicked(ResourceEventArgs args)
    {
        // 打开并设置数量选择面板
        OpenPanel("QuantitySelectPanel");
        QuantitySelectPanel panel = panelDict["QuantitySelectPanel"] as QuantitySelectPanel;
        if(panel != null)
        {
            panel.SetUpPanel(args);
        }else{
            Debug.LogError("QuantitySelectPanel 未找到");
        }
    }
    private void OnBuildingSelected(Building building)
    {
        if(building != null){
            OpenPanel("BuildingInfoPanel");
        }
    }
    private void OnNPCSelected(NPC npc)
    {
        
    }
    #endregion
    private void Start()
    {
        OpenPanel("TestPanel");
        OpenPanel("EditModePanel");
        OpenPanel("ResourcePanel");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 如果点击了空白处，关闭商店面板
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                TryClosePanelsOnClick();
                TryHidePanelsOnClick();
            }
        }
    }


    /// <summary>
    /// 对于不常驻的面板，如果打开了，则关闭
    /// </summary>
    private void TryClosePanelsOnClick()
    {
        ClosePanel("ShopPanel");
        ClosePanel("PlaceableShopPanel");
        ClosePanel("InsufficientResourcePanel");
        ClosePanel("BuildingShopPanel");
        // 如果资源商店打开了
        if (panelDict.ContainsKey("ResourceShopPanel") && panelDict["ResourceShopPanel"].IsShowing)
        {
            // 但是没有打开数量选择面板，则关闭资源商店
            if (!panelDict.ContainsKey("QuantitySelectPanel") || !panelDict["QuantitySelectPanel"].IsShowing)
            {
                ClosePanel("ResourceShopPanel");
            }
            // 如果打开了数量选择面板，则关闭数量选择面板
            else
            {
                ClosePanel("QuantitySelectPanel");
            }
        }
    }
    /// <summary>
    /// 对于常驻的面板，如果打开了，则隐藏
    /// </summary>
    private void TryHidePanelsOnClick(){

    }
    private void InitDicts()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();

        pathDict = new Dictionary<string, string>();
        pathDict.Add("TestPanel", UIConst.TestPanel);
        pathDict.Add("ShopPanel", UIConst.ShopPanel);
        pathDict.Add("EditModePanel", UIConst.EditModePanel);
        pathDict.Add("PlaceableShopPanel", UIConst.PlaceableShopPanel);
        pathDict.Add("InsufficientResourcePanel", UIConst.InsufficientResourcePanel);
        pathDict.Add("ResourcePanel", UIConst.ResourcePanel);
        pathDict.Add("BuildingShopPanel", UIConst.BuildingShopPanel);
        pathDict.Add("ResourceShopPanel", UIConst.ResourceShopPanel);
        pathDict.Add("QuantitySelectPanel", UIConst.QuantitySelectPanel);
        pathDict.Add("BuildingInfoPanel", UIConst.BuildingInfoPanel);
    }
    public BasePanel OpenPanel(string panelName)
    {
        // 检查是否已打开
        BasePanel panel = null;
        if (panelDict.TryGetValue(panelName, out panel))
        {
            if (showDebugInfo)
            {
                Debug.LogWarning($"UI: {panelName} 已打开");
            }
            panel.Show();
            return panel;
        }

        // 检查是否有配置路径
        string path = "";
        if (!pathDict.TryGetValue(panelName, out path))
        {
            if (showDebugInfo)
            {
                Debug.LogError($"UI: {panelName} 未配置路径");
            }
            return null;
        }

        // 加载预制件
        GameObject panelPrefab = null;
        if (!prefabDict.TryGetValue(panelName, out panelPrefab))
        {
            string prefabPath = path;
            panelPrefab = Resources.Load<GameObject>(prefabPath) as GameObject;
            if (panelPrefab == null)
            {
                Debug.LogError($"UI: {panelName} 未找到预制件");
                return null;
            }
            prefabDict.Add(panelName, panelPrefab);
        }

        // 打开界面
        GameObject panelObj = Instantiate(panelPrefab, UIRoot, false);
        panel = panelObj.GetComponent<BasePanel>();
        if (panel == null)
        {
            Debug.LogError($"UI: {panelName} 未找到面板组件");
            return null;
        }
        panelDict.Add(panelName, panel);
        panel.Open(panelName);
        return panel;
    }
    public BasePanel ShowPanel(string panelName)
    {
        BasePanel panel = null;
        if (panelDict.TryGetValue(panelName, out panel))
        {
            panel.Show();
            return panel;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"UI: {panelName} 未打开");
        }
        return null;
    }
    public bool HidePanel(string panelName)
    {
        BasePanel panel = null;
        if (panelDict.TryGetValue(panelName, out panel))
        {
            panel.Hide();
            return true;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"UI: {panelName} 未打开");
        }
        return false;
    }
    public bool ClosePanel(string panelName)
    {
        BasePanel panel = null;
        if (panelDict.TryGetValue(panelName, out panel))
        {
            panel.Close();
            panelDict.Remove(panelName);
            return true;
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning($"UI: {panelName} 未打开");
        }
        return false;
    }
}

public class UIConst
{
    public const string TestPanel = "UI/Panels/TestPanel";
    public const string ShopPanel = "UI/Panels/ShopPanel";
    public const string EditModePanel = "UI/Panels/EditModePanel";
    public const string PlaceableShopPanel = "UI/Panels/PlaceableShopPanel";
    public const string InsufficientResourcePanel = "UI/Panels/InsufficientResourcePanel";
    public const string ResourcePanel = "UI/Panels/ResourcePanel";
    public const string BuildingShopPanel = "UI/Panels/BuildingShopPanel";
    public const string ResourceShopPanel = "UI/Panels/ResourceShopPanel";
    public const string QuantitySelectPanel = "UI/Panels/QuantitySelectPanel";
    public const string BuildingInfoPanel = "UI/Panels/BuildingInfoPanel";
}