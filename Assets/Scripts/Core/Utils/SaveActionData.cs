using System;
using System.Collections.Generic;

[Serializable]
public class SaveActionData
{
	public List<InputEventSnapshot> history = new List<InputEventSnapshot>();

	public SaveActionData(List<InputEventSnapshot> inputHistory)
	{
		history = inputHistory;
	}
}
