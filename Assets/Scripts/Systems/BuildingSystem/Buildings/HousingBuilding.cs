using System.Collections.Generic;
using UnityEngine;

public abstract class HousingBuilding : Building
{
	public List<NPC> livingNPCs = new List<NPC>();
	public int maxLivingNPCs;

	public bool RegisterLivingNPC(NPC npc)
	{
		Debug.Log($"[住房] 注册住房NPC {npc.data.npcName}");
		if (livingNPCs.Contains(npc) || livingNPCs.Count >= maxLivingNPCs) return false;
		livingNPCs.Add(npc);
		npc.housing = this;
		return true;
	}

	public bool UnRegisterLivingNPC(NPC npc)
	{
		Debug.Log($"[住房] 取消注册住房NPC {npc.name}");
		if (!livingNPCs.Contains(npc)) return false;
		livingNPCs.Remove(npc);
		npc.housing = null;
		return true;
	}

	public bool MovePlaceTo(HousingBuilding building, NPC target)
	{
		if (UnRegisterLivingNPC(target))
		{
			if (building.RegisterLivingNPC(target))
			{
				return true;
			}
			else
			{
				RegisterLivingNPC(target);
				return false;
			}
		}
		return false;
	}
}
