using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class NPCAbilityInfo : WidgetBase{
    private NPC npc;
    [LabelText("性格"), SerializeField] private TextMeshProUGUI personality;
    [LabelText("词条根节点"), SerializeField] private Transform traitsParent;
    [LabelText("词条预制体"), SerializeField] private GameObject traitPrefab;
    [LabelText("效率加成"), SerializeField] private TextMeshProUGUI efficiencyBonus;

    public void SetUp(NPC npc){
        this.npc = npc;
        UpdateSelf();
    }
    public override void UpdateSelf(){
        // 性格
        personality.text = npc.data.personality.ToString();
        // 词条
        foreach(Transform child in traitsParent){
            Destroy(child.gameObject);
        }
        foreach(NPCTraitType trait in npc.data.traits){
            GameObject traitObj = Instantiate(traitPrefab, traitsParent);
            TraitUI traitUI = traitObj.GetComponent<TraitUI>();
            traitUI.SetUp(trait);
        }
        // 效率加成
        efficiencyBonus.text = npc.data.baseWorkAbility.ToString();
    }
}