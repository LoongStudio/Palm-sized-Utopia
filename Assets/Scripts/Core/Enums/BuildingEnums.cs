// 建筑类型
public enum BuildingType
{
    Production, // 生产类
    Buff,       // 加成类
    Decoration, // 装饰类
    Housing,    // 住房类
    Functional, // 功能类
}

// 建筑子类型
public enum BuildingSubType
{
    Farm,         // 农田
    Ranch,        // 牧场
    TradeMarket,  // 贸易市场
    WaterTower,   // 水塔
    CompostYard,  // 堆肥场
    Warehouse,    // 仓库
    Garden        // 花园
}

// 设备类型
public enum EquipmentType
{
    IrrigationSystem,           // 灌溉系统
    GrasslandManagementSystem   // 草场管理系统
}

// 建筑状态
public enum BuildingStatus
{
    Active,    // 活跃
    Inactive,  // 非活跃
    Upgrading  // 升级中
}