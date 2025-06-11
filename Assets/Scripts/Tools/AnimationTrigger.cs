using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    private NPC npc;
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
    }
    public void ExitSocialPreparation()
    {
        Debug.Log("[AnimationTrigger] ExitSocialPreparation");
        npc.ChangeState(NPCState.MovingToSocial);
    }
}
