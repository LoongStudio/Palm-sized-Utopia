using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grass.Script
{
    // [ExecuteInEditMode]
    public class GetPlayerPos : MonoBehaviour
    {
        const int MAX_PLAYERS = 100;
        private List<Vector4> _poss = new List<Vector4>(MAX_PLAYERS);
        public string targetMaterial = "grass";
        public MaterialConfig materialConfig;
        [ShowInInspector] private Material _material; 
        void Start()
        {
            // 从 Scriptable Object 中获取原始材质
            _material = materialConfig.GetMaterial(targetMaterial);
        }
        void Update()
        {
            _poss.Clear();
            // 获取所有激活的 GrassCollider 组件
            GrassCollider[] colliders = FindObjectsByType<GrassCollider>(FindObjectsSortMode.None);
            string output = "";
            foreach (var col in colliders)
            {
                _poss.Add(new Vector4(col.Position.x, col.Position.y, col.Position.z, col.radius));
                output += $"[Grass] {_poss[_poss.Count - 1]}\n";
            }
            while (_poss.Count < MAX_PLAYERS)
            {
                _poss.Add(Vector4.zero);
            }
            // Debug.Log(output);
            if (_poss != null && _poss.Count > 0)
            {
                
                // _material.SetVectorArray("_Players", _poss.ToArray());
                // _material.SetPass(0);
                var possArray = _poss.ToArray();
                foreach (var meshRenderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                {
                    if (meshRenderer.CompareTag("Grass"))
                    {
                        // Debug.Log(meshRenderer.name);
                        meshRenderer.material.SetVectorArray("_Players",  possArray);
                        meshRenderer.material.SetPass(0);
                        // break;
                    }
                }
            }
            
            
        }
    }
}