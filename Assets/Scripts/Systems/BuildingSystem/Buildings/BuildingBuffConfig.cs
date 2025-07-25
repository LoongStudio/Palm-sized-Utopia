using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingBuffConfig", menuName = "Utopia/BuildingBuffConfig")]
public class BuildingBuffConfig : ScriptableObject{
    [SerializeField] private List<BuffIcon> buffIcons;
    public Sprite GetBuffIcon(BuffEnums type){
        return buffIcons.Find(b => b.type == type).icon;
    }
}

[System.Serializable]
public struct BuffIcon{
    public BuffEnums type;
    public Sprite icon;
}

