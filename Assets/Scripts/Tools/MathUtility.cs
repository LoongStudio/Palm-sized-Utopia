using UnityEngine;

public static class MathUtility {
	// 检查浮点数是否有效（非NaN且非无穷大）
	public static bool IsValid(float value) {
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}

	// 检查Vector3是否每个分量都有效
	public static bool IsValid(Vector3 vector) {
		return IsValid(vector.x) && IsValid(vector.y) && IsValid(vector.z);
	}

	// 检查Quaternion是否有效（避免非法旋转）
	public static bool IsValid(Quaternion quaternion) {
		return IsValid(quaternion.x) && IsValid(quaternion.y) && 
		       IsValid(quaternion.z) && IsValid(quaternion.w);
	}
}
