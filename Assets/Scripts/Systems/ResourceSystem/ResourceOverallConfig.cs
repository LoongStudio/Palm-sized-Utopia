using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceOverallConfig", menuName = "Utopia/ResourceOverallConfig")]
public class ResourceOverallConfig : ScriptableObject
{
    [InfoBox("此处分配应有资源种类、初始资源数量、上限、买卖价格, 金币和奖励券是特殊资源，单独配置以方便经常调用")]
    [LabelText("资源配置")] public List<ResourceStack> resourceStacks;
    [LabelText("金币")] public ResourceConfig gold;
    [LabelText("奖励券")] public ResourceConfig ticket;
}