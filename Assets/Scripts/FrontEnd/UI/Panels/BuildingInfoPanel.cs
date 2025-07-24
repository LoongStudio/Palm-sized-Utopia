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
    [SerializeField] private TextMeshProUGUI buildingName;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private TextMeshProUGUI workInfo;

    // 各个小面板的父物体
    [SerializeField] private Transform npcSlotParent;
    [SerializeField] private Transform BuffSlotParent;
    [SerializeField] private Transform InventorySlotParent;

    // 引用
    [SerializeField] private AmountInfo npcAmountInfo;
    [SerializeField] private Button closeButton;

    // 各个小面板的预制体
    [SerializeField] private GameObject npcSlotItemPrefab;
    [SerializeField] private GameObject BuffSlotItemPrefab;
    [SerializeField] private GameObject InventorySlotItemPrefab;

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
    protected override void OnShow(){
        base.OnShow();
        RefreshBuildingInfo();
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
    private void RefreshNPCSlotInfo(){
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
                itemComponent.SetUp(building, npc, building.IsNPCLocked(npc));
            }else{
                itemComponent.SetUp(building, null, false);
            }
        }else{
            Debug.LogError("[BuildingInfoPanel] NPCSlotItem组件为空");
        }
    }
    private void RefreshBuffSlotInfo(){

    }
    private void RefreshInventorySlotInfo(){
        // 清除所有Inventory槽位
        foreach(Transform child in InventorySlotParent){
            Destroy(child.gameObject);
        }
        // 根据当前建筑的库存，刷新Inventory槽位信息
        List<ResourceStack> resources = building.inventory.currentStacks.Where(stack => stack.amount > 0).ToList();  // inventory.currentStacks中amount>0的数量
        foreach(ResourceStack resource in resources){
            GenerateInventorySlotItem(resource);
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