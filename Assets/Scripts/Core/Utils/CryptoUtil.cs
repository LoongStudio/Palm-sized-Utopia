using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class CryptoUtil
{
	// 可以替换为自己定义的 Key / IV
	// key 原始值（16字节）: 例子用十六进制 "01 23 45 67 89 AB CD EF FE DC BA 98 76 54 32 10"
	// 被混淆成多个片段
	private static readonly byte[] k1 = { 0x76, 0x54, 0x32, 0x10 };
	private static readonly byte[] k2 = { 0x89, 0xAB, 0xCD, 0xEF };
	private static readonly byte[] k3 = { 0x01, 0x23, 0x45, 0x67 };
	private static readonly byte[] k4 = { 0xFE, 0xDC, 0xBA, 0x98 };

	// iv 原始值（16字节）: 示例为 ASCII 编码 + 混合方式
	private static readonly string ivStr1 = "H2O9";
	private static readonly string ivStr2 = "ZxcV";
	private static readonly byte[] ivExtra = { 0x5A, 0xA5, 0xC3, 0x3C };
	private static readonly byte[] ivExtra2 = { 0x9F, 0x1E, 0x2D, 0x3C }; // 补足

	private static byte[] key = null;
	private static byte[] iv = null;

	static CryptoUtil()
	{
		key = GetKeyFromCode();
		iv = GetIVFromCode();
	}

	public static byte[] GetKey() => key;
	public static byte[] GetIV() => iv;

	private static byte[] GetKeyFromCode()
	{
		// 重新组合原始 key，顺序错位，防静态分析
		var part1 = k3;         // 前段
		var part2 = k2.Reverse(); // 中段反转
		var part3 = k4;
		var part4 = k1;

		return part1.Concat(part2).Concat(part3).Concat(part4).ToArray();
	}

	private static byte[] GetIVFromCode()
	{
		byte[] iv1 = Encoding.ASCII.GetBytes(ivStr1); // "H2O9"
		byte[] iv2 = Encoding.ASCII.GetBytes(ivStr2); // "ZxcV"
		return iv1.Concat(ivExtra).Concat(iv2).Concat(ivExtra2).ToArray();
	}
	
	public static byte[] EncryptBytes(byte[] data)
	{
		using Aes aesAlg = Aes.Create();
		aesAlg.Key = GetKeyFromCode();
		aesAlg.IV = GetIVFromCode();

		ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
		using var ms = new MemoryStream();
		using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
		cs.Write(data, 0, data.Length);
		cs.FlushFinalBlock();
		return ms.ToArray();
	}

	public static byte[] DecryptBytes(byte[] encrypted)
	{
		using Aes aesAlg = Aes.Create();
		aesAlg.Key = GetKeyFromCode();
		aesAlg.IV = GetIVFromCode();

		ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
		using var ms = new MemoryStream(encrypted);
		using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
		using var result = new MemoryStream();
		cs.CopyTo(result);
		return result.ToArray();
	}
}
