using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;


public class NPCSocialInfo : WidgetBase{
    private NPC _npc;
    [SerializeField, LabelText("社交关系组件预制体")] private GameObject relationshipItemPrefab;
    [SerializeField, LabelText("社交关系组件父对象")] private Transform relationshipItemParent;
    private void Awake(){
        if(relationshipItemPrefab == null){
            Debug.LogError("[NPCSocialInfo] 社交关系组件预制体为空");
        }
    }

    public void SetUp(NPC npc){
        _npc = npc;
        UpdateSelf();
    }
    public override void UpdateSelf(){
        // 清空所有子对象
        foreach(Transform child in relationshipItemParent){
            Destroy(child.gameObject);
        }
        // 创建社交关系组件
        Dictionary<NPC, int> relationships = NPCManager.Instance.socialSystem.GetAllRelationshipsFor(_npc);
        var sortedRelationships = relationships.OrderByDescending(r => r.Value).ToList();
        foreach(var relationship in sortedRelationships){
            NPC partner = relationship.Key;
            int relationshipValue = relationship.Value;
            GameObject item = Instantiate(relationshipItemPrefab, relationshipItemParent);
            RelationshipItem relationshipItem = item.GetComponent<RelationshipItem>();
            relationshipItem.SetUp(partner, _npc);
        }
    }
}