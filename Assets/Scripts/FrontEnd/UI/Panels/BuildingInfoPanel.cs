using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

public class BuildingInfoPanel : BasePanel{
    [SerializeField, ReadOnly, LabelText("建筑")] private Building building;

    // 基础信息
    [SerializeField, BoxGroup("基础信息")] private TextMeshProUGUI buildingName;
    [SerializeField, BoxGroup("基础信息")] private TextMeshProUGUI level;
    [SerializeField, BoxGroup("基础信息")] private Button levelUpButton;
    [SerializeField, BoxGroup("基础信息")] private TextMeshProUGUI workInfo;

    [SerializeField, BoxGroup("子面板")] private GameObject NPCSlotInfo;
    [SerializeField, BoxGroup("子面板")] private GameObject BuffSlotInfo;
    [SerializeField, BoxGroup("子面板")] private GameObject InventorySlotInfo;

    // 各个小面板的父物体
    [SerializeField, BoxGroup("各个小面板的父物体")] private Transform npcSlotParent;
    [SerializeField, BoxGroup("各个小面板的父物体")] private Transform BuffSlotParent;
    [SerializeField, BoxGroup("各个小面板的父物体")] private Transform InventorySlotParent;

    // 引用
    [SerializeField, BoxGroup("引用")] private AmountInfo npcAmountInfo;
    [SerializeField, BoxGroup("引用")] private Button closeButton;
    [SerializeField, BoxGroup("引用")] private TextMeshProUGUI efficiency;
    [SerializeField, BoxGroup("引用")] private TextMeshProUGUI percentSign;


    [SerializeField, BoxGroup("各个小面板的预制体")] private GameObject npcSlotItemPrefab;
    [SerializeField, BoxGroup("各个小面板的预制体")] private GameObject BuffSlotItemPrefab;
    [SerializeField, BoxGroup("各个小面板的预制体")] private GameObject InventorySlotItemPrefab;

    protected override void Awake(){
        base.Awake();
        
    }

