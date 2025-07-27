using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;


public class NPCBaseInfo : WidgetBase
{
    private NPC npc;
    [BoxGroup("NPC信息"), LabelText("名字"), SerializeField] private TextMeshProUGUI npcName;
    [BoxGroup("NPC信息"), LabelText("头像"), SerializeField] private Image avatar;
    [BoxGroup("NPC信息/当前状态"), LabelText("动词"), SerializeField] private TextMeshProUGUI verb;
    [BoxGroup("NPC信息/当前状态"), LabelText("介词"), SerializeField] private TextMeshProUGUI prep;
    [BoxGroup("NPC信息/当前状态"), LabelText("对象"), SerializeField] private TextMeshProUGUI objectName;

    public void SetUp(NPC npc){
        this.npc = npc;
        UpdateSelf();
    }
    public override void UpdateSelf(){
        npcName.text = npc.data.npcName;
        UpdateStateText();
        // 让所有TextMeshProUGUI组件的宽度自适应
        AutoAdjustTextWidth(npcName);
        AutoAdjustTextWidth(verb);
        AutoAdjustTextWidth(prep);
        AutoAdjustTextWidth(objectName);
    }
    public void UpdateStateText(){
        var state = npc.stateMachine.CurrentState;
        if(state == NPCState.Idle){
            verb.text = "Idle";
            prep.text = "";
            objectName.text = "";
        }else if(state == NPCState.PrepareForSocial){
            verb.text = "Looking";
            prep.text = "for";
            objectName.text = "partner";
        }
        else if(state == NPCState.MovingToSocial){
            verb.text = "Moving";
            prep.text = "to";
            objectName.text = "Social";
        }
        else if(state == NPCState.Social){
            verb.text = "Socializing";
            prep.text = "with";
            NPC otherNPC = NPCManager.Instance.socialSystem.GetSocialPartner(npc);
            objectName.text = otherNPC.data.npcName;
        }
        else if(state == NPCState.MovingToWork){
            Building building = npc.AssignedTask.building;
            if(building != null){
                verb.text = "Moving";
                prep.text = "to";
                objectName.text = building.data.buildingName;
            }
            else{
                verb.text = "Finding";
                prep.text = "a";
                objectName.text = "job";
            }
        }
        else if(state == NPCState.Working){
            Building building = npc.AssignedTask.building;
            if(building == null){
                return;
            }
            switch(npc.AssignedTask.taskType){
                case TaskType.Production:
                    verb.text = "Producing";
                    prep.text = "at";
                    objectName.text = building.data.buildingName;
                    break;
                case TaskType.HandlingAccept:
                    verb.text = "Picking";
                    prep.text = "at";
                    objectName.text = building.data.buildingName;
                    break;
                case TaskType.HandlingDrop:
                    verb.text = "Dropping";
                    prep.text = "at";
                    objectName.text = building.data.buildingName;
                    break;
            }
        }
        else if(state == NPCState.MovingHome){
            verb.text = "Moving";
            prep.text = "";
            objectName.text = "Home";
        }
        else if(state == NPCState.Sleeping){
            verb.text = "Sleeping";
            prep.text = "";
            objectName.text = "";
        }
    }
}