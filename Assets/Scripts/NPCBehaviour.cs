using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform destination) 
    {
        if (destination != null)
        {
            agent.SetDestination(destination.position);
        }
    }
}