using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class NPCSlotItem : MonoBehaviour{
    // 引用
    private Building building;
    private NPC npc;
    [SerializeField, ReadOnly]private Button itemButton;
    [SerializeField, ReadOnly]private Image avatar;
    [SerializeField]private Image lockIcon;
    [SerializeField]private Button lockButton;

    // 配置
    [SerializeField, LabelText("头像")] private Sprite avatarImage;
    [SerializeField, LabelText("空槽位")] private Sprite emptySlotImage;

    [SerializeField, LabelText("默认图标（未锁定）")] private Sprite defaultLockIcon;
    [SerializeField, LabelText("锁定图标")] private Sprite lockedIcon;
    [SerializeField, ReadOnly, LabelText("是否锁定")] private bool isLocked = false;
    private void Awake(){
        itemButton = GetComponent<Button>();
        if(itemButton == null){
            Debug.LogError("[NPCSlotItem] 按钮组件为空");
        }
        avatar = GetComponent<Image>();
        if(avatar == null){
            Debug.LogError("[NPCSlotItem] 头像组件为空");
        }
    }
    private void OnEnable(){
        itemButton.onClick.AddListener(OnItemButtonClick);
        lockButton.onClick.AddListener(OnLockButtonClick);
    }
    private void OnDisable(){
        itemButton.onClick.RemoveListener(OnItemButtonClick);
        lockButton.onClick.RemoveListener(OnLockButtonClick);
    }
    private void OnItemButtonClick(){
        if(npc == null){
            // TODO: 空槽位点击逻辑
            Debug.LogWarning("[NPCSlotItem] 空槽位点击");
        }
        else{
            // TODO: 有NPC的槽位点击逻辑
            Debug.LogWarning("[NPCSlotItem] 有NPC的槽位点击");
        }
    }

    private void OnLockButtonClick()
    {   
        Debug.Log($"[NPCSlotItem] 锁定按钮点击，NPC：{npc.data.npcName}，建筑：{building.data.buildingName}");
        bool lockSuccess = false;
        if(building.IsNPCLocked(npc)){
            // 如果已经锁定，则尝试解锁
            lockSuccess = npc.TryUnlockCurrentTask();
        }
        else{
            // 如果未锁定，则尝试锁定
            lockSuccess = npc.TryLockCurrentTask();
        }

        isLocked = lockSuccess;
        SetLockIcon();
    }

    private void SetLockIcon()
    {
        if (isLocked)
        {
            lockIcon.sprite = lockedIcon;
        }
        else
        {
            lockIcon.sprite = defaultLockIcon;
        }
    }

    public void SetUp(Building building, NPC npc, bool isLocked){
        this.building = building;
        if(building == null){
            Debug.LogError("[NPCSlotItem] 建筑为空");
            return;
        }
        // 根据NPC是否为空，设置槽位信息
        if(npc == null){
            avatar.sprite = emptySlotImage;

            // 锁定图标全透明并禁用锁定按钮
            lockIcon.color = new Color(1, 1, 1, 0);
            lockButton.interactable = false;
        }else{
            this.npc = npc;
            avatar.sprite = avatarImage;

            // 解锁图标不透明并启用锁定按钮
            lockIcon.color = new Color(1, 1, 1, 1);
            lockButton.interactable = true;

            // 设置锁定状态
            this.isLocked = isLocked;
            SetLockIcon();
        }
    }
}