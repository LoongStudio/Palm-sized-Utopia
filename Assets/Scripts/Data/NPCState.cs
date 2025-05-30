using UnityEngine;

public abstract class NPCStateBase
{
    protected NPCInstance npc;
    
    public NPCStateBase(NPCInstance npcInstance)
    {
        npc = npcInstance;
    }
    
    public abstract void Enter();
    public abstract void Update(float deltaTime);
    public abstract void Exit();
    public abstract bool CanTransitionTo(NPCState newState);
}

public class IdleState : NPCStateBase
{
    public IdleState(NPCInstance npc) : base(npc) { }
    
    public override void Enter() { }
    public override void Update(float deltaTime) { }
    public override void Exit() { }
    public override bool CanTransitionTo(NPCState newState) { return false; }
    
    // 寻找其他NPC进行社交
    private void LookForSocialInteraction() { }
    
    // 随机移动
    private void RandomMovement() { }
}

public class WorkingState : NPCStateBase
{
    public WorkingState(NPCInstance npc) : base(npc) { }
    
    public override void Enter() { }
    public override void Update(float deltaTime) { }
    public override void Exit() { }
    public override bool CanTransitionTo(NPCState newState) { return false; }
    
    // 执行工作任务
    private void PerformWork() { }
    
    // 检查是否需要休息
    private bool ShouldTakeBreak() { return false; }
}

public class RestingState : NPCStateBase
{
    public RestingState(NPCInstance npc) : base(npc) { }
    
    public override void Enter() { }
    public override void Update(float deltaTime) { }
    public override void Exit() { }
    public override bool CanTransitionTo(NPCState newState) { return false; }
    
    // 前往休息地点
    private void GoToRestLocation() { }
    
    // 恢复体力或心情
    private void RestoreEnergy() { }
}

public class SocializingState : NPCStateBase
{
    public SocializingState(NPCInstance npc) : base(npc) { }
    
    public override void Enter() { }
    public override void Update(float deltaTime) { }
    public override void Exit() { }
    public override bool CanTransitionTo(NPCState newState) { return false; }
    
    // 进行对话
    private void Converse() { }
    
    // 检查是否发生争吵
    private bool CheckForArgument() { return false; }
    
    // 结束社交
    private void EndSocial() { }
}
