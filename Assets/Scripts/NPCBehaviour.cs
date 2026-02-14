/*
* Author: Jeffrey
* Date: 2026-02-08
* Description: This script manages the behavior of non-player characters (NPCs) in the game. It handles NPC movement, animation, interaction with the player, and roaming behavior within a defined area.
*/
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
    private NavMeshAgent agent; // Reference to the NavMeshAgent component for handling NPC movement
    private Animator npcAnimator; // Reference to the Animator component for controlling NPC animations
    private Transform currentTarget; // The current target destination for the NPC, used for movement and rotation
    private GameObject dialogueCanvas; // Reference to the dialogue canvas GameObject that displays NPC dialogue, set in the Unity Editor
    private GameObject exteriorCanvas; // Reference to the exterior dialogue canvas GameObject that displays dialogue when the player is near the NPC in the exterior, set in the Unity Editor
    private Transform npcModel;  // Reference to the NPC's model transform for applying rotation offsets, set in the Unity Editor

    [Header("SETTINGS")]
    public float rotationSpeed = 10f; // Speed at which the NPC rotates towards its target or the player, set in the Unity Editor
    [Range(-180, 180)]
    public float idleRotationOffset = 0f; // Rotation offset applied to the NPC's model when in idle state, allowing for a more natural look, set in the Unity Editor

    [Header("ROAMING")]
    public bool isRoaming = false; // Flag to indicate whether the NPC should roam around the exterior, set in the Unity Editor
    public float wanderRadius = 15f; // The radius within which the NPC will roam when in roaming mode, set in the Unity Editor
    public float minWaitTime = 2f; // Minimum time the NPC will wait at a destination before moving to the next one when roaming, set in the Unity Editor
    public float maxWaitTime = 5f; // Maximum time the NPC will wait at a destination before moving to the next one when roaming, set in the Unity Editor

    [Header("DISTANCE INTERACTION")]
    public float detectionRange = 4.0f;  // The distance within which the NPC will detect the player and trigger interactions, set in the Unity Editor
    private Transform playerTransform; // Reference to the player's transform for distance detection and interaction, set in the Unity Editor
    private bool playerIsNear = false; // Flag to track whether the player is currently within the detection range of the NPC, initialized to false
    private bool isExiting = false; // Flag to indicate whether the NPC is currently exiting the scene, initialized to false
    private Vector3 moveDirection;  // The current movement direction of the NPC, used for rotating the NPC model to face the direction of movement   

    private string cleanNpcName; // A cleaned version of the NPC's name used for Firebase database references, initialized in the Awake method
    private bool hasInteracted = false; // Flag to track whether the player has interacted with the NPC, used to prevent repeated interactions, initialized to false

    // This method is called when the script instance is being loaded. It initializes references to the NavMeshAgent and Animator components, cleans the NPC's name for Firebase references, finds the dialogue canvases, and determines whether the NPC should start in roaming mode based on the current scene. It also sets the NavMeshAgent to not update rotation automatically, allowing for custom rotation handling in the script.
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

    // This method is called when the script instance is being enabled. It finds the player in the scene and starts the wandering routine if the NPC is set to roam.
    private void Start()
    {
        FindPlayer();
        
        if (dialogueCanvas == null || exteriorCanvas == null)
        {
            FindCanvases();
        }

        if (isRoaming) StartCoroutine(WanderRoutine());
    }

    // This method is responsible for finding the dialogue canvases in the NPC's children. It searches through all child Canvas components and assigns the dialogueCanvas and exteriorCanvas references based on their names. The dialogueCanvas is used for interactions when the player is close to the NPC, while the exteriorCanvas is used for displaying dialogue when the NPC is roaming in the exterior. Both canvases are initially set to inactive to ensure they only appear when needed during interactions.
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

    // This method is responsible for finding the player GameObject in the scene and assigning its transform to the playerTransform variable. It uses the GameObject.FindGameObjectWithTag method to search for an object tagged as "Player". If a player object is found, its transform is assigned to playerTransform for use in distance detection and interactions. If no player object is found, playerTransform remains null, which is handled in the distance detection logic to prevent errors.
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    // This method is called once per frame. It handles the NPC's behavior based on its current state, such as roaming, interacting with the player, and exiting the scene. It checks the distance to the player for interaction triggers, manages animations based on movement and interactions, and handles rotation towards targets or the player. The method also ensures that dialogue canvases are shown or hidden appropriately based on the NPC's state and proximity to the player.
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

    // This method rotates the NPC's model to face the direction of movement while applying an idle rotation offset when the NPC is not moving. It calculates the target rotation based on the moveDirection and applies the idleRotationOffset when the NPC is in an idle state. The rotation is smoothly interpolated using Quaternion.Slerp for a natural look.
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

    // This method fetches the NPC's order text from Firebase and updates the dialogue canvas with the retrieved text. It constructs the database reference using the cleanNpcName to access the specific NPC's order data. If the data retrieval is successful and the order text exists, it updates the TextMeshProUGUI component in the dialogue canvas with the fetched order text. If the order text cannot be found, it logs a warning message to the console.
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

    // This method is called when the player interacts with the NPC. It sets the hasInteracted flag to true to prevent repeated interactions, plays the "Greetings" animation, and activates the dialogue canvas to display the interaction dialogue. It also fetches the interaction dialogue from Firebase using the FetchDialogueFromFirebase method. Finally, it updates the player's interaction status with this NPC in Firebase by calling the UpdateFirebaseStatus method.
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

    // This method is called when the player presses the recall button for the NPC. It sets the hasInteracted flag to true, plays the "Greetings" animation, and activates the dialogue canvas to display the recall dialogue. It fetches the recall dialogue from Firebase using the FetchDialogueFromFirebase method. Additionally, it updates the player's recall status for this NPC in Firebase by calling the UpdateFirebaseStatus method, and then checks if all NPCs have been recalled to potentially unlock a twist in the game by calling CheckAllRecalledAndUnlockTwist.
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

    // This method is called when the player exits the shop. It triggers the exit routine, which moves the NPC towards a specified exit point and eventually destroys the NPC GameObject once it reaches the exit. The method also ensures that the dialogue canvas is hidden when the NPC starts exiting.
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

    // This method is responsible for fetching the NPC's dialogue from Firebase based on the specified dialogue type (e.g., "interact_dialogue" or "recall_dialogue"). It constructs the database reference using the cleanNpcName to access the specific NPC's dialogue data. If the data retrieval is successful and the dialogue text exists, it updates the TextMeshProUGUI component in the dialogue canvas with the fetched dialogue text. If the dialogue text cannot be found, it logs a warning message to the console.
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

    // This method updates the player's interaction or recall status for this NPC in Firebase. It checks if there is a currently authenticated user, and if so, it constructs the database reference using the cleanNpcName to update the specific status (either "npcs" for interactions or "recalled_npcs" for recalls) with the provided boolean status value.
    private void UpdateFirebaseStatus(string folder, bool status)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            dbRef.Child("users").Child(userId).Child(folder).Child(cleanNpcName).SetValueAsync(status);
        }
    }

    // This method is responsible for handling distance detection between the NPC and the player. It checks if the NPC is in roaming mode and not currently exiting, then calculates the distance to the player. If the player is within the detection range, it sets the playerIsNear flag to true and stops the NPC's movement. If the player moves out of range, it resets the playerIsNear flag and allows the NPC to resume roaming if applicable.
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

    // This method rotates the NPC to face a specified target position. It calculates the direction vector from the NPC's current position to the target position, normalizes it, and then creates a look rotation based on that direction. The NPC's rotation is then smoothly interpolated towards the look rotation using Quaternion.Slerp for a natural turning motion. The y-component of the direction is set to 0 to ensure that the NPC only rotates around the vertical axis, keeping it upright.
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

    // This method sets a new target destination for the NPC to move towards. It checks if the provided destination is not null and if the NavMeshAgent reference is valid. If both conditions are met, it calls the SetDestination method of the NavMeshAgent to move the NPC towards the specified destination position, and updates the currentTarget reference to keep track of the new target.
    public void SetTarget(Transform destination) 
    { 
        if (destination != null && agent != null) 
        { 
            agent.SetDestination(destination.position); 
            currentTarget = destination; 
        } 
    }

    // This method is called to trigger the NPC's exit from the scene. It stops all current coroutines to ensure that any ongoing roaming behavior is halted, then starts the ExitRoutine coroutine which will move the NPC towards a specified exit point after a delay. The method also ensures that the dialogue canvas is hidden when the NPC starts exiting.
    public void TriggerExit(Transform exitPoint, float delaySeconds) 
    { 
        StopAllCoroutines(); 
        StartCoroutine(ExitRoutine(exitPoint, delaySeconds)); 
    }

    // This coroutine handles the wandering behavior of the NPC when it is in roaming mode. It continuously generates random destinations within a specified radius and moves the NPC towards those destinations. After reaching each destination, it waits for a random amount of time between minWaitTime and maxWaitTime before generating the next destination. The wandering behavior continues until the NPC is set to exit the scene.
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

    // This method generates a random point on the NavMesh within a specified radius from a given center position. It uses Random.insideUnitSphere to generate a random direction and distance, then adds that to the center position to get a random point in the world. It then uses NavMesh.SamplePosition to find the nearest valid point on the NavMesh to that random point, ensuring that the NPC can navigate to it. If a valid point is found, it returns that position; otherwise, it returns the original center position as a fallback.
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius) 
    { 
        Vector3 randomDirection = Random.insideUnitSphere * radius; 
        randomDirection += center; 
        NavMeshHit hit; 
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) return hit.position; 
        return center; 
    }

    // This coroutine is responsible for moving the NPC towards a specified exit point after a delay. It first waits for the specified delay time, then sets the NPC's target to the exit point and marks it as exiting. The NPC will then move towards the exit point, and once it is close enough (within 1.5 units), the NPC GameObject will be destroyed to remove it from the scene. The method also ensures that the dialogue canvas is hidden when the NPC starts exiting.
    IEnumerator ExitRoutine(Transform exitPoint, float delay) 
    { 
        yield return new WaitForSeconds(delay); 
        SetTarget(exitPoint); 
        isExiting = true; 
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false); 
    }

    // This method rotates the NPC to face the direction of movement based on the current velocity of the NavMeshAgent. It checks if the moveDirection vector is not zero, then calculates a target rotation using Quaternion.LookRotation based on the moveDirection. The NPC's rotation is then smoothly interpolated towards the target rotation using Quaternion.Slerp for a natural turning motion. The y-component of the moveDirection is set to 0 to ensure that the NPC only rotates around the vertical axis, keeping it upright.
    void RotateRootToMovement() 
    { 
        moveDirection.y = 0; 
        if (moveDirection != Vector3.zero) 
        { 
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection); 
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); 
        } 
    }

    // This method rotates the NPC's root transform to face the current target destination. It checks if the currentTarget reference is not null, then calculates the final rotation based on the target's rotation. The NPC's rotation is smoothly interpolated towards the final rotation using Quaternion.Slerp for a natural turning motion. This method is typically called when the NPC is in an idle state and needs to face its target, such as when it has reached a destination or is interacting with the player.
    void RotateRootToTarget() 
    { 
        if (currentTarget != null) 
        { 
            Quaternion finalRotation = currentTarget.rotation; 
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSpeed * Time.deltaTime); 
        } 
    }

    // This method applies a rotation offset to the NPC's model when it is in an idle state. It calculates an offset rotation using the idleRotationOffset value and applies it to the npcModel's local rotation. The rotation is smoothly interpolated using Quaternion.Slerp for a natural look. This allows the NPC to have a more relaxed and natural posture when it is not moving, rather than always facing directly forward.
    void RotateModelWithOffset() 
    { 
        if (npcModel != null) 
        { 
            Quaternion offsetRotation = Quaternion.Euler(0, idleRotationOffset, 0); 
            npcModel.localRotation = Quaternion.Slerp(npcModel.localRotation, offsetRotation, rotationSpeed * Time.deltaTime); 
        } 
    }

    // This method resets the NPC's model rotation back to its default orientation when it is moving. It smoothly interpolates the npcModel's local rotation back to Quaternion.identity (no rotation) using Quaternion.Slerp for a natural transition. This ensures that the NPC's model faces forward while moving, and only applies the idle rotation offset when the NPC is in an idle state.
    void ResetModelRotation() 
    { 
        if (npcModel != null) 
        { 
            npcModel.localRotation = Quaternion.Slerp(npcModel.localRotation, Quaternion.identity, rotationSpeed * Time.deltaTime); 
        } 
    }
}