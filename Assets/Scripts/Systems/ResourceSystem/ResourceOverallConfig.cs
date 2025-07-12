using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceOverallConfig", menuName = "Utopia/ResourceOverallConfig")]
public class ResourceOverallConfig : ScriptableObject
{
    [InfoBox("此处分配应有资源种类、初始资源数量、上限、买卖价格")]
    [LabelText("资源配置")] public List<ResourceStack> resourceStacks;
}