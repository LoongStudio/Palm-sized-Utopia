using UnityEngine;

public class GameManager : SingletonManager<GameManager>
{

	protected override void Awake()
	{
		base.Awake();
	}

	public GameManager()
	{
		// GridManager.Instance.SetOccupied(new Vector2Int(0, 0), );
	}
	
	
}

