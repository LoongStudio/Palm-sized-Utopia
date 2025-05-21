using UnityEngine;
using UnityEngine.AI;

public class BehaviorController : MonoBehaviour
{
    public Transform navStart;
    public Transform navEnd;
    public NavMeshAgent agent;
    public bool finishedNav = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!finishedNav)
        {
            agent.destination = navEnd.position;
        }
        // 如果调出世界重新再 Block 上生成
        if (transform.position.y < -2.0f)
        {
            GameObject targetObject = FindFirstObjectByType<BlockProperties>().gameObject;
            
            transform.position.Set(
                targetObject.transform.position.x,
                targetObject.transform.position.y + targetObject.transform.localScale.y / 2, 
                targetObject.transform.position.z);
            
        }
    }
}
