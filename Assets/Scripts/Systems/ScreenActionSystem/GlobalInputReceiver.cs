using UnityEngine;
using GlobalInputPlugin;  // 你的DLL命名空间

public class GlobalInputReceiver : MonoBehaviour
{
	void OnEnable()
	{
		// 订阅输入事件
		GlobalInputListener.OnInputEvent += OnInputEvent;
		GlobalInputListener.Init();
	}

	void OnDisable()
	{
		GlobalInputListener.OnInputEvent -= OnInputEvent;
		GlobalInputListener.Stop();
	}

	private void OnInputEvent(InputEventData e)
	{
		switch (e.EventType)
		{
			case InputEventData.InputType.KeyDown:
				Debug.Log($"[KeyDown] {e.KeyCode} at {e.Timestamp}");
				break;

			case InputEventData.InputType.KeyUp:
				Debug.Log($"[KeyUp] {e.KeyCode} at {e.Timestamp}");
				break;

			case InputEventData.InputType.MouseMove:
				Debug.Log($"[MouseMove] X:{e.MouseX} Y:{e.MouseY} at {e.Timestamp}");
				break;

			case InputEventData.InputType.MouseClick:
				Debug.Log($"[MouseClick] Btn:{e.MouseButton} Pos:({e.MouseX},{e.MouseY}) at {e.Timestamp}");
				break;

			case InputEventData.InputType.MouseWheel:
				Debug.Log($"[MouseWheel] Delta:{e.WheelDelta} Pos:({e.MouseX},{e.MouseY}) at {e.Timestamp}");
				break;
		}
	}
}
