public class Apartment : HousingBuilding
{
	public override void OnUpgraded() { }
	public override void OnDestroyed() { }

	public override void InitialSelfStorage()
	{
		inventory = new Inventory();
	}
}