    private void OnEnable(){
        levelUpButton.onClick.AddListener(OnLevelUpButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    private void OnDisable(){
        levelUpButton.onClick.RemoveListener(OnLevelUpButtonClick);
        closeButton.onClick.RemoveListener(OnCloseButtonClick);
    }
    private void RegisterEvents(){
        GameEvents.OnNPCLocked += RefreshNPCSlotInfo;
        GameEvents.OnNPCUnlocked += RefreshNPCSlotInfo;
        GameEvents.OnNPCInWorkingPosition += RefreshNPCSlotInfo;
        GameEvents.OnNPCInWorkingPosition += RefreshBuffSlotInfo;
        GameEvents.OnNPCLeaveWorkingPosition += RefreshNPCSlotInfo;
        GameEvents.OnNPCLeaveWorkingPosition += RefreshBuffSlotInfo;
        GameEvents.OnResourceChanged += RefreshInventorySlotInfo;
    }
    private void UnregisterEvents(){
        GameEvents.OnNPCLocked -= RefreshNPCSlotInfo;
        GameEvents.OnNPCUnlocked -= RefreshNPCSlotInfo;
        GameEvents.OnNPCInWorkingPosition -= RefreshNPCSlotInfo;
        GameEvents.OnNPCInWorkingPosition -= RefreshBuffSlotInfo;
        GameEvents.OnNPCLeaveWorkingPosition -= RefreshNPCSlotInfo;
        GameEvents.OnNPCLeaveWorkingPosition -= RefreshBuffSlotInfo;
        GameEvents.OnResourceChanged -= RefreshInventorySlotInfo;
    }
    protected override void OnShow(){
        base.OnShow();
        RegisterEvents();
        RefreshBuildingInfo();
    }

    protected override void OnHide(){
        base.OnHide();
        UnregisterEvents();
        // 取消对应建筑的选中效果
        building.OnDeselect();
    }
    protected override void OnOpen(){
        base.OnOpen();
        RegisterEvents();
    }
    protected override void OnClose(){
        base.OnClose();
        UnregisterEvents();
    }
    protected override void OnInitialize(){
        base.OnInitialize();
        RefreshBuildingInfo();
    }
    private void RefreshBuildingInfo(){
        building = SelectManager.Instance.Selected.GetComponentInChildren<Building>();

        if(building == null){
            Debug.LogError("[BuildingInfoPanel] 建筑为空");
            return;
        }
        // 刷新基础信息
        RefreshBasicInfo();
        // 刷新NPC槽位信息
        RefreshNPCSlotInfo();
        // 刷新Buff槽位信息
        RefreshBuffSlotInfo();
        // 刷新Inventory槽位信息
        RefreshInventorySlotInfo();
    }
    private void RefreshBasicInfo(){
        buildingName.text = building.data.buildingName;
        level.text = (building.currentLevel + 1).ToString();
        // TODO：根据当前建筑类型和正在进行的任务，显示工作信息
    }
    private void RefreshNPCSlotInfo(NPCEventArgs args = null){
        // 如果与当前建筑无关，则不刷新，防止性能浪费
        if(args != null){
            if(args.relatedBuilding != building){
                return;
            }
        }
        // 刷新NPC槽位数量信息
        npcAmountInfo.SetInfo(building.assignedNPCs.Count, building.NPCSlotAmount);
        // 清除所有NPC槽位
        foreach(Transform child in npcSlotParent){
            Destroy(child.gameObject);
        }
        // 根据已经分配的NPC，刷新NPC槽位信息，未分配的槽位用空槽位预制体填充
        for(int i = 0; i < building.NPCSlotAmount; i++){
            GenerateNPCSlotItem(i);
        }
    }
    private void GenerateNPCSlotItem(int index){
        GameObject npcSlotItem = Instantiate(npcSlotItemPrefab, npcSlotParent);
        NPCSlotItem itemComponent = npcSlotItem.GetComponent<NPCSlotItem>();
        if(itemComponent != null){
            int assignedNPCsCount = building.assignedNPCs.Count;
            if(index < assignedNPCsCount){
                // 如果该槽位有NPC
                NPC npc = building.assignedNPCs[index];
                itemComponent.SetUp(building, npc);
            }else{
                itemComponent.SetUp(building, null);
            }
        }else{
            Debug.LogError("[BuildingInfoPanel] NPCSlotItem组件为空");
        }
    }
    private void RefreshBuffSlotInfo(NPCEventArgs args = null){
        if(args != null){
            if(args.relatedBuilding != building){
                return;
            }
        }
        // 刷新Buff总百分比和颜色
        RefreshBuffBaseInfo();

        // 清除所有Buff槽位
        foreach(Transform child in BuffSlotParent){
            Destroy(child.gameObject);
        }
        // 根据当前建筑的Buff，刷新Buff槽位信息
        // 1. NPC槽位Buff
        Buff npcSlotBuff = building.GetNPCSlotBuff();
        if(npcSlotBuff.intensity > 0){
            GenerateBuffSlotItem(npcSlotBuff);
        }
        // 2. NPC之间的关系Buff
        Buff friendWorkTogetherBuff = building.GetFriendWorkTogetherBuff();
        if(friendWorkTogetherBuff.intensity > 0){
            GenerateBuffSlotItem(friendWorkTogetherBuff);
        }
        // 3. NPC自身加成Buff
        Buff inSlotNPCBuff = building.GetInSlotNPCBuff();
        if(inSlotNPCBuff.intensity > 0){
            GenerateBuffSlotItem(inSlotNPCBuff);
        }
        // 4. 其他施加的Buff
        foreach(var buff in building.Buffs){
            GenerateBuffSlotItem(buff);
        }

    }
    private void GenerateBuffSlotItem(Buff buff){
        GameObject buffSlotItem = Instantiate(BuffSlotItemPrefab, BuffSlotParent);
        BuffSlotItem itemComponent = buffSlotItem.GetComponent<BuffSlotItem>();
        if(itemComponent != null){
            itemComponent.SetUp(buff.type, buff.intensity);
        }else{
            Debug.LogError("[BuildingInfoPanel] BuffSlotItem组件为空");
        }
    }
    private void GenerateBuffSlotItem(KeyValuePair<BuffEnums, int> buff){
        GameObject buffSlotItem = Instantiate(BuffSlotItemPrefab, BuffSlotParent);
        BuffSlotItem itemComponent = buffSlotItem.GetComponent<BuffSlotItem>();
        if(itemComponent != null){
            itemComponent.SetUp(buff.Key, buff.Value);
        }else{
            Debug.LogError("[BuildingInfoPanel] BuffSlotItem组件为空");
        }
    }
    private void RefreshBuffBaseInfo(){
        // 如果建筑不是生产建筑，则隐藏Buff信息
        if(building is not ProductionBuilding){
            SetBuffBaseInfoVisible(false);
            return;
        }
        // 如果建筑是生产建筑，则显示Buff信息
        SetBuffBaseInfoVisible(true);
        ProductionBuilding productionBuilding = building as ProductionBuilding;
        int effi = (int)(productionBuilding.productionSpeedMultiplier * 100);
        // 更新效率数字和颜色
        SetEfficiencyNumber(effi);
    }
    private void SetBuffBaseInfoVisible(bool visible){
        efficiency.color = new Color(1, 1, 1, visible ? 1 : 0);
        percentSign.color = new Color(1, 1, 1, visible ? 1 : 0);
    }
    private void SetEfficiencyNumber(int effi){
        // 设置数字
        efficiency.text = effi.ToString();
        // 设置颜色，0-50为红色，50-100为黄色，100以上为绿色
        Color color = effi > 50 ? Color.green : Color.red;
        color = effi > 100 ? Color.yellow : color;
        efficiency.color = color;
        percentSign.color = color;  // 百分号颜色与效率颜色相同
    }
    private void RefreshInventorySlotInfo(ResourceEventArgs args = null){
        if(args != null){
            // 如果与自己无关，则不刷新
            if(args.relatedBuildingInventory != building.inventory){
                return;
            }
        }
        // 清除所有Inventory槽位
        foreach(Transform child in InventorySlotParent){
            Destroy(child.gameObject);
        }
        // 直接遍历所有资源堆栈，只显示数量大于0的
        foreach(ResourceStack resource in building.inventory.currentStacks){
            if(resource.amount > 0){
                GenerateInventorySlotItem(resource);
            }
        }
    }
    private void GenerateInventorySlotItem(ResourceStack resource){
        GameObject inventorySlotItem = Instantiate(InventorySlotItemPrefab, InventorySlotParent);
        InventorySlotItem itemComponent = inventorySlotItem.GetComponent<InventorySlotItem>();
        if(itemComponent != null){
            itemComponent.SetUp(resource);
        }else{
            Debug.LogError("[BuildingInfoPanel] InventorySlotItem组件为空");
        }
    }

    private void OnLevelUpButtonClick(){
        // 升级建筑
        Debug.LogWarning("[BuildingInfoPanel] 升级建筑按钮功能暂未实装");
    }
    private void OnCloseButtonClick(){
        Hide();
    }
}