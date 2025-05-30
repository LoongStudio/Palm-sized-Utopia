// 资源基础类型
public enum ResourceBaseType
{
    Seed,       // 种子
    Crop,       // 作物
    Feed,       // 饲料
    Livestock,  // 种畜
    Animal,     // 牲畜
    Coin,       // 金币
    RewardTicket // 奖励券
}

// 资源子类型
public enum ResourceSubType
{
    // 种子类
    Seed,
    WheatSeed,
    CornSeed,
    
    // 作物类
    Crop,
    Wheat,
    Corn,
    
    // 饲料类
    Feed,
    AnimalFeed,
    
    // 种畜类
    Breeding,
    BreedingCow,
    BreedingSheep,
    
    // 牲畜类
    Livestock,
    Cow,
    Sheep,
    
    // 货币类
    Coin,
    Ticket
}

// 转化任务状态
public enum TransformationTaskState
{
    Pending,    // 等待中
    Processing, // 进行中
    Paused,     // 暂停
    Completed,  // 完成
    Cancelled   // 取消
}

// 建筑基础类型
public enum BuildingBaseType
{
    Production, // 生产类
    Boost,      // 加成类
    Decoration, // 装饰类
    Housing     // 住房类
}

// 建筑子类型
public enum BuildingSubType
{
    // 生产类
    Farm,           // 农田
    Ranch,          // 牧场
    TradeMarket,    // 贸易市场
    
    // 加成类
    WaterTower,     // 水塔
    CompostYard,    // 堆肥场
    Warehouse,      // 仓库
    
    // 装饰类
    Garden,         // 花园
    
    // 住房类
    House           // 房屋
}

// 建筑状态
public enum BuildingState
{
    Planning,       // 规划中
    Constructing,   // 建造中
    Operational,    // 运营中
    Upgrading,      // 升级中
    Demolished      // 已拆除
}

// 设备类型
public enum EquipmentType
{
    // 生产类设备
    Irrigation,         // 灌溉机
    GrasslandManagement, // 草场管理系统
    
    // 加成类设备
    ManagementDevice,   // 管理设备
    WaterPump          // 抽水设备
}

// NPC状态
public enum NPCState
{
    Idle,           // 空闲
    Working,        // 工作
    Resting,        // 休息
    Socializing,    // 社交
    
    // 中间状态
    IdleToWork,     // 空闲→工作
    IdleToRest,     // 空闲→休息
    WorkToIdle,     // 工作→空闲
    RestToIdle      // 休息→空闲
}

// NPC性格类型
public enum PersonalityType
{
    Hardworking,    // 勤劳
    Lazy,           // 懒惰
    Honest,         // 诚实
    Deceitful,      // 虚伪
    Kind,           // 善良
    Evil            // 邪恶
}

// NPC个性词条
public enum PersonalityTrait
{
    SocialMaster,   // 交友大师
    Bootlicker,     // 舔狗
    FarmExpert,     // 倪哥（农业专家）
    RanchExpert,    // 蒙古人（养殖专家）
    CheapLabor,     // 工贼（低薪资）
    MaintenanceMaster // 保养大师
}

// 解锁条件类型
public enum UnlockConditionType
{
    BuildingExists,     // 存在特定建筑
    BuildingLevel,      // 建筑达到特定等级
    ResourceAmount,     // 资源数量达到要求
    NPCCount           // NPC数量达到要求
}

// 报告类型
public enum ReportType
{
    Daily,      // 日报
    Weekly,     // 周报
    Monthly     // 月报
}

// 好感度等级
public enum RelationshipLevel
{
    Hostile,    // 敌对 (0-20)
    Dislike,    // 不喜欢 (21-40)
    Neutral,    // 中性 (41-60)
    Friendly,   // 友好 (61-80)
    Close       // 亲密 (81-100)
}
