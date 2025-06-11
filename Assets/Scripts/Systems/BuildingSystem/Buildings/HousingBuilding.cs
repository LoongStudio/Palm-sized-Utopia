using System.Collections.Generic;

public abstract class HousingBuilding : Building
{
	public List<NPC> livingNPCs = new List<NPC>();
	public int maxLivingNPCs;

	public bool RegisterLivingNPC(NPC npc)
	{
		if (livingNPCs.Contains(npc) || livingNPCs.Count >= maxLivingNPCs) return false;
		livingNPCs.Add(npc);
		return true;
	}

	public bool UnRegisterLivingNPC(NPC npc)
	{
		if (!livingNPCs.Contains(npc)) return false;
		livingNPCs.Remove(npc);
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
