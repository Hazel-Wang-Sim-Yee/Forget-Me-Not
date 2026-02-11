using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;

public class NPCBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator npcAnimator;
    private Transform currentTarget; 
    private GameObject dialogueCanvas; 
    private GameObject exteriorCanvas; 
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

    private string cleanNpcName; 
    private bool hasInteracted = false;

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

        FindCanvases();

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainGameScene") 
        {
            isRoaming = false; 
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
        
        if (dialogueCanvas == null || exteriorCanvas == null)
        {
            FindCanvases();
        }

        if (isRoaming) StartCoroutine(WanderRoutine());
    }

    private void FindCanvases()
    {
        Canvas[] childCanvases = GetComponentsInChildren<Canvas>(true);
        foreach (Canvas c in childCanvases)
        {
            if (c.gameObject.name == "DialogueCanvas")
            {
                dialogueCanvas = c.gameObject;
                dialogueCanvas.SetActive(false); 
            }
            else if (c.gameObject.name == "ExteriorDialogueCanvas")
            {
                exteriorCanvas = c.gameObject;
                exteriorCanvas.SetActive(false); 
            }
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (npcAnimator == null || agent == null) return;

        HandleDistanceDetection();

        if (isRoaming && playerIsNear && playerTransform != null)
        {
            RotateTowards(playerTransform.position);
            
            if (exteriorCanvas != null && !exteriorCanvas.activeSelf) 
            {
                exteriorCanvas.SetActive(true);
            }
        }
        else if (isRoaming)
        {
            if (exteriorCanvas != null && exteriorCanvas.activeSelf) exteriorCanvas.SetActive(false);
            if (dialogueCanvas != null && dialogueCanvas.activeSelf) dialogueCanvas.SetActive(false);
            hasInteracted = false; 
        }

        if (isExiting && currentTarget != null)
        {
            float distToExit = Vector3.Distance(transform.position, currentTarget.position);
            if (distToExit < 1.5f) { Destroy(gameObject); return; }
        }

        if (isRoaming && playerIsNear)
        {
            if (!hasInteracted) npcAnimator.Play("Idle");
            RotateModelWithOffset();
            return; 
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (isExiting) { npcAnimator.Play("Walking"); return; }
            
            npcAnimator.Play("Idle");
            
            if (!isRoaming) RotateRootToTarget();
            
            RotateModelWithOffset();

            if (!isRoaming && !hasInteracted)
            {
                OnShopArrival(); 
            }
        }
        else 
        {
            npcAnimator.Play("Walking");
            
            if (!hasInteracted && dialogueCanvas != null && dialogueCanvas.activeSelf) 
                dialogueCanvas.SetActive(false);

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                moveDirection = agent.velocity.normalized;
                RotateRootToMovement();
            }
            ResetModelRotation();
        }
    }

    private void OnShopArrival()
    {
        hasInteracted = true;
        npcAnimator.Play("Greetings");

        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
            FetchOrderFromFirebase();
        }
        else
        {
            Debug.LogError("Shop Arrival failed: DialogueCanvas is missing on " + cleanNpcName);
        }
    }

    private void FetchOrderFromFirebase()
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("npc_data").Child("dialogue").Child(cleanNpcName).Child("order").Child("text").GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string text = task.Result.Value.ToString();
                TMP_Text textComp = dialogueCanvas.GetComponentInChildren<TMP_Text>();
                if (textComp != null) textComp.text = text;
            }
            else
            {
                Debug.LogWarning("Could not find order text in Firebase for: " + cleanNpcName);
            }
        });
    }


    public void OnInteractButtonPressed()
    {
        hasInteracted = true;
        npcAnimator.Play("Greetings");

        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
            FetchDialogueFromFirebase("interact_dialogue");
        }
        else
        {
            Debug.LogError("Interaction failed: DialogueCanvas is missing on " + cleanNpcName);
        }

        UpdateFirebaseStatus("npcs", true);
    }

    public void OnRecallButtonPressed()
    {
        hasInteracted = true;
        npcAnimator.Play("Greetings");

        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
            FetchDialogueFromFirebase("recall_dialogue");
        }
        else
        {
            Debug.LogError("Recall failed: DialogueCanvas is missing on " + cleanNpcName);
        }

        UpdateFirebaseStatus("recalled_npcs", true);
        CheckAllRecalledAndUnlockTwist();
    }

    private void CheckAllRecalledAndUnlockTwist()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null) return;
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("users").Child(userId).Child("recalled_npcs").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                long count = 0;
                foreach(var child in task.Result.Children)
                {
                    if (child.Value is bool && (bool)child.Value == true) count++;
                }
                if (count >= 5) dbRef.Child("users").Child(userId).Child("twist_unlocked").SetValueAsync(true);
            }
        });
    }

    private void FetchDialogueFromFirebase(string dialogueType)
    {
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        dbRef.Child("npc_data").Child("dialogue").Child(cleanNpcName).Child(dialogueType).GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                string text = task.Result.Value.ToString();
                TMP_Text textComp = dialogueCanvas.GetComponentInChildren<TMP_Text>();
                if (textComp != null) textComp.text = text;
            }
        });
    }

    private void UpdateFirebaseStatus(string folder, bool status)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            dbRef.Child("users").Child(userId).Child(folder).Child(cleanNpcName).SetValueAsync(status);
        }
    }

    private void HandleDistanceDetection()
    {
        if (!isRoaming || isExiting) return;
        
        if (playerTransform == null) FindPlayer();
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= detectionRange)
        {
            if (!playerIsNear)
            {
                playerIsNear = true;
                if (agent != null) agent.isStopped = true;
            }
        }
        else if (playerIsNear && distance > detectionRange)
        {
            playerIsNear = false;
            if (agent != null) agent.isStopped = false;
        }
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