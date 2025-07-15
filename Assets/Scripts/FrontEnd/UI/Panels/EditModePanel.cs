using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class EditModePanel : BasePanel
{
    [SerializeField] private Button editModeButton;
    private bool isEditMode = false;
    private void OnEnable()
    {
        GameEvents.OnEditModeChanged += HandleEditModeChanged;
        editModeButton.onClick.AddListener(HandleEditModeButtonClick);
    }
    private void OnDisable()
    {
        GameEvents.OnEditModeChanged -= HandleEditModeChanged;
        editModeButton.onClick.RemoveListener(HandleEditModeButtonClick);
    }
    private void Start()
    {
    }

    private void HandleEditModeButtonClick()
    {
        isEditMode = !isEditMode;
        InputManager.Instance.ToggleEditMode();
        editModeButton.GetComponentInChildren<TextMeshProUGUI>().text = isEditMode ? 
            UITools.GetLocalizedText("EditMode", "FontText") : 
            UITools.GetLocalizedText("EditMode", "NormalMode");
    }

    public void HandleEditModeChanged(bool isEditMode)
    {
        this.isEditMode = isEditMode;
        editModeButton.GetComponentInChildren<TextMeshProUGUI>().text = isEditMode ? 
            UITools.GetLocalizedText("EditMode", "FontText") : 
            UITools.GetLocalizedText("EditMode", "NormalMode");
    }
}