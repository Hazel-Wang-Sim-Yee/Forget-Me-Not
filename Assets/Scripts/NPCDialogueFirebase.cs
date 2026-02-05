using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class NPCDialogueFirebase : MonoBehaviour
{
    [Header("Firebase Settings")]
    public string npcID;
    private DatabaseReference dbRef;

    [Header("XR Settings")]
    [SerializeField]
    private XRSocketInteractor rightHandSocket; // For Flower
    [SerializeField]
    private XRSocketInteractor leftHandSocket;  // For Money

    [Header("UI Settings")]
    [SerializeField]
    public TMP_Text DialogueBox;

    [Header("Game References")]
    public GameObject NPC;
    [SerializeField]
    private GameObject MoneyPrefab;
    private NPCBehaviour npcMovement; 

    // Internal State
    private string wantedFlowerType;
    private string successResponse;
    private string failResponse;
    private bool transactionComplete = false; // Flag to ensure he doesn't leave twice

    void Start()
    {
        NPC = this.gameObject;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        
        // ROBUST SCRIPT FINDING (Parent/Self/Children)
        npcMovement = GetComponent<NPCBehaviour>();
        if (npcMovement == null) npcMovement = GetComponentInParent<NPCBehaviour>();
        if (npcMovement == null) npcMovement = GetComponentInChildren<NPCBehaviour>();

        FetchNPCDialogue();
    }

    void OnEnable()
    {
        // Listen for Flower Input
        if (rightHandSocket != null) rightHandSocket.selectEntered.AddListener(CheckFlower);
        
        // NEW: Listen for Money Removal
        if (leftHandSocket != null) leftHandSocket.selectExited.AddListener(OnMoneyTaken);
    }

    void OnDisable()
    {
        if (rightHandSocket != null) rightHandSocket.selectEntered.RemoveListener(CheckFlower);
        if (leftHandSocket != null) leftHandSocket.selectExited.RemoveListener(OnMoneyTaken);
    }

    void FetchNPCDialogue()
    {
        DialogueBox.text = "Loading...";
        dbRef.Child("npc_data").Child("dialogue").Child(npcID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                DialogueBox.text = snapshot.Child("order").Child("text").Value.ToString();
                wantedFlowerType = snapshot.Child("order").Child("item_requested").Value.ToString();
                successResponse = snapshot.Child("response_success").Value.ToString();
                failResponse = snapshot.Child("response_fail").Value.ToString();
            }
        });
    }

    private void CheckFlower(SelectEnterEventArgs args)
    {
        GameObject selectedObject = args.interactableObject.transform.gameObject;
        string heldFlowerName = selectedObject.name;
        GameManager gm = GameManager.Instance;

        if (heldFlowerName.Contains(wantedFlowerType))
        {
            // --- SUCCESS ---
            DialogueBox.text = successResponse;

            // Spawn Money
            if (leftHandSocket != null && MoneyPrefab != null)
                Instantiate(MoneyPrefab, leftHandSocket.transform.position, Quaternion.identity);

            if (gm != null) gm.happyCustomers += 1;

            Destroy(selectedObject);
            
            // SET FLAG: Transaction is done, waiting for player to take money
            transactionComplete = true; 

            // IMPORTANT: We do NOT call TriggerExit here anymore. We wait.
            Debug.Log("Flower accepted. Waiting for player to take money...");
        }
        else
        {
            // --- FAILURE ---
            DialogueBox.text = failResponse;
            Destroy(selectedObject);
            
            // If failed, just leave after 2 seconds (no money to take)
            if (npcMovement != null && gm != null && gm.spawnLocation != null)
            {
                npcMovement.TriggerExit(gm.spawnLocation, 2.0f);
            }
        }
    }

    // --- NEW FUNCTION: Called when player grabs the money ---
    private void OnMoneyTaken(SelectExitEventArgs args)
    {
        // Only exit if the transaction was actually successful
        if (transactionComplete)
        {
            Debug.Log("Money taken! NPC leaving now.");
            
            GameManager gm = GameManager.Instance;
            if (npcMovement != null && gm != null && gm.spawnLocation != null)
            {
                // Leave after a short delay (1 second) so it doesn't look instant
                npcMovement.TriggerExit(gm.spawnLocation, 1.0f);
            }
            
            // Reset flag so it doesn't trigger again
            transactionComplete = false;
        }
    }
}