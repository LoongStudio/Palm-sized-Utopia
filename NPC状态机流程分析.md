# NPC状态机流程分析

## 状态机概述

Palm-sized Utopia的NPC系统采用了完整的状态机模式，实现了智能的NPC行为管理。状态机包含14个不同的状态，覆盖了NPC从生成到日常生活的完整生命周期。

## 状态定义

### 核心状态枚举
```csharp
public enum NPCState
{
    Generated,        // 生成状态 - NPC刚被创建
    Idle,            // 空闲状态 - NPC在等待或休息
    PrepareForSocial, // 准备社交 - 准备开始社交活动
    MovingToSocial,   // 前往社交位置 - 移动到社交地点
    Social,          // 社交中 - 正在进行社交互动
    SocialEndHappy,   // 社交结束（友好） - 社交成功结束
    SocialEndFight,   // 社交结束（争吵） - 社交失败结束
    MovingToWork,     // 前往工作 - 移动到工作地点
    Working,          // 进行工作 - 在工作地点执行任务
    Transporting,     // 运输阶段 - 搬运资源或物品
    WorkComplete,     // 完成工作 - 工作完成后的状态
    MovingHome,       // 回家阶段 - 返回住所
    ArrivedHome,      // 到家阶段 - 到达住所
    Sleeping          // 休眠阶段 - 在住所休息
}
```

## 状态机核心架构

### 状态机组件 (NPCStateMachine)
- **状态字典**: 存储所有状态实例
- **当前状态**: 跟踪当前活跃状态
- **状态计时器**: 记录状态持续时间
- **状态切换**: 管理状态之间的转换

### 状态基类 (NPCStateBase)
- **生命周期方法**: EnterState, UpdateState, ExitState
- **状态属性**: 描述、超时设置、下一状态
- **调试支持**: 状态信息显示

## 默认NPC进入场景的完整流程

### 第一阶段：生成 (Generated State)

#### 1.1 状态初始化
```csharp
// NPCStateMachine.Start() 中设置初始状态
ChangeState(NPCState.Generated);
```

#### 1.2 生成状态行为 (NPCGeneratedState)
```csharp
protected override void OnEnterState()
{
    // 开始寻找住房
    SearchForHousing();
}

private void SearchForHousing()
{
    // 1. 查找场景中所有的HousingBuilding
    HousingBuilding[] housingBuildings = Object.FindObjectsByType<HousingBuilding>();
    
    // 2. 如果没有住房建筑，NPC被销毁
    if (housingBuildings.Length == 0) {
        Object.Destroy(npc.gameObject);
        return;
    }
    
    // 3. 尝试注册到有空位的住房
    foreach (var building in housingBuildings) {
        if (building.RegisterLivingNPC(npc)) {
            npc.housing = building;
            return; // 成功注册
        }
    }
    
    // 4. 如果所有住房都满了，NPC被销毁
    Object.Destroy(npc.gameObject);
}
```

#### 1.3 状态转换条件
```csharp
protected override void OnUpdateState()
{
    // 当NPC成功落地（找到住房）时，切换到空闲状态
    if(npc.isLanded){
        stateMachine.ChangeState(NPCState.Idle);
    }
}
```

### 第二阶段：空闲 (Idle State)

#### 2.1 空闲状态初始化
```csharp
protected override void OnEnterState()
{
    // 重置空闲权重
    npc.ResetIdleWeight();
    // 开始随机移动
    npc.StartRandomMovement();
}
```

#### 2.2 权重累积系统
```csharp
public override void UpdateState()
{
    // 每帧增加空闲权重
    npc.IncreaseIdleWeight();
    
    // 当权重达到1.0时，决定下一个状态
    if (npc.CurrentIdleWeight >= 1f) {
        DetermineNextState();
    }
}
```

#### 2.3 状态决策逻辑
```csharp
private void DetermineNextState()
{
    // 权重计算
    float restTimeWeight = npc.IsRestTime() ? 0.9f : 0f;      // 休息时间权重
    float pendingWorkWeight = npc.PendingWorkTarget.HasValue ? 0.7f : 0f; // 待处理工作权重
    float socialWeight = 0.3f;    // 社交权重
    float newWorkWeight = 0.4f;   // 新工作权重
    
    // 根据权重随机选择下一个状态
    float totalWeight = restTimeWeight + pendingWorkWeight + socialWeight + newWorkWeight;
    float randomValue = Random.Range(0f, totalWeight);
    
    if (randomValue < restTimeWeight) {
        // 回家休息
        npc.currentTarget.position = npc.housing.transform.position;
        stateMachine.ChangeState(NPCState.MovingHome);
    }
    else if (randomValue < restTimeWeight + pendingWorkWeight) {
        // 执行待处理的工作
        npc.currentTarget.position = npc.PendingWorkTarget.Value;
        stateMachine.ChangeState(NPCState.MovingToWork);
    }
    else if (randomValue < restTimeWeight + pendingWorkWeight + socialWeight) {
        // 社交活动
        stateMachine.ChangeState(NPCState.PrepareForSocial);
    }
    else {
        // 寻找新工作
        stateMachine.ChangeState(NPCState.MovingToWork);
    }
}
```

### 第三阶段：工作流程

#### 3.1 前往工作 (MovingToWork State)
```csharp
protected override void OnEnterState()
{
    // 如果没有待处理的工作，寻找新的工作
    if (!npc.PendingWorkTarget.HasValue) {
        var targetBuilding = BuildingManager.Instance.GetBestWorkBuildingForNPC(npc);
        if (targetBuilding != null) {
            npc.currentTarget.position = targetBuilding.transform.position;
            npc.SetPendingWork(targetBuilding.transform.position, targetBuilding);
        } else {
            // 没有找到工作，返回空闲状态
            stateMachine.ChangeState(NPCState.Idle);
            return;
        }
    }
}

public override void UpdateState()
{
    // 检查是否到达工作地点
    if (Vector3.Distance(npc.transform.position, npc.currentTarget.position) < 0.1f) {
        // 清除待处理标记
        if (npc.PendingWorkTarget.HasValue) {
            npc.ClearPendingWork();
        }
        // 进入工作状态
        stateMachine.ChangeState(NPCState.Working);
    }
}
```

#### 3.2 工作中 (Working State)
```csharp
protected override void OnEnterState()
{
    // 开始工作逻辑
    // 这里会调用建筑的工作方法
}

// 工作完成后自动切换到运输状态
nextState = NPCState.Transporting;
```

#### 3.3 运输 (Transporting State)
- 搬运资源或物品
- 在建筑之间移动资源

#### 3.4 工作完成 (WorkComplete State)
- 工作完成后的处理
- 通常返回空闲状态

### 第四阶段：回家休息

#### 4.1 回家 (MovingHome State)
```csharp
// 移动到住所位置
npc.currentTarget.position = npc.housing.transform.position;
```

#### 4.2 到家 (ArrivedHome State)
- 到达住所的处理

#### 4.3 睡眠 (Sleeping State)
- 在住所休息
- 恢复体力和状态

### 第五阶段：社交系统

#### 5.1 准备社交 (PrepareForSocial State)
- 寻找社交对象
- 准备社交活动

#### 5.2 前往社交 (MovingToSocial State)
- 移动到社交位置

#### 5.3 社交中 (Social State)
- 进行社交互动
- 影响关系值

#### 5.4 社交结束
- **SocialEndHappy**: 社交成功，关系提升
- **SocialEndFight**: 社交失败，关系下降

## 状态转换图

```
Generated → Idle → [决策系统]
    ↓
Idle → MovingToWork → Working → Transporting → WorkComplete → Idle
  ↓
Idle → PrepareForSocial → MovingToSocial → Social → SocialEndHappy/Fight → Idle
  ↓
Idle → MovingHome → ArrivedHome → Sleeping → Idle
```

## 关键机制

### 1. 权重系统
- **空闲权重**: 控制NPC在空闲状态停留的时间
- **状态权重**: 影响下一个状态的选择概率
- **时间权重**: 根据游戏时间调整行为倾向

### 2. 住房系统
- NPC必须找到住房才能生存
- 住房满员时，新NPC会被销毁
- 住房作为NPC的归属地

### 3. 工作分配
- BuildingManager负责分配工作
- 支持待处理工作队列
- 工作完成后自动寻找新工作

### 4. 社交系统
- 基于关系值的社交互动
- 社交结果影响NPC关系
- 支持多种社交结局

### 5. 时间系统
- 工作时间: 影响工作效率
- 休息时间: 影响回家倾向
- 季节变化: 可能影响行为模式

## 实际运行示例

### 场景：新NPC进入游戏世界

1. **生成阶段** (0-5秒)
   - NPC被创建
   - 寻找可用住房
   - 成功注册到住房或销毁

2. **空闲阶段** (5-30秒)
   - 在住房附近随机移动
   - 累积空闲权重
   - 观察周围环境

3. **决策阶段** (30秒后)
   - 根据权重系统选择行为
   - 可能选择工作、社交或回家

4. **行为执行** (30秒-几分钟)
   - 执行选定的行为
   - 与其他NPC或建筑交互
   - 完成任务后返回空闲状态

5. **循环往复**
   - 继续权重累积和决策
   - 形成动态的生活模式

## 优化建议

### 1. 性能优化
- 使用对象池管理NPC
- 优化状态检查频率
- 减少不必要的计算

### 2. 行为丰富化
- 增加更多状态类型
- 实现更复杂的决策逻辑
- 添加个性化和随机性

### 3. 社交系统增强
- 实现更复杂的社交网络
- 添加社交事件和故事线
- 支持群体社交活动

### 4. 工作系统完善
- 实现技能系统
- 添加工作等级和晋升
- 支持专业化和分工

这个状态机系统为NPC提供了丰富而真实的行为模式，使游戏世界更加生动和有趣。 