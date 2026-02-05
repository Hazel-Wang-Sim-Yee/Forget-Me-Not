using UnityEngine;
using UnityEngine.AI;

public class NPCBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;
    float rotationSpeed = 5f;
    Vector3 moveDirection;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform destination) 
    {
        if (destination != null)
        {
            Debug.Log("Setting destination to: " + destination.position);
            agent.SetDestination(destination.position);
        }
    }

    void Update()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 moveDirection = agent.velocity.normalized;
            RotateTowardsMovementDirection();
        }
        
    }

    void RotateTowardsMovementDirection()
    {
        moveDirection.y = 0; // Keep only horizontal rotation
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}