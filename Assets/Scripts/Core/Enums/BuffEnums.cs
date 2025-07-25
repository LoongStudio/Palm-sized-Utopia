public enum BuffEnums
{
	WellIrrigated,		// 水量丰沛
	GrowthHormone,		// 生长激素
	BargainingExpert,	// 讨价还价专家
	FriendWorkTogether,	// 友方工作协同
    NPCWorkingInSlot,	// NPC在槽位工作
    ComposedYard,		// 合成车间
	NPCEfficiency,		// NPC自身效率


}

public struct Buff{
    public BuffEnums type;
    public int intensity;
	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="type">Buff类型</param>
	/// <param name="intensity">Buff强度，范围为0-100</param>
	public Buff(BuffEnums type, int intensity){
		this.type = type;
		this.intensity = intensity;
	}
	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="type">Buff类型</param>
	/// <param name="intensity">Buff强度，范围为0-1</param>
	public Buff(BuffEnums type, float intensity){
		this.type = type;
		this.intensity = (int)(intensity * 100);
	}
}
