using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourceConfig))]
public class ResourceDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取目标对象
        var data = (ResourceConfig)target;

        // ResourceType 下拉菜单
        data.type = (ResourceType)EditorGUILayout.EnumPopup("Resource Type", data.type);

        // 根据类型获取子类型枚举
        var subTypeEnum = ResourceSubTypeHelper.GetSubTypeEnum(data.type);
        var enumNames = System.Enum.GetNames(subTypeEnum);
        var enumValues = System.Enum.GetValues(subTypeEnum);

        // 获取当前 subType 对应的索引
        int selectedIndex = 0;
        for (int i = 0; i < enumValues.Length; i++)
        {
            if ((int)enumValues.GetValue(i) == data.subType)
            {
                selectedIndex = i;
                break;
            }
        }

        // 子类型下拉菜单
        selectedIndex = EditorGUILayout.Popup("Sub Type", selectedIndex, enumNames);
        data.subType = (int)enumValues.GetValue(selectedIndex);

        // 其余字段使用默认绘制
        data.displayName = EditorGUILayout.TextField("Display Name", data.displayName);
        data.description = EditorGUILayout.TextField("Description", data.description);
        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);
        data.canBeSold = EditorGUILayout.Toggle("Can Be Sold", data.canBeSold);
        data.canBePurchased = EditorGUILayout.Toggle("Can Be Purchased", data.canBePurchased);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}
