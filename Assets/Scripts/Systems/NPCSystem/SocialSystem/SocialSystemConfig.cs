using UnityEngine;

[CreateAssetMenu(fileName = "SocialSystemConfig", menuName = "Utopia/Social System Config")]
public class SocialSystemConfig : ScriptableObject
{
    [Header("基础设置")]
    public float interactionCheckInterval = 2f;
    public float interactionRadius = 3f;
    public float interactionDuration = 3f;
    public int maxDailyInteractions = 5;
    public float socialInteractionDistance = 2f;
    public float socialMoveSpeed = 0.5f;
    public float socialTimeout = 10f;
    
    [Header("好感度设置")]
    public int baseRelationshipChange = 5;
    public int fightRelationshipPenalty = -15;
    public int workTogetherBonus = 2;
    public int relationshipDecayDaily = -1;
    public int maxRelationship = 100;
    public int minRelationship = 0;
    public int defaultRelationship = 50;
    
    [Header("争吵概率设置")]
    public float baseFightChance = 0.2f;
    public float personalityConflictModifier = 0.3f;
    public float socialMasterFightReduction = 0.15f;
    public float bootlickerFightReduction = 0.1f;
    
    [Header("工作效率设置")]
    public float bestFriendWorkBonus = 0.15f;
    public float friendWorkBonus = 0.1f;
    public float neutralWorkBonus = 0.05f;
    public float enemyWorkPenalty = -0.1f;
}