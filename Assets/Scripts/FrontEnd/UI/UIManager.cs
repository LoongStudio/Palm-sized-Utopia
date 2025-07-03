using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

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
    private void RegisterEvents()
    {
    }
    private void UnregisterEvents()
    {
    }

    private void Start()
    {
        OpenPanel("TestPanel");
        OpenPanel("EditModePanel");
    }

    private void InitDicts()
    {
        prefabDict = new Dictionary<string, GameObject>();
        panelDict = new Dictionary<string, BasePanel>();

        pathDict = new Dictionary<string, string>();
        pathDict.Add("TestPanel", UIConst.TestPanel);
        pathDict.Add("ShopPanel", UIConst.ShopPanel);
        pathDict.Add("EditModePanel", UIConst.EditModePanel);
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
}