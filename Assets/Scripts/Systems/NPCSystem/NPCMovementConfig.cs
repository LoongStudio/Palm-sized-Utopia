using UnityEngine;

[CreateAssetMenu(fileName = "NPCMovementConfig", menuName = "Utopia/NPC Movement Config")]
public class NPCMovementConfig : ScriptableObject
{
    public float stoppingDistance = 0.05f;
    public float moveSpeed = 1f;
    public float acceleration = 1f;
    public float turnSpeed = 5f;
    public float turnThreshold = 10f;
    public float moveRadius = 10f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 2f;
    public bool enableTurnBeforeMove = true;
}