using System.Collections.Generic;

public class Apartment : HousingBuilding
{
	public override void OnUpgraded() { }
	public override void OnDestroyed() { }

	public override void InitialSelfStorage()
	{
		inventory = new Inventory(
			new List<SubResourceValue<int>>(),
			new List<SubResourceValue<int>>(),
			Inventory.InventoryAcceptMode.OnlyDefined,
			Inventory.InventoryListFilterMode.None,
			null,
			null,
			Inventory.InventoryOwnerType.Building
		);
	}
}
