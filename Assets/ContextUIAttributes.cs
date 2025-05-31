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
    private List<Vector2Int> GetBottomAnchorOffsets(GameObject prefab)
    {
        Debug.Log("[UI] Get Bottom Anchor Offsets");
        List<Vector2Int> offsets = new List<Vector2Int>();
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
                child.localPosition.z * scale.y,
                child.localPosition.z * scale.z);
            Vector3 delta = childAnchor - basePosition;
            offsets.Add(new Vector2Int(
                Mathf.RoundToInt(delta.x),
                Mathf.RoundToInt(delta.z)
            ));
            Debug.Log("[UI] offset: " + offsets.Last() + " origin: " + childAnchor);
        }

        return offsets;
    }

    private bool CheckAnchorFitAt(Vector2Int basePos, List<Vector2Int> anchorOffsets)
    {
        foreach (var offset in anchorOffsets)
        {
            Vector2Int pos = basePos + offset;
            if (GridManager.Instance.GetMap().ContainsKey(pos))
            {
                Debug.Log("[UI] Find Contains: " + pos);
                return false;
            }
                
        }
        return true;
    }

    private void InstantiatePrefabAtAnchor(GameObject prefab, Vector2Int basePos, Transform baseAnchor)
    {
        Vector2 anchorOffset = baseAnchor.localPosition;
        Vector2 spawnWorldPos = basePos - anchorOffset;
        // spawnWorldPos.z = belongTo.transform.position.z;
        Debug.Log("[UI] Instantiate Anchor: " + spawnWorldPos);
        GameObject obj = Instantiate(prefab, spawnWorldPos, Quaternion.identity);
        
    }

    public void CallSpawnPrefabNearby(GameObject prefab)
    {
        if (prefab == null) return;

        List<Vector2Int> anchorOffsets = GetBottomAnchorOffsets(prefab);
        if (anchorOffsets.Count == 0) return;

        Transform baseAnchor = prefab.GetComponent<DragAndSnapWithAnchors>().bottomAnchors[0];

        Vector2 baseWorldPos = belongTo.transform.position;
        Vector2Int baseGrid = new Vector2Int(
            Mathf.RoundToInt(baseWorldPos.x),
            Mathf.RoundToInt(baseWorldPos.y)
        );

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(baseGrid);
        visited.Add(baseGrid);

        Vector2Int[] directions = 
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            Debug.Log("[UI] BFS 遍历: " + current);
            if (CheckAnchorFitAt(current, anchorOffsets))
            {
                InstantiatePrefabAtAnchor(prefab, current, baseAnchor);
                return;
            }

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
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
            Vector2 screenPos = Camera.main.WorldToScreenPoint(
                belongTo.transform.position 
                + new Vector3(0.0f, gameObject.transform.localScale.z * 1.5f, 0.0f));
            transform.position = screenPos;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("窗口被点击");
        
    }

}
