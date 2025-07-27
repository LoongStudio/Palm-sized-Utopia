using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class NPCInfoPanel : BasePanel
{
    [SerializeField, ReadOnly, LabelText("NPC")] private NPC npc;
    [LabelText("关闭按钮"), SerializeField] private Button closeButton;
    [BoxGroup("子面板"), LabelText("NPC基础信息面板"), SerializeField] private NPCBaseInfo npcBaseInfo;
    [BoxGroup("子面板"), LabelText("NPC能力面板"), SerializeField] private NPCAbilityInfo npcAbilityInfo;

    protected override void Awake(){
        base.Awake();
    }
    private void OnEnable(){
        RegisterEvents();
    }
    private void OnDisable(){
        UnregisterEvents();
    }

    #region 事件订阅和处理
    private void RegisterEvents(){
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }
    private void UnregisterEvents(){
        closeButton.onClick.RemoveListener(OnCloseButtonClick);
    }
    private void OnCloseButtonClick(){
        Hide();
    }
    #endregion

    #region UI生命周期
    protected override void OnShow(){
        base.OnShow();
        RefreshNPCInfo();
    }
    protected override void OnHide(){
        base.OnHide();
        // 取消对应NPC的选中效果
        npc.OnDeselect();
    }
    protected override void OnOpen(){
        base.OnOpen();
    }
    protected override void OnClose(){
        base.OnClose();
    }
    protected override void OnInitialize(){
        base.OnInitialize();
        RefreshNPCInfo();
    }
    #endregion

    private void RefreshNPCInfo(){
        npc = SelectManager.Instance.Selected.GetComponentInChildren<NPC>();

        if(npc != null){
            npcBaseInfo.SetUp(npc);
            npcAbilityInfo.SetUp(npc);
        }
        else{
            Debug.LogError("[NPCInfoPanel] NPC为空");
        }
    }

}