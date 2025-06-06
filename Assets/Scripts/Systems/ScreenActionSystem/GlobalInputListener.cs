using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(InputHistoryLogger))]
public class GlobalInputListener : SingletonManager<GlobalInputListener>
{
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_KEYBOARD_LL = 13;
    private const int WH_MOUSE_LL = 14;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_MOUSEMOVE = 0x0200;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MOUSEWHEEL = 0x020A;

    private static HookProc _keyboardProc;
    private static HookProc _mouseProc;

    private static IntPtr _keyboardHookID = IntPtr.Zero;
    private static IntPtr _mouseHookID = IntPtr.Zero;

    private static Vector2Int _lastMousePos;

    private void OnEnable()
    {
        _keyboardProc = KeyboardHookCallback;
        _mouseProc = MouseHookCallback;

        _keyboardHookID = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(null), 0);
        _mouseHookID = SetHook(WH_MOUSE_LL, _mouseProc);
    }

    private void OnDisable()
    {
        UnhookWindowsHookEx(_keyboardHookID);
        UnhookWindowsHookEx(_mouseHookID);
    }

    private static IntPtr SetHook(int hookType, HookProc callback)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(hookType, callback, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            string key = ((KeyCode)vkCode).ToString();
            InputHistoryLogger.Instance?.LogKey(key);
        }
        return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            MSLLHOOKSTRUCT hook = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

            switch (msg)
            {
                case WM_MOUSEMOVE:
                    Vector2Int curPos = new Vector2Int(hook.pt.x, hook.pt.y);
                    float moveDistance = Vector2Int.Distance(curPos, _lastMousePos);
                    InputHistoryLogger.Instance?.LogMouse($"Move {curPos.x},{curPos.y}");
                    InputHistoryLogger.Instance?.AddMouseMoveDistance(moveDistance);
                    _lastMousePos = curPos;
                    break;

                case WM_LBUTTONDOWN:
                    InputHistoryLogger.Instance?.LogMouse("Left Click");
                    break;

                case WM_RBUTTONDOWN:
                    InputHistoryLogger.Instance?.LogMouse("Right Click");
                    break;

                case WM_MOUSEWHEEL:
                    short delta = (short)((hook.mouseData >> 16) & 0xffff);
                    InputHistoryLogger.Instance?.LogMouse($"Scroll Delta: {delta}");
                    break;
            }
        }
        return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x, y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public int mouseData;
        public int flags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
