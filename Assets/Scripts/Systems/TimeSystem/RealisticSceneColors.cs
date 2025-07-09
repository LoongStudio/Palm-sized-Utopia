

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MaterialConfig", menuName = "Utopia/MaterialConfig")]
public class MaterialConfig : ScriptableObject
{
	[Serializable]
	public class RealisticSceneMaterial
	{
		public string name;
		public Material material;
	}
	public List<RealisticSceneMaterial> materials = new List<RealisticSceneMaterial>();
	public Material GetMaterial(string name)
	{
		foreach (var sceneMaterial in materials)
			if (sceneMaterial.name == name) return sceneMaterial.material;
		return null;
	}
}
public class RealisticSceneColors : SingletonManager<RealisticSceneColors>
{
	public MaterialConfig materialConfig;
	public GameTime lastUpdateGameTime;
	public float updateRate = 5f;
	public float currentTime = 0f;
	
	public void Start()
	{
		UpdateMaterials();
	}
	public void FixedUpdate()
	{
		currentTime += Time.fixedDeltaTime;
		if (currentTime >= updateRate)
		{
			currentTime = 0f;
			UpdateMaterials();
		}
	}

	public void UpdateMaterials()
	{
		if (TimeManager.Instance.CurrentTime.Equals(lastUpdateGameTime)) return; 
		lastUpdateGameTime = TimeManager.Instance.CurrentTime;
		float interval = (lastUpdateGameTime.month * 30 + lastUpdateGameTime.day) / 360.0f;
		foreach (var realisticSceneMaterial in materialConfig.materials)
			realisticSceneMaterial.material.SetFloat("_SeasonFloat", interval);
	}
}
