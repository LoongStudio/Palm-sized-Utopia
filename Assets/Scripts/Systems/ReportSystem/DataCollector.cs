using System.Collections.Generic;

public class DataCollector
{
    private Dictionary<System.DateTime, GameStateSnapshot> dailySnapshots;
    
    public void RecordResourceChange(ResourceType type, int oldAmount, int newAmount) { }
    public void RecordBuildingEvent(Building building) { }
    public void RecordNPCEvent(NPC npc) { }
    
    public GameStateSnapshot GetCurrentSnapshot() { return null; }
    public List<GameStateSnapshot> GetSnapshotsInRange(System.DateTime start, System.DateTime end) { return null; }
}

[System.Serializable]
public class GameStateSnapshot
{
    public System.DateTime timestamp;
    public Dictionary<ResourceType, int> resourceAmounts;
    public int buildingCount;
    public int npcCount;
    public int totalIncome;
    public int totalExpenses;
}