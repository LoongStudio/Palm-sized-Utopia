using UnityEngine;
using System.Collections;


public enum ResourceType
{
	Seed,      // 种子
	Crop,      // 作物
	Feed,      // 饲料
	Livestock, // 种畜
	Animal,    // 牲畜
	Coin,      // 金币
	Voucher    // 奖励券
}


public class ResourceManager: SingletonManager<ResourceManager>
{
	protected override void Awake()
	{
		base.Awake();
		
	}
}
