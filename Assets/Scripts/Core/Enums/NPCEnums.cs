// NPC状态
public enum NPCState
{
    Generated, // 生成
    Idle,      // 空闲
    PrepareForSocial, // 准备社交
    MovingToSocial, // 前往社交位置
    Social,    // 社交中
    SocialEndHappy, // 社交结束（友好）
    SocialEndFight, // 社交结束（争吵）
    MovingToWork,   // 前往工作
    Working,        // 进行工作
    Transporting,   // 运输阶段
    WorkComplete,   // 完成工作
    MovingHome,     // 回家阶段
    Sleeping        // 休眠阶段
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