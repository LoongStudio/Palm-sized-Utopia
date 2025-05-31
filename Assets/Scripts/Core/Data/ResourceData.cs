using UnityEngine;

[System.Serializable]
public class ResourceData
{
    public ResourceType resourceType;
    public int subType;
    public int amount;
    public int stackLimit;
    public int storageLimit;
    public bool canBePurchased;
    public int purchasePrice;
    public int sellPrice;
} 