using UnityEngine;
using System;
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
	
	// 返回平方根而非倒数
	public static float FastSqrt(float x)
	{
		float xhalf = 0.5f * x;
		int i = BitConverter.SingleToInt32Bits(x);
		i = 0x5f3759df - (i >> 1);
		float y = BitConverter.Int32BitsToSingle(i);
		y = y * (1.5f - xhalf * y * y); // 一次牛顿迭代提高精度
		return x * y; // 转换回平方根
	}
}
