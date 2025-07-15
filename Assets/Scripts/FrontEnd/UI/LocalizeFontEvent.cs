using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(UnityEngine.UI.Text))]
public class LocalizeFontEvent : MonoBehaviour
{
    [Tooltip("本地化字体资源引用")]
    [SerializeField] private LocalizedAsset<Font> localizedFont;

    [Tooltip("字体加载完成后触发的事件")]
    public UnityEvent<Font> onFontLoaded;

    private UnityEngine.UI.Text targetText;
    private AsyncOperationHandle<Font> fontLoadHandle;

    private void Awake()
    {
        targetText = GetComponent<UnityEngine.UI.Text>();
    }

    private void OnEnable()
    {
        if (localizedFont == null)
        {
            Debug.LogError("未设置LocalizedFont引用！", this);
            return;
        }

        // 注册字体变更事件
        localizedFont.AssetChanged += OnFontChanged;

        // 检查本地化系统是否初始化完成
        if (LocalizationSettings.InitializationOperation.IsDone)
        {
            LoadFont();
        }
        else
        {
            LocalizationSettings.InitializationOperation.Completed += OnLocalizationInitialized;
        }
    }

    private void OnDisable()
    {
        if (localizedFont != null)
        {
            localizedFont.AssetChanged -= OnFontChanged;
        }

        // 取消异步操作
        if (fontLoadHandle.IsValid())
        {
            fontLoadHandle.Completed -= OnFontLoadCompleted;
        }

        LocalizationSettings.InitializationOperation.Completed -= OnLocalizationInitialized;
    }

    private void OnLocalizationInitialized(AsyncOperationHandle<LocalizationSettings> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            LoadFont();
        }
        else
        {
            Debug.LogError("本地化系统初始化失败，无法加载字体", this);
        }
    }

    private void LoadFont()
    {
        if (fontLoadHandle.IsValid())
        {
            fontLoadHandle.Completed -= OnFontLoadCompleted;
        }

        fontLoadHandle = localizedFont.LoadAssetAsync();
        fontLoadHandle.Completed += OnFontLoadCompleted;
    }

    private void OnFontLoadCompleted(AsyncOperationHandle<Font> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            OnFontChanged(handle.Result);
        }
        else
        {
            Debug.LogError($"字体加载失败: {handle.OperationException}", this);
        }
    }

    private void OnFontChanged(Font newFont)
    {
        if (targetText != null && newFont != null)
        {
            targetText.font = newFont;
            onFontLoaded?.Invoke(newFont); // 触发外部事件
        }
    }

    private void OnDestroy()
    {
        // 释放资源句柄（新版本API）
        if (fontLoadHandle.IsValid())
        {
            Addressables.Release(fontLoadHandle); // 使用Addressables静态方法
        }
    }
}