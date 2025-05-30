using System;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;

public class EditorManager : MonoBehaviour
{
    public static EditorManager Instance { get; private set; }
    private InputSystem_Actions _inputSystem;
    [Header("生成设置")]
    public GameObject prefab;             // 要生成的预制体
    public Vector3 startPosition = Vector3.zero;
    public bool inEditMode = false;
    
    [Header("范围扩展（以起点为中心）")]
    public int extendX = 30;
    public int extendY = 0;
    public int extendZ = 30;
    public List<GameObject> prefabs;
    [Header("生成间隔")]
    public float spacing = 1.0f;

    
    [ContextMenu("生成 Grid")]
    public List<GameObject> GenerateGrid(Vector3 startPosition, float spacing, int extendX, int extendY, int extendZ)
    {
        List<GameObject> prefabs = new List<GameObject>();
        if (prefab == null)
        {
            Debug.LogError("请在 Inspector 中指定 prefab！");
            return prefabs;
        }

        for (int x = -extendX; x <= extendX; x++)
        {
            for (int y = -extendY; y <= extendY; y++)
            {
                for (int z = -extendZ; z <= extendZ; z++)
                {
                    Vector3 offset = new Vector3(x * spacing, y * spacing, z * spacing);
                    Vector3 spawnPos = startPosition + offset;
                    GameObject gameObject = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                    prefabs.Add(gameObject);
                }
            }
        }

        Debug.Log("Grid 生成完成！");
        return prefabs;
    }

    public void toggleEditMode()
    {
        Debug.Log("切换编辑模式");
        inEditMode = !inEditMode;
        if (inEditMode)
        {
            // 如果生成物不存在 则生成
            if (prefabs.Count > 0)
            {
                for (int i = 0; i < prefabs.Count; i++)
                {
                    prefabs[i].SetActive(true);
                }
            }
            else
            {
                prefabs = GenerateGrid(startPosition, spacing, extendX, extendY, extendZ);
            }
            // 清空窗口
            ClickManager.Instance.CloseAllWindows();
        }
        else
        {
            // 隐藏生成物
            for (int i = 0; i < prefabs.Count; i++)
            {
                prefabs[i].SetActive(false);
            }
        }
        // return inEditMode;
    }
    private void CallGenerateGrid(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        toggleEditMode();
    }
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _inputSystem = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _inputSystem.Enable();
        _inputSystem.UI.Edit.performed += CallGenerateGrid;
    }

    private void OnDisable()
    {
        _inputSystem.UI.Edit.performed -= CallGenerateGrid;
        _inputSystem.Disable();
    }
}
