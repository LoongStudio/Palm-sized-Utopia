using System;
using UnityEngine;
public class CollisionManager : MonoBehaviour
{
	private void Start()
	{
		int npcLayer = LayerMask.NameToLayer("NPC");
		Physics.IgnoreLayerCollision(npcLayer, npcLayer, true); // true表示忽略碰撞
	}
}
