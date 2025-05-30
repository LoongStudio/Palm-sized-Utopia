using UnityEngine;

public interface Produceable
{
    public int producePerSec { get; set; }
    public int maxStorageAmount { get; set; }
    public int currentStorageAmount { get; set; }
    public bool canBeCollected { get; set; }
    public int Produce();
    public int BeCollect();
}