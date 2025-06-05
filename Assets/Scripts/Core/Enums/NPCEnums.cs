// NPC状态
public enum NPCState
{
    Working,   // 工作
    Resting,   // 休息
    Idle,      // 空闲
    Social,    // 社交
    Transition, // 状态转换中
    MovingToSource,      // 前往采集点
    Collecting,          // 采集中
    MovingToDestination, // 前往目的地
    Delivering,          // 卸货中
    WaitingForTask       // 等待任务
}

// NPC性格
public enum NPCPersonalityType
{
    Diligent,   // 勤劳
    Lazy,       // 懒惰
    Honest,     // 诚实
    Hypocritical, // 虚伪
    Kind,       // 善良
    Evil        // 邪恶
}

// NPC词条
public enum NPCTraitType
{
    SocialMaster,    // 交友大师
    Bootlicker,      // 舔狗
    FarmExpert,      // 倪哥
    LivestockExpert, // 蒙古人
    CheapLabor,       // 工贼
    NightOwl,        // 夜猫子
    EarlyBird,       // 早起鸟

} 