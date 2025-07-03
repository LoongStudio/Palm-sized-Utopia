using UnityEngine;
using UnityEngine.UI;

public class BasePanel : MonoBehaviour
{
    protected string panelName;
    protected bool isRemoved = false;
    protected bool isShow = false;
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
        gameObject.SetActive(true);
        isShow = true;
        OnShow();
    }
    /// <summary>
    /// 隐藏面板
    /// </summary>
    public virtual void Hide()
    {
        isShow = false;
        gameObject.SetActive(false);
        OnHide();
    }
    // 子类重写
    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}