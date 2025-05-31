using System;
using System.Collections.Generic;
using UnityEngine;
using DataType;
using TMPro;
using UnityEngine.UI;

public class StorageAttributes : MonoBehaviour
{
    public float interval = 1f;
    public int transferRate = 5; // 每秒最多转移几个物品

    private float timer = 0f;

    // 当前在仓库区域内的农夫
    private readonly HashSet<BehaviorController> farmersInRange = new HashSet<BehaviorController>();

    // 仓库数据（你可以换成 ScriptableObject 或其他管理器）
    public Dictionary<Items, int> warehouseInventory = new Dictionary<Items, int>();
    
    // UI
    public GameObject itemPrefab;
    
    void Update()
    {
        if (farmersInRange.Count == 0) return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;

            foreach (var farmer in farmersInRange)
            {
                if (farmer == null) continue;
                // bool isEmpty = true;
                // 遍历所有物品种类
                foreach (Items item in System.Enum.GetValues(typeof(Items)))
                {
                    int carried = farmer.GetItemAmount(item);
                    if (carried > 0)
                    {
                        int transferAmount = Mathf.Min(carried, transferRate);
                        // if (transferAmount > 0) isEmpty = false;
                        // 从农夫背包中扣除
                        farmer.RemoveItem(item, transferAmount);

                        // 存入仓库
                        if (!warehouseInventory.ContainsKey(item))
                            warehouseInventory[item] = 0;

                        warehouseInventory[item] += transferAmount;

                        Debug.Log($"[{farmer.name}] 转移 {item}: {transferAmount} -> 仓库当前数量: {warehouseInventory[item]}");
                    }
                    
                }
                
            }
            
            // 更新 UI 内容
            Transform contentParent = GetComponent<ClickToShowUI>()
                .currentUI
                .GetComponentInChildren<ScrollRect>()
                .content;
            for (int i = contentParent.childCount - 1; i >= 0; i--)
                Destroy(contentParent.GetChild(i).gameObject);
            foreach (var item in warehouseInventory.Keys)
            {
                GameObject itemText = Instantiate(itemPrefab, contentParent);
                itemText.GetComponent<TMP_Text>().text = item + " - " + warehouseInventory[item];
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BehaviorController farmer = other.GetComponent<BehaviorController>();
            if (farmer != null && farmer.currentNPCState == NPCStates.Walking)
            {
                farmersInRange.Add(farmer);
                farmer.currentNPCState = NPCStates.Storaging;
                Debug.Log("农夫 " + farmer.name + " 开始卸货");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BehaviorController farmer = other.GetComponent<BehaviorController>();
            if (farmer != null && farmer.currentNPCState == NPCStates.Walking)
            {
                farmersInRange.Add(farmer);
                farmer.currentNPCState = NPCStates.Storaging;
                Debug.Log("农夫 " + farmer.name + " 开始卸货");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            BehaviorController farmer = other.GetComponent<BehaviorController>();
            if (farmer != null)
            {
                farmersInRange.Remove(farmer);
                // farmer.currentNPCState = NPCStates.Wandering;
                Debug.Log("农夫 " + farmer.name + " 离开仓库区域");
                farmer.EnterWanderingMode();
            }
        }
    }
}
