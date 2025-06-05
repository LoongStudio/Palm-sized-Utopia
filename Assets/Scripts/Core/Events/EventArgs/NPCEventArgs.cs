public class NPCEventArgs 
{
    public NPC npc;
    public NPCEventType eventType;
    public NPCState oldState;
    public NPCState newState;
    public Building relatedBuilding;
    public NPC otherNPC; // 用于社交事件
    public int relationshipChange;
    public System.DateTime timestamp;
    
    public enum NPCEventType 
    {
        Hired,
        Fired,
        StateChanged,
        AssignedToBuilding,
        RemovedFromBuilding,
        RelationshipChanged,
        SocialInteraction
    }
}