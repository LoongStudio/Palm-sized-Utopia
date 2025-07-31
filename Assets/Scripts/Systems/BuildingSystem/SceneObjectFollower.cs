using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FollowerData
{
    public Transform target;
    public Vector3 localPosition;
    public Quaternion localRotation;
    
    public FollowerData(Transform target, Vector3 localPosition, Quaternion localRotation)
    {
        this.target = target;
        this.localPosition = localPosition;
        this.localRotation = localRotation;
    }
}

public class SceneObjectFollower : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] public Transform root;
    [SerializeField] private List<FollowerData> followers = new List<FollowerData>();
    
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = false;
    
    private void Start()
    {
        // 初始化时更新所有跟随物体的位置
        UpdateAllFollowers();
    }
    
    private void LateUpdate()
    {
        // 在LateUpdate中更新跟随物体位置，确保在所有移动之后执行
        // UpdateAllFollowers();
    }
    
    /// <summary>
    /// 注册一个物体作为跟随者
    /// </summary>
    /// <param name="target">要跟随的物体</param>
    public void RegisterFollower(Transform target)
    {
        if (root == null)
        {
            Debug.LogError("[SceneObjectFollower] Root未设置，无法注册跟随者");
            return;
        }
        
        // 将目标物体设置为root的子物体
        target.SetParent(root);
        
        // 计算相对位置和旋转
        Vector3 localPosition = target.localPosition;
        Quaternion localRotation = target.localRotation;
        
        // 创建跟随数据并添加到列表
        FollowerData followerData = new FollowerData(target, localPosition, localRotation);
        followers.Add(followerData);
        
        if (showDebugInfo)
        {
            Debug.Log($"[SceneObjectFollower] 已注册跟随者: {target.name}");
        }
    }
    
    /// <summary>
    /// 注销一个跟随者
    /// </summary>
    /// <param name="target">要注销的物体</param>
    public void UnregisterFollower(Transform target)
    {
        FollowerData followerToRemove = followers.Find(f => f.target == target);
        if (followerToRemove != null)
        {
            followers.Remove(followerToRemove);
            
            if (showDebugInfo)
            {
                Debug.Log($"[SceneObjectFollower] 已注销跟随者: {target.name}");
            }
        }
    }
    
    /// <summary>
    /// 更新所有跟随物体的位置
    /// </summary>
    private void UpdateAllFollowers()
    {
        if (root == null) return;
        
        foreach (FollowerData follower in followers)
        {
            if (follower.target != null)
            {
                // 根据保存的相对位置和旋转更新物体位置
                follower.target.localPosition = follower.localPosition;
                follower.target.localRotation = follower.localRotation;
            }
        }
    }
    
    /// <summary>
    /// 更新特定跟随者的位置
    /// </summary>
    /// <param name="target">要更新的物体</param>
    public void UpdateFollower(Transform target)
    {
        FollowerData follower = followers.Find(f => f.target == target);
        if (follower != null && follower.target != null)
        {
            follower.target.localPosition = follower.localPosition;
            follower.target.localRotation = follower.localRotation;
        }
    }
    
    /// <summary>
    /// 更新所有跟随者的相对位置和旋转数据
    /// </summary>
    public void UpdateFollowerData()
    {
        foreach (FollowerData follower in followers)
        {
            if (follower.target != null)
            {
                follower.localPosition = follower.target.localPosition;
                follower.localRotation = follower.target.localRotation;
            }
        }
    }
    
    /// <summary>
    /// 清除所有跟随者
    /// </summary>
    public void ClearAllFollowers()
    {
        followers.Clear();
    }
    
    /// <summary>
    /// 获取所有跟随者列表
    /// </summary>
    /// <returns>跟随者列表</returns>
    public List<FollowerData> GetAllFollowers()
    {
        return new List<FollowerData>(followers);
    }
}