using System;
using System.Collections.Generic;

[Serializable]
public class InputEventSnapshot
{
	public DateTime timestamp;
	public List<string> keyEvents = new List<string>();
	public List<string> mouseEvents = new List<string>();
	public float mouseMoveDistance;

	public InputEventSnapshot()
	{
		timestamp = DateTime.Now;
		mouseMoveDistance = 0f;
	}
}
