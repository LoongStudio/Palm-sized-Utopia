using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
[RequireComponent(typeof(CanvasGroup))]
public class BasePanel : MonoBehaviour
{
    protected string panelName;
    protected bool isRemoved = false;
    protected bool isShow = false;
    [Required("CanvasGroup组件不能为空")]
    protected CanvasGroup canvasGroup;
    protected virtual void Awake()
    {
        Initialize();
    }
    private void Initialize(){
        InitializeCanvasGroup();

        OnInitialize();
    }
    /// <summary>
    /// 初始化CanvasGroup组件
    /// </summary>
    private void InitializeCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError($"BasePanel: {gameObject.name} 缺少 CanvasGroup 组件！");
            }
        }
    }
    /// <summary>
    /// 打开面板
    /// </summary>
    public virtual void Open(string panelName)
    {
        this.panelName = panelName;
        gameObject.SetActive(true);
        isShow = true;
        OnOpen();
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    public virtual void Close()
    {
        isRemoved = true;
        isShow = false;
        gameObject.SetActive(false);
        Destroy(gameObject);
        OnClose();
    }
    /// <summary>
    /// 显示面板
    /// </summary>
    public virtual void Show()
    {
        InitializeCanvasGroup(); // 确保canvasGroup已初始化
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }
        isShow = true;
        OnShow();
    }
    /// <summary>
    /// 隐藏面板
    /// </summary>
    public virtual void Hide()
    {
        isShow = false;
        InitializeCanvasGroup(); // 确保canvasGroup已初始化
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
        OnHide();
    }
    // 子类重写
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
    protected virtual void OnInitialize() { }
}