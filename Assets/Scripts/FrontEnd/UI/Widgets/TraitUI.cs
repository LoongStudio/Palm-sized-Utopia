using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class TraitUI : MonoBehaviour{
    [LabelText("词条名称"), SerializeField] private TextMeshProUGUI traitName;
    [LabelText("背景图片"), SerializeField] private Image icon;
    /// <summary>
    /// 根据不同的词条设置词条的名称和背景图片颜色,确保对比度足够
    /// </summary>
    /// <param name="traitType"></param>
    public void SetUp(NPCTraitType traitType){
        switch(traitType){
            case NPCTraitType.SocialMaster:
                traitName.text = "社交大师";
                traitName.color = Color.white;
                icon.color = Color.magenta;
                break;
            case NPCTraitType.Bootlicker:
                traitName.text = "舔狗";
                traitName.color = Color.black;
                icon.color = Color.yellow;
                break;
            case NPCTraitType.FarmExpert:
                traitName.text = "倪哥";
                traitName.color = Color.white;
                icon.color = Color.black;
                break;
            case NPCTraitType.LivestockExpert:
                traitName.text = "蒙古人";
                traitName.color = Color.black;
                icon.color = Color.white;
                break;
            case NPCTraitType.CheapLabor:
                traitName.text = "工贼";
                traitName.color = Color.black;
                icon.color = Color.green;
                break;
            case NPCTraitType.NightOwl:
                traitName.text = "夜猫子";
                traitName.color = Color.black;
                icon.color = Color.gray;
                break;
            case NPCTraitType.EarlyBird:
                traitName.text = "早鸟";
                traitName.color = Color.white;
                icon.color = Color.blue;
                break;
            default:
                traitName.text = "未知";
                traitName.color = Color.white;
                icon.color = Color.white;
                break;
        }

    }
}