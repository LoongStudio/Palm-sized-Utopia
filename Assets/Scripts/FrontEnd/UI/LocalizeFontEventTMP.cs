using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizeFontEventTMP : MonoBehaviour
{
    [Tooltip("本地化TMP字体资源引用")]
    [SerializeField] private LocalizedAsset<TMP_FontAsset> localizedTMPFont;

    [Tooltip("字体加载完成后触发的事件")]
    public UnityEvent<TMP_FontAsset> onFontLoaded;

    private TextMeshProUGUI targetTMPText;
    private AsyncOperationHandle<TMP_FontAsset> fontLoadHandle;

    private void Awake()
    {
        targetTMPText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (localizedTMPFont == null)
        {
            Debug.LogError("未设置LocalizedTMPFont引用！", this);
            return;
        }

        // 注册字体变更事件
        localizedTMPFont.AssetChanged += OnFontChanged;

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
        if (localizedTMPFont != null)
        {
            localizedTMPFont.AssetChanged -= OnFontChanged;
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
            Debug.LogError("本地化系统初始化失败，无法加载TMP字体", this);
        }
    }

    private void LoadFont()
    {
        if (fontLoadHandle.IsValid())
        {
            fontLoadHandle.Completed -= OnFontLoadCompleted;
        }

        fontLoadHandle = localizedTMPFont.LoadAssetAsync();
        fontLoadHandle.Completed += OnFontLoadCompleted;
    }

    private void OnFontLoadCompleted(AsyncOperationHandle<TMP_FontAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            OnFontChanged(handle.Result);
        }
        else
        {
            Debug.LogError($"TMP字体加载失败: {handle.OperationException}", this);
        }
    }

    private void OnFontChanged(TMP_FontAsset newFont)
    {
        if (targetTMPText != null && newFont != null)
        {
            targetTMPText.font = newFont;
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