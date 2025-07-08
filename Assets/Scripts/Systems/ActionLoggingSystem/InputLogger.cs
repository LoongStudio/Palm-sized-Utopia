using UnityEngine;

public class InputLogger : MonoBehaviour
{
    void Update()
    {
        // 统计所有按键
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                GameInstanceStats.Instance?.AddKey(key.ToString());
            }
        }

        // 统计鼠标移动
        Vector2 mousePos = Input.mousePosition;
        GameInstanceStats.Instance?.AddMouseMove(mousePos);
    }
} 