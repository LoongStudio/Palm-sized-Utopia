using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class NPCInfoPanel : BasePanel
{
    [SerializeField, ReadOnly, LabelText("NPC")] private NPC npc;
    private bool _isSameNPC = false;
    [LabelText("关闭按钮"), SerializeField] private Button closeButton;
    [BoxGroup("子面板"), LabelText("NPC基础信息面板"), SerializeField] private NPCBaseInfo npcBaseInfo;
    [BoxGroup("子面板"), LabelText("NPC能力面板"), SerializeField] private NPCAbilityInfo npcAbilityInfo;
    [BoxGroup("子面板"), LabelText("NPC物品"), SerializeField] private InventorySlotItem npcInventorySlot;
    [BoxGroup("子面板"), LabelText("NPC社交信息面板"), SerializeField] private NPCSocialInfo npcSocialInfo;
    protected override void Awake(){
        base.Awake();
    }

    #region 事件订阅和处理
    protected override void RegisterEvents(){
        base.RegisterEvents();
        closeButton.onClick.AddListener(OnCloseButtonClick);
        GameEvents.OnNPCStateChanged += OnNPCStateChanged;
        GameEvents.OnResourceChanged += OnResourceChanged;
    }
    protected override void UnregisterEvents(){
        closeButton.onClick.RemoveListener(OnCloseButtonClick);
        GameEvents.OnNPCStateChanged -= OnNPCStateChanged;
        GameEvents.OnResourceChanged -= OnResourceChanged;
    }
    private void OnNPCStateChanged(NPCEventArgs args){
        if(args.npc == npc){
            // 更新基础信息（主要是状态部分）
            npcBaseInfo.UpdateSelf();
        }
    }
    private void OnResourceChanged(ResourceEventArgs args){
        if(args.relatedNPCInventory == npc.inventory){
            // 更新Inventory信息
            SetUpNPCInventory();
        }
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
        NPC newNPC = SelectManager.Instance.Selected.GetComponentInChildren<NPC>();
        if(newNPC != null){
            if(npc == newNPC){
                _isSameNPC = true;
            }
            else{
                _isSameNPC = false;
                npc = newNPC;
            }
        }
        if(npc != null)
        {
            // 设置NPC基础信息
            npcBaseInfo.SetUp(npc);
            // 如果NPC是同一个，则不更新能力信息，因为npc的这个信息不会发生变化
            if (!_isSameNPC)
            {
                npcAbilityInfo.SetUp(npc);
            }
            // 设置NPC物品
            SetUpNPCInventory();
            // 设置NPC社交信息
            npcSocialInfo.SetUp(npc);
        }
        else
        {
            Debug.LogError("[NPCInfoPanel] NPC为空");
        }
    }

    private void SetUpNPCInventory()
    {
        if (npc.inventory != null && npc.inventory.currentStacks.Count > 0)
        {
            var resource = npc.inventory.currentStacks[0];
            if (resource != null && resource.amount > 0)
            {
                npcInventorySlot.gameObject.SetActive(true);
                npcInventorySlot.SetUp(resource);
            }
            else
            {
                npcInventorySlot.gameObject.SetActive(false);
            }
        }
        else
        {
            npcInventorySlot.gameObject.SetActive(false);
        }
    }
}