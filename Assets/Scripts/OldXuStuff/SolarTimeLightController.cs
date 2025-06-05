using UnityEngine;
using System;

public class SolarTimeLightController : MonoBehaviour
{
	[Header("控制选项")]
	public bool syncWithSystemTime = false;
	public float cycleDuration = 20f; // 20秒一圈（如果不同步）

	[Header("角度范围（不重要，可调）")]
	public float minElevation = 0f;
	public float maxElevation = 90f;

	private Light sun;
	private float timeOffset;

	void Start()
	{
		sun = GetComponent<Light>();
		if (sun == null || sun.type != LightType.Directional)
		{
			Debug.LogWarning("请将此脚本挂载在 Directional Light 上。");
		}

		// 随机时间偏移，使不同实例不同步（非必须）
		timeOffset = UnityEngine.Random.Range(0f, 1000f);
	}

	void Update()
	{
		float elevationAngle = 0f;

		if (syncWithSystemTime)
		{
			// 获取当前系统小时（0~24）
			float hour = DateTime.Now.Hour + DateTime.Now.Minute / 60f;
			// 将 0~24 映射为 -90° 到 270°（一天旋转360度）
			elevationAngle = (hour / 24f) * 360f - 90f;
		}
		else
		{
			// 非同步：按时间匀速旋转
			float t = (Time.time + timeOffset) / cycleDuration;
			elevationAngle = (t % 1f) * 360f;
		}

		// 更新光照角度（仅绕 X 轴旋转，模拟日升日落）
		transform.rotation = Quaternion.Euler(elevationAngle, 170f, 0f);
	}
}
