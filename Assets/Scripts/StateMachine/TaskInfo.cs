using JetBrains.Annotations;

[System.Serializable]
public class TaskInfo
{
	public Building building;
	public TaskType taskType;
	public TaskInfo(Building building, TaskType taskType)
	{
		this.building = building;
		this.taskType = taskType;
	}

	public static TaskInfo GetNone()
	{
		return (new TaskInfo(null, TaskType.None));
	}

	public bool IsNone()
	{
		return taskType == TaskType.None;
	}
	public bool Equals(TaskInfo other)
	{
		if (building == other.building 
		    && taskType == other.taskType) return true;
		return false;
	}
}

public enum TaskType
{
	None,
	Production,
	HandlingAccept,
	HandlingDrop,
	Rest,
}
