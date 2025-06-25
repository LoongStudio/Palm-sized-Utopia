using JetBrains.Annotations;

[System.Serializable]
public class PendingTaskInfo
{
	public Building building;
	public TaskType taskType;
	public PendingTaskInfo(Building building, TaskType taskType)
	{
		this.building = building;
		this.taskType = taskType;
	}

	public static PendingTaskInfo GetNone()
	{
		return (new PendingTaskInfo(null, TaskType.None));
	}
}

public enum TaskType
{
	None,
	Production,
	Handling,
	Rest,
}
