using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

/// NavMesh集成管理器
public class NavMeshIntegration : SingletonManager<NavMeshIntegration>
{
    [SerializeField] private PlacementSettings settings;
    [SerializeField] private NavMeshSurface navMeshSurface;
    
    private Coroutine updateCoroutine;
    private bool needsUpdate = false;
    
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // 自动找到NavMeshSurface
        if (navMeshSurface == null)
        {
            navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
        }
        
        if (navMeshSurface == null)
        {
            Debug.LogWarning("[NavMeshIntegration] NavMeshSurface not found!");
            return;
        }
        
        // 订阅事件
        PlacementEvents.OnObjectPlaced += OnObjectPlaced;
        PlacementEvents.OnObjectRemoved += OnObjectRemoved;
        PlacementEvents.OnObjectMoved += OnObjectMoved;
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        PlacementEvents.OnObjectPlaced -= OnObjectPlaced;
        PlacementEvents.OnObjectRemoved -= OnObjectRemoved;
        PlacementEvents.OnObjectMoved -= OnObjectMoved;
    }
    
    private void OnObjectPlaced(IPlaceable placeable)
    {
        RequestNavMeshUpdate();
    }
    
    private void OnObjectRemoved(IPlaceable placeable)
    {
        RequestNavMeshUpdate();
    }
    
    private void OnObjectMoved(IPlaceable placeable)
    {
        RequestNavMeshUpdate();
    }
    
    private void RequestNavMeshUpdate()
    {
        if (!needsUpdate)
        {
            needsUpdate = true;
            
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }
            
            updateCoroutine = StartCoroutine(DelayedNavMeshUpdate());
        }
    }
    
    private IEnumerator DelayedNavMeshUpdate()
    {
        yield return new WaitForSeconds(settings.NavMeshUpdateDelay);
        
        if (navMeshSurface != null)
        {
            if (settings.EnableAsyncOperations)
            {
                yield return StartCoroutine(AsyncNavMeshUpdate());
            }
            else
            {
                navMeshSurface.BuildNavMesh();
            }
            
            Debug.Log("[NavMeshIntegration] NavMesh updated");
        }
        
        needsUpdate = false;
        updateCoroutine = null;
    }
    
    private IEnumerator AsyncNavMeshUpdate()
    {
        var operation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        
        while (!operation.isDone)
        {
            yield return null;
        }
    }
    
    /// <summary>
    /// 强制立即更新NavMesh
    /// </summary>
    public void ForceUpdateNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("[NavMeshIntegration] NavMesh force updated");
        }
    }
}
