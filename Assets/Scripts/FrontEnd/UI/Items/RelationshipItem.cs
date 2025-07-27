using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class RelationshipItem : MonoBehaviour{
    private NPC _thisNPC;
    private NPC _sourceNPC;
    [SerializeField] private Image avatar;
    [SerializeField] private TextMeshProUGUI thisNPCName;
    [SerializeField] private AmountInfo relationshipInfo;

    public void SetUp(NPC thisNPC, NPC sourceNPC){
        _thisNPC = thisNPC;
        _sourceNPC = sourceNPC;
        thisNPCName.text = thisNPC.data.npcName;
        int relationship = NPCManager.Instance.socialSystem.GetRelationship(thisNPC, sourceNPC);
        int maxRelationship = NPCManager.Instance.socialSystem.maxRelationship;
        relationshipInfo.SetInfo(relationship, maxRelationship);
    }
}