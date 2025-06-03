using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Data", menuName = "Utopia/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("基础信息")]
    public ResourceType type;
    public int subType;
    public string displayName;
    public string description;
    public Sprite icon;
    
    [Header("交易信息")]
    public bool canBePurchased;
    public bool canBeSold;
    
    // 获取类型安全的子类型
    public T GetSubType<T>() where T : System.Enum
    {
        return ResourceSubTypeHelper.FromInt<T>(subType);
    }
    
    // 获取子类型的显示名称
    public string GetSubTypeName()
    {
        return ResourceSubTypeHelper.GetSubTypeName(type, subType);
    }
    
    // 验证子类型是否有效
    public bool IsValidSubType()
    {
        return ResourceSubTypeHelper.IsValidSubType(type, subType);
    }
} 

