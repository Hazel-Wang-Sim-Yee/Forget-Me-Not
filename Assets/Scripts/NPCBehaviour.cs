using UnityEngine;
using UnityEngine.AI;
using System.Collections; 

public class NPCBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator npcAnimator;
    private Transform currentTarget; 
    private GameObject dialogueCanvas;
    private Transform npcModel; 

    [Header("SETTINGS")]
    public float rotationSpeed = 10f; 
    [Range(-180, 180)]
    public float idleRotationOffset = 0f; 

    private bool isExiting = false; 
    private Vector3 moveDirection;     

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        npcAnimator = GetComponentInChildren<Animator>();
        
        if (npcAnimator != null) npcModel = npcAnimator.transform;

        Transform canvasTrans = transform.Find("DialogueCanvas");
        if (canvasTrans != null)
        {
            dialogueCanvas = canvasTrans.gameObject;
            dialogueCanvas.SetActive(false); 
        }

        // IMPORTANT: Prevent Unity from auto-rotating so we can control it manually
        if (agent != null) agent.updateRotation = false; 
    }

    public void SetTarget(Transform destination) 
    {
        if (destination != null)
        {
            if (agent != null) agent.SetDestination(destination.position);
            currentTarget = destination; 
        }
    }

    public void TriggerExit(Transform exitPoint, float delaySeconds)
    {
        StartCoroutine(ExitRoutine(exitPoint, delaySeconds));
    }

    IEnumerator ExitRoutine(Transform exitPoint, float delay)
    {
        // 1. Wait
        yield return new WaitForSeconds(delay);

        // 2. Set destination to the Exit
        SetTarget(exitPoint);
        isExiting = true; 
        
        // Force the Canvas off immediately
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
    }

    void Update()
    {
        if (npcAnimator == null || agent == null) return;

        // --- SAFETY CHECK: Destroy if close to Exit ---
        if (isExiting && currentTarget != null)
        {
            float distToExit = Vector3.Distance(transform.position, currentTarget.position);
            if (distToExit < 1.5f) 
            {
                Destroy(gameObject);
                return;
            }
        }

        // CHECK: Are we at the destination?
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (isExiting) 
            {
                npcAnimator.Play("Walking");
                return; 
            }

            // --- STATE: IDLE ---
            npcAnimator.Play("Idle"); // Or "ShopIdle" if that is your animation name
            if (dialogueCanvas != null) dialogueCanvas.SetActive(true);
            RotateRootToTarget();
            RotateModelWithOffset();
        }
        else
        {
            // --- STATE: WALKING ---
            npcAnimator.Play("Walking");
            if (dialogueCanvas != null) dialogueCanvas.SetActive(false);

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                moveDirection = agent.velocity.normalized;
                RotateRootToMovement();
            }
            ResetModelRotation();
        }
    }

    void RotateRootToMovement()
    {
        moveDirection.y = 0; 
        if (moveDirection != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void RotateRootToTarget()
    {
        if (currentTarget != null)
        {
            Quaternion finalRotation = currentTarget.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void RotateModelWithOffset()
    {
        if (npcModel != null)
        {
            Quaternion offsetRotation = Quaternion.Euler(0, idleRotationOffset, 0);
            npcModel.localRotation = Quaternion.Slerp(npcModel.localRotation, offsetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ResetModelRotation()
    {
        if (npcModel != null)
        {
            npcModel.localRotation = Quaternion.Slerp(npcModel.localRotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
        }
    }
}