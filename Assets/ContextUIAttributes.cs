using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ContextUIAttributes : MonoBehaviour, IPointerClickHandler
{
    public GameObject belongTo;
    public GameObject shoppingBlockPrefab;
    public GameObject[] buildingList;
    public bool stackToBelong = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<Button> buttons = GetComponentsInChildren<Button>().ToList();
        foreach (var button in buttons)
        {
            if (button.CompareTag("UI_CLOSE"))
            {
                button.onClick.AddListener(() =>
                {
                    ClickManager.Instance.CloseWindow(belongTo);
                });
            }
            if (button.CompareTag("UI_STACK_TO"))
            {
                button.onClick.AddListener(() =>
                {
                    stackToBelong = true;
                });
            }
        }
        Transform content = GetComponentInChildren<ScrollRect>().content;
        foreach (var building in buildingList)
        {
            GameObject buildingInfo = Instantiate(shoppingBlockPrefab, content);
            var children = buildingInfo.GetComponentsInChildren<TMP_Text>();
            foreach (var child in children)
                if (child.name == "BuildingTitle")
                    child.text = building.name;
            buildingInfo.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                Debug.Log("[UI] On Button Clicked");
                CallSpawnPrefabNearby(building);
            });
        }
    }
    // ==========================================
    // 附近生成有效放置算法
    // ==========================================
    private List<Vector3Int> GetBottomAnchorOffsets(GameObject prefab)
    {
        Debug.Log("[UI] Get Bottom Anchor Offsets");
        List<Vector3Int> offsets = new List<Vector3Int>();
        Vector3 scale = prefab.transform.localScale;
        List<Transform> children = prefab.GetComponent<DragAndSnapWithAnchors>().bottomAnchors;
        Vector3 basePosition = children[0].localPosition;
        basePosition = new Vector3(
            basePosition.x * scale.x, 
            basePosition.y * scale.y, 
            basePosition.z * scale.z);
        Debug.Log("[UI] basePosition: " + basePosition);
        // 计算相对偏移
        foreach (var child in children)
        {
            Vector3 childAnchor = new Vector3(
                child.localPosition.x * scale.x, 
                child.localPosition.y * scale.y, 
                child.localPosition.z * scale.z);
            Vector3 delta = childAnchor - basePosition;
            offsets.Add(new Vector3Int(
                Mathf.RoundToInt(delta.x),
                0,
                Mathf.RoundToInt(delta.z)
            ));
            Debug.Log("[UI] offset: " + offsets.Last() + " origin: " + childAnchor);
        }

        return offsets;
    }

    private bool CheckAnchorFitAt(Vector3Int basePos, List<Vector3Int> anchorOffsets)
    {
        foreach (var offset in anchorOffsets)
        {
            Vector3Int pos = basePos + offset;
            if (GridManager.Instance.GetMap().ContainsKey(pos))
            {
                Debug.Log("[UI] Find Contains: " + pos);
                return false;
            }
                
        }
        return true;
    }

    private void InstantiatePrefabAtAnchor(GameObject prefab, Vector3Int basePos, Transform baseAnchor)
    {
        Vector3 anchorOffset = baseAnchor.localPosition;
        Vector3 spawnWorldPos = basePos - anchorOffset;
        // spawnWorldPos.y = belongTo.transform.position.y;
        Debug.Log("[UI] Instantiate Anchor: " + spawnWorldPos);
        GameObject obj = Instantiate(prefab, spawnWorldPos, Quaternion.identity);
        
    }

    public void CallSpawnPrefabNearby(GameObject prefab)
    {
        if (prefab == null) return;

        List<Vector3Int> anchorOffsets = GetBottomAnchorOffsets(prefab);
        if (anchorOffsets.Count == 0) return;

        Transform baseAnchor = prefab.GetComponent<DragAndSnapWithAnchors>().bottomAnchors[0];

        Vector3 baseWorldPos = belongTo.transform.position;
        Vector3Int baseGrid = new Vector3Int(
            Mathf.RoundToInt(baseWorldPos.x),
            0,
            Mathf.RoundToInt(baseWorldPos.z)
        );

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        queue.Enqueue(baseGrid);
        visited.Add(baseGrid);

        Vector3Int[] directions = 
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1)
        };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            Debug.Log("[UI] BFS 遍历: " + current);
            if (CheckAnchorFitAt(current, anchorOffsets))
            {
                InstantiatePrefabAtAnchor(prefab, current, baseAnchor);
                return;
            }

            foreach (var dir in directions)
            {
                Vector3Int next = current + dir;
                if (!visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        Debug.LogWarning("未找到合适位置生成 prefab。");
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) 
            && ClickManager.Instance.GetPointerTouchUI(gameObject))
        {
            transform.SetAsLastSibling();
        }
        if (stackToBelong)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(
                belongTo.transform.position 
                + new Vector3(0.0f, gameObject.transform.localScale.y * 1.5f, 0.0f));
            transform.position = screenPos;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("窗口被点击");
        
    }

}
