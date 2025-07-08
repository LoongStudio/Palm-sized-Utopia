using Sirenix.OdinInspector;
using UnityEngine;

namespace Grass.Script
{
	public class GrassCollider : MonoBehaviour
	{
		[Min(0)]
		public float radius = 1f;
        
		[Tooltip("碰撞区域的中心偏移量")]
		public Vector3 centerOffset = Vector3.zero;
        
		[ShowInInspector, ReadOnly]
		public Vector3 Position => transform.position + centerOffset;


		private void OnDrawGizmos()
		{
			// 仅在场景视图中绘制调试信息
			if (!Application.isPlaying)
			{
				DrawDebugGizmos();
				return;
			}
			Gizmos.color = new Color(0.92f, 0.39f, 1f, 1f); // 默认颜色 (ED65FF)
			DrawDebugGizmos();
		}

		private void DrawDebugGizmos()
		{
			// 绘制碰撞区域球体
			Gizmos.DrawWireSphere(Position, radius);
            
			// 绘制从物体中心到碰撞区域中心的连接线
			Gizmos.color = Color.gray;
			Gizmos.DrawLine(transform.position, Position);
		}
		
	}
}
