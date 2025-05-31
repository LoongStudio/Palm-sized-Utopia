using UnityEngine;

[System.Serializable]
public class ResourceData : ScriptableObject
{
    [Header("基本信息")]
    public ResourceSubType resourceType; // 资源类型
    public ResourceBaseType baseType; // 资源基础类型
    public string displayName; // 资源名
    public string description; // 描述
    public Sprite icon; // 资源图标
    
    [Header("经济属性")]
    public int basePrice; // 资源价格
    public bool canBuy; // 是否可购买
    public bool canSell; // 是否可销售
    
    [Header("转化属性")]
    public bool canTransform; 
    public TransformationRecipe[] transformationRecipes; // 转化所需资源
}

[System.Serializable]
public class TransformationRecipe
{
    public ResourceCost[] inputResources;
    public ResourceCost[] outputResources;
    public float baseDuration;
    public BuildingSubType requiredBuilding;
}

[System.Serializable]
public class ResourceCost
{
    public ResourceSubType resourceType;
    public int amount;
}
