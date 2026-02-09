using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth; 

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

    [Header("ROAMING")]
    public bool isRoaming = false;
    public float wanderRadius = 15f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("DISTANCE INTERACTION")]
    public float detectionRange = 4.0f; 
    private Transform playerTransform; 
    private bool playerIsNear = false;
    private bool isExiting = false; 
    private Vector3 moveDirection;     

    // Firebase Helper Variables
    private string cleanNpcName; 
    private string hardcodedUserID = "GuxSIr39D3Y1Sp59VMYAW4LTVnW2"; // Fallback ID

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        npcAnimator = GetComponentInChildren<Animator>();
        
        if (npcAnimator != null) npcModel = npcAnimator.transform;

        cleanNpcName = gameObject.name
            .Replace("_prefab(Clone)", "")
            .Replace("(Clone)", "")
            .Replace("_prefab", "") 
            .Trim();

        Transform canvasTrans = transform.Find("DialogueCanvas");
        if (canvasTrans != null)
        {
            dialogueCanvas = canvasTrans.gameObject;
            dialogueCanvas.SetActive(false); 
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainGameScene") 
        {
            isRoaming = false; 
            Transform triggerTrans = transform.Find("DetectionTrigger");
            if (triggerTrans != null) Destroy(triggerTrans.gameObject);
        }
        else 
        {
            isRoaming = true; 
        }

        if (agent != null) agent.updateRotation = false; 
    }

    private void Start()
    {
        FindPlayer();
        if (isRoaming) StartCoroutine(WanderRoutine());
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (npcAnimator == null || agent == null) return;

        // 1. CHECK DISTANCE
        HandleDistanceDetection();

        // 2. FACE PLAYER OVERRIDE
        // This runs every frame while player is near, ensuring they look at you
        if (playerIsNear && playerTransform != null)
        {
            RotateTowards(playerTransform.position);
        }

        // 3. EXIT LOGIC
        if (isExiting && currentTarget != null)
        {
            float distToExit = Vector3.Distance(transform.position, currentTarget.position);
            if (distToExit < 1.5f) { Destroy(gameObject); return; }
        }

        // 4. MOVEMENT & ANIMATION STATES
        if (playerIsNear)
        {
            // MOVED: Play("Greetings") is now in HandleDistanceDetection
            // We only handle model offset here to allow the Root Rotation to work
            RotateModelWithOffset();
            return; 
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (isExiting) 
            {
                npcAnimator.Play("Walking");
                return; 
            }

            npcAnimator.Play("Idle");
            if (dialogueCanvas != null) dialogueCanvas.SetActive(true);
            if (!isRoaming) RotateRootToTarget();
            
            RotateModelWithOffset();
        }
        else 
        {
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

    private void HandleDistanceDetection()
    {
        if (playerTransform == null) FindPlayer();
        if (!isRoaming || isExiting || playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRange)
        {
            if (!playerIsNear)
            {
                playerIsNear = true;
                
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                // PLAY ANIMATION ONCE (Crucial Fix)
                npcAnimator.Play("Greetings");

                // UPDATE FIREBASE
                UpdateFirebaseStatus(true);
            }
        }
        else if (playerIsNear && distance > detectionRange)
        {
            playerIsNear = false;
            
            if (agent != null) 
            {
                agent.isStopped = false;
            }
        }
    }

    private void UpdateFirebaseStatus(bool status)
    {
        string userId = hardcodedUserID;
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("users").Child(userId).Child("npcs").Child(cleanNpcName).SetValueAsync(status);
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0; 
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void SetTarget(Transform destination) 
    {
        if (destination != null && agent != null)
        {
            agent.SetDestination(destination.position);
            currentTarget = destination; 
        }
    }

    public void TriggerExit(Transform exitPoint, float delaySeconds)
    {
        StopAllCoroutines();
        StartCoroutine(ExitRoutine(exitPoint, delaySeconds));
    }

    IEnumerator WanderRoutine()
    {
        while (!isExiting)
        {
            Vector3 newDest = GetRandomNavMeshPoint(transform.position, wanderRadius);
            agent.SetDestination(newDest);
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) { yield return null; }
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);
        }
    }

    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) return hit.position;
        return center;
    }

    IEnumerator ExitRoutine(Transform exitPoint, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetTarget(exitPoint);
        isExiting = true; 
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
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