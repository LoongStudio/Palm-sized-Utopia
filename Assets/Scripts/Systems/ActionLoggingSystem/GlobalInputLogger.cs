using Gma.System.MouseKeyHook;
using UnityEngine;

public class GlobalInputLogger : MonoBehaviour
{
    private IKeyboardMouseEvents m_GlobalHook;
    private Vector2 lastMousePos;

    void Start()
    {
        m_GlobalHook = Hook.GlobalEvents();
        m_GlobalHook.KeyDown += OnKeyDown;
        m_GlobalHook.MouseMove += OnMouseMove;
    }

    private void OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        GameInstanceStats.Instance?.AddKey(e.KeyCode.ToString());
    }

    private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        Vector2 pos = new Vector2(e.X, e.Y);
        GameInstanceStats.Instance?.AddMouseMove(pos);
    }

    void OnDestroy()
    {
        if (m_GlobalHook != null)
        {
            m_GlobalHook.KeyDown -= OnKeyDown;
            m_GlobalHook.MouseMove -= OnMouseMove;
            m_GlobalHook.Dispose();
        }
    }
} 