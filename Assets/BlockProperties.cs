using System;
using UnityEditor;
using UnityEngine;

public class BlockProperties : MonoBehaviour
{
    [Header("方块属性")]
    public string blockID { get; private set; } = "";
    public string blockName { get; private set; } = "";
    public float pathFindingCost = 1.0f;
    
    private DragAndSnapWithAnchors dragAndSnapWithAnchors;

    private void Awake()
    {
        if (blockID == "") blockID = Guid.NewGuid().ToString();
        if (blockName == "") blockName = gameObject.name;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (dragAndSnapWithAnchors == null)
        {
            if (!gameObject.TryGetComponent<DragAndSnapWithAnchors>(out dragAndSnapWithAnchors))
                Debug.LogError("BlockProperties: DragAndSnapWithAnchors 无法找到");
                
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
