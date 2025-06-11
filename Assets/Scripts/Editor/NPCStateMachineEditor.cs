using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCStateMachine))]
public class NPCStateMachineEditor : Editor
{
    private GUIStyle stateStyle;
    private GUIStyle labelStyle;

    private void InitializeStyles()
    {
        if (stateStyle == null)
        {
            stateStyle = new GUIStyle(EditorStyles.boldLabel);
            stateStyle.fontSize = 14;
            stateStyle.alignment = TextAnchor.MiddleCenter;
            stateStyle.normal.textColor = Color.white;
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontSize = 12;
            labelStyle.alignment = TextAnchor.MiddleLeft;
        }
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();

        NPCStateMachine stateMachine = (NPCStateMachine)target;
        
        // 绘制默认的Inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // 绘制状态信息
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // 当前状态
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("当前状态:", labelStyle, GUILayout.Width(80));
        EditorGUILayout.LabelField(stateMachine.CurrentState.ToString(), stateStyle);
        EditorGUILayout.EndHorizontal();

        // 上一个状态
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("上一个状态:", labelStyle, GUILayout.Width(80));
        EditorGUILayout.LabelField(stateMachine.PreviousState.ToString(), labelStyle);
        EditorGUILayout.EndHorizontal();

        // 状态持续时间
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("状态持续时间:", labelStyle, GUILayout.Width(80));
        EditorGUILayout.LabelField($"{stateMachine.GetStateTimer():F2}秒", labelStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        // 强制重绘Inspector以更新状态显示
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
} 