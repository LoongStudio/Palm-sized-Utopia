public class NPCEventArgs 
{
    public NPC npc;
    public NPCData npcData;
    public NPCEventType eventType;
    public NPCState oldState;
    public NPCState newState;
    public Building relatedBuilding;
    public NPC otherNPC; // 用于社交事件
    public NPCState shouldChangeStateTo;
    public int relationshipChange;
    public System.DateTime timestamp;
    
    public enum NPCEventType 
    {
        Hired,
        Instantiated,
        Fired,
        Destroyed,
        StateChanged,
        AssignedToBuilding,
        RemovedFromBuilding,
        RelationshipChanged,
        SocialInteraction
    }

    // public NPCEventArgs(NPC npc, NPCEventType eventType)
    // {
    //     this.npc = npc;
    //     this.eventType = eventType;
    //     this.oldState = npc.previousState;
    //     this.newState = npc.currentState;
    //     this.relatedBuilding = npc.assignedBuilding;
    //     this.otherNPC = null;
    //     this.relationshipChange = 0;
    //     this.timestamp = System.DateTime.Now;
    // }
    // public NPCEventArgs(NPC npc, NPCEventType eventType, NPCState oldState, NPCState newState, Building relatedBuilding, NPC otherNPC, int relationshipChange)
    // {
    //     this.npc = npc;
    //     this.eventType = eventType;
    //     this.oldState = oldState;
    //     this.newState = newState;
    //     this.relatedBuilding = relatedBuilding;
    //     this.otherNPC = otherNPC;
    //     this.relationshipChange = relationshipChange;
    //     this.timestamp = System.DateTime.Now;
    // }
}