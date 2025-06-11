using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private NPC npc;
    [SerializeField] private NPCState state = NPCState.Idle;
    private void Awake()
    {
        if (npc == null)
        {
            npc = GetComponent<NPC>();
            if (npc == null)
            {
                Debug.LogError("[AnimationTrigger] NPC is not found");
            }
        }
        Debug.Log($"[AnimationTrigger] {npc.name} 初始化完成");
    }
    public void ExitSocialPreparation()
    {
        Debug.Log("[AnimationTrigger] ExitSocialPreparation");
        npc.ChangeState(NPCState.MovingToSocial);
    }

    [ContextMenu("Test Change To State")]
    public void TestChangeStateTo(){
        npc.ChangeState(state);
    }
}
