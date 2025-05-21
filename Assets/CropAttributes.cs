using System;
using UnityEngine;
using System.Collections.Generic;
using DataType;

public class CropAttributes : MonoBehaviour
{
	public float interval = 1f;

	private float timer = 0f;

	// 保存所有正在采集的玩家
	private readonly HashSet<BehaviorController> farmersInRange = new HashSet<BehaviorController>();

	void Update()
	{
		if (farmersInRange.Count == 0)
			return;

		timer += Time.deltaTime;
		if (timer >= interval)
		{
			timer = 0f;

			foreach (var farmer in farmersInRange)
			{
				if (farmer != null 
				    && farmer.currentNPCState == NPCStates.Harvesting 
				    && !farmer.IsItemFull(Items.Crops))
				{
					Debug.Log(farmer.name + " 增加Crops " + farmer.NPCbackpack[Items.Crops]);
					farmer.AddItem(Items.Crops, 1);
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("Player"))
		{
			
			BehaviorController farmerInventory = other.transform.GetComponent<BehaviorController>();
			if (farmerInventory != null && farmerInventory.currentNPCState == NPCStates.Walking)
			{
				Debug.Log("农夫 " + other.transform.name + " 开始采摘");
				farmerInventory.currentNPCState = NPCStates.Harvesting;
				farmersInRange.Add(farmerInventory);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.transform.CompareTag("Player"))
		{
			
			BehaviorController farmerInventory = other.transform.GetComponent<BehaviorController>();
			if (farmerInventory != null && farmerInventory.currentNPCState == NPCStates.Walking)
			{
				Debug.Log("农夫 " + other.transform.name + " 开始采摘");
				farmerInventory.currentNPCState = NPCStates.Harvesting;
				farmersInRange.Add(farmerInventory);
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		if (other.transform.CompareTag("Player"))
		{
			BehaviorController farmerInventory = other.transform.GetComponent<BehaviorController>();
			if (farmerInventory != null)
			{
				farmersInRange.Remove(farmerInventory);
			}
		}
	}
}
