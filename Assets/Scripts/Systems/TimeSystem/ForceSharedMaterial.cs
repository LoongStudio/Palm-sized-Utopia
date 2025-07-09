using UnityEngine;

public class ForceSharedMaterial : MonoBehaviour
{
	public string targetMaterial = "grass";
	public MaterialConfig materialConfig;
	void Start()
	{
		// Debug.Log($"[Scene] {gameObject.name} 更新目标材质: " + targetMaterial);
		// 获取所有渲染器组件（包括MeshRenderer、SkinnedMeshRenderer等）
		Renderer renderer = GetComponent<Renderer>();
		if (renderer == null || materialConfig == null)
			Destroy(this);
		// 从 Scriptable Object 中获取原始材质
		Material originalMaterial = materialConfig.GetMaterial(targetMaterial);
		if (originalMaterial != null)
			renderer.sharedMaterial = originalMaterial;
        
		// 任务完成后销毁自身，避免持续占用资源
		Destroy(this);
	}
}
