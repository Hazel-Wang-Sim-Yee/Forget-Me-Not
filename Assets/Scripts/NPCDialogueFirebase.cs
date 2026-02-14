/*
* Author: Jeffrey
* Date: 2026-02-09
* Description: This script manages the dialogue and interaction behavior of non-player characters (NPCs) in the game. It retrieves NPC dialogue data from Firebase, handles flower interactions with the player's XR controllers, and updates UI elements accordingly.
*/
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
    public string npcID; // Unique identifier for the NPC, used to fetch specific dialogue data from Firebase
    private DatabaseReference dbRef; // Reference to the Firebase Realtime Database, initialized in the Start method

    [Header("XR Settings")]
    [SerializeField]
    private XRSocketInteractor rightHandSocket; // Reference to the XRSocketInteractor for the player's right hand, used to detect when a flower is placed in the socket for interaction
    [SerializeField]
    private XRSocketInteractor leftHandSocket; // Reference to the XRSocketInteractor for the player's left hand, used to detect when the player takes the money after a successful transaction

    [Header("UI Settings")]
    [SerializeField]
    public TMP_Text DialogueBox; // Reference to the TextMeshPro text component that displays the NPC's dialogue, set in the Unity Editor

    [Header("Game References")]
    public GameObject NPC; // Reference to the NPC GameObject, used for movement and interaction purposes
    [SerializeField]
    private GameObject MoneyPrefab; // Reference to the prefab for the money that appears when the player successfully completes a transaction with the NPC, set in the Unity Editor
    private NPCBehaviour npcMovement;  // Reference to the NPCBehaviour script attached to the NPC, used to control the NPC's movement and behavior during interactions

    private string wantedFlowerType; // String to store the type of flower that the NPC wants, retrieved from Firebase and used to check against the player's selection during interactions
    private string successResponse; // String to store the NPC's response when the player successfully gives them the correct flower, retrieved from Firebase and displayed in the dialogue box
    private string failResponse; // String to store the NPC's response when the player gives them the wrong flower, retrieved from Firebase and displayed in the dialogue box
    private bool transactionComplete = false; // Flag to indicate whether the transaction with the NPC has been completed, used to manage the flow of interactions and NPC behavior after a successful transaction

    // This method is called when the script instance is being loaded. It initializes the Firebase Database reference, retrieves the NPCBehaviour component for controlling the NPC's movement, and calls the FetchNPCDialogue method to retrieve the NPC's dialogue data from Firebase and set up the initial state of the dialogue box and interaction variables.
    void Start()
    {
        NPC = this.gameObject;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        
        npcMovement = GetComponent<NPCBehaviour>();
        if (npcMovement == null) npcMovement = GetComponentInParent<NPCBehaviour>();
        if (npcMovement == null) npcMovement = GetComponentInChildren<NPCBehaviour>();

        FetchNPCDialogue();
    }

    // This method is called when the script instance is being enabled. It adds event listeners to the right hand socket to check for flower interactions and to the left hand socket to check when the player takes the money after a successful transaction. The CheckFlower method is called when a flower is placed in the right hand socket, and the OnMoneyTaken method is called when the player takes the money from the left hand socket.
    void OnEnable()
    {
        if (rightHandSocket != null) rightHandSocket.selectEntered.AddListener(CheckFlower);
        
        if (leftHandSocket != null) leftHandSocket.selectExited.AddListener(OnMoneyTaken);
    }

    // This method is called when the script instance is being disabled. It removes the event listeners from the right hand socket and left hand socket to prevent any unintended interactions or errors when the NPC is not active in the scene. This ensures that the CheckFlower and OnMoneyTaken methods are only called when the NPC is active and can interact with the player.
    void OnDisable()
    {
        if (rightHandSocket != null) rightHandSocket.selectEntered.RemoveListener(CheckFlower);
        if (leftHandSocket != null) leftHandSocket.selectExited.RemoveListener(OnMoneyTaken);
    }

    // This method retrieves the NPC's dialogue data from Firebase using the npcID to access the specific dialogue information for this NPC. It updates the DialogueBox with the retrieved dialogue text and stores the wanted flower type, success response, and fail response for use during player interactions. The method uses asynchronous calls to Firebase and ensures that the UI is updated on the main thread after retrieving the data.
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

    // This method is called when a flower is placed in the right hand socket. It checks if the name of the selected flower matches the wanted flower type retrieved from Firebase. If it matches, it updates the DialogueBox with the success response, instantiates the money prefab for the player to take, increments the happyCustomers count in the GameManager, and destroys the selected flower. If it does not match, it updates the DialogueBox with the fail response, destroys the selected flower, and triggers the NPC to exit if applicable.
    private void CheckFlower(SelectEnterEventArgs args)
    {
        GameObject selectedObject = args.interactableObject.transform.gameObject;
        string heldFlowerName = selectedObject.name;
        GameManager gm = GameManager.Instance;

        if (heldFlowerName.Contains(wantedFlowerType))
        {
            DialogueBox.text = successResponse;

            if (leftHandSocket != null && MoneyPrefab != null)
                Instantiate(MoneyPrefab, leftHandSocket.transform.position, Quaternion.identity);

            if (gm != null) gm.happyCustomers += 1;

            Destroy(selectedObject);
            
            transactionComplete = true; 

            Debug.Log("Flower accepted. Waiting for player to take money...");
        }
        else
        {
            DialogueBox.text = failResponse;
            Destroy(selectedObject);
            
            if (npcMovement != null && gm != null && gm.spawnLocation != null)
            {
                npcMovement.TriggerExit(gm.spawnLocation, 2.0f);
            }
        }
    }

    // This method is called when the player takes the money from the left hand socket after a successful transaction. It checks if the transactionComplete flag is true, indicating that the player has successfully completed the transaction. If so, it triggers the NPC to exit the scene by calling the TriggerExit method on the npcMovement reference, passing in the spawn location from the GameManager and a delay time before exiting. The transactionComplete flag is then reset to false to prevent any unintended behavior if the player interacts with the NPC again.
    private void OnMoneyTaken(SelectExitEventArgs args)
    {
        if (transactionComplete)
        {
            Debug.Log("Money taken! NPC leaving now.");
            
            GameManager gm = GameManager.Instance;
            if (npcMovement != null && gm != null && gm.spawnLocation != null)
            {
                npcMovement.TriggerExit(gm.spawnLocation, 1.0f);
            }
            
            transactionComplete = false;
        }
    }
}