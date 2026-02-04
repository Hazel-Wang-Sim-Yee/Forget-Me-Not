using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using System.Collections.Generic;
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
    private XRSocketInteractor rightHandSocket; 
    [SerializeField]
    private XRSocketInteractor leftHandSocket;  

    [Header("UI Settings")]
    [SerializeField]
    public TMP_Text DialogueBox;

    [Header("Game References")]
    public GameObject NPC;
    [SerializeField]
    private GameObject MoneyPrefab;
    private GameManager gameManager;

    // Internal State
    private string wantedFlowerType;
    private string successResponse;
    private string failResponse;
    private GameObject selectedObject;

    void Start()
    {
        NPC = this.gameObject;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        FetchNPCDialogue();
    }

    void OnEnable()
    {
        if (rightHandSocket != null)
        {
            rightHandSocket.selectEntered.AddListener(CheckFlower);
        }
    }

    void OnDisable()
    {
        if (rightHandSocket != null)
        {
            rightHandSocket.selectEntered.RemoveListener(CheckFlower);
        }
    }

    void FetchNPCDialogue()
    {
        DialogueBox.text = "Loading...";

        dbRef.Child("npc_data").Child("dialogue").Child(npcID).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Firebase Error: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists) 
                {
                    DialogueBox.text = snapshot.Child("order").Child("text").Value.ToString();
                    wantedFlowerType = snapshot.Child("order").Child("item_requested").Value.ToString();
                    successResponse = snapshot.Child("response_success").Value.ToString();
                    failResponse = snapshot.Child("response_fail").Value.ToString();
                    
                    Debug.Log(npcID + " wants: " + wantedFlowerType);
                }
                else
                {
                    DialogueBox.text = "I... I don't remember what I wanted.";
                }
            }
        });
    }

    private void CheckFlower(SelectEnterEventArgs args)
    {
        // 1. Get the object name
        selectedObject = args.interactableObject.transform.gameObject;
        string heldFlowerName = selectedObject.name;

        Debug.Log("Checking: '" + heldFlowerName + "' against wanted: '" + wantedFlowerType + "'");

        // 2. Check if the name contains the Firebase string
        // This handles "Sunflower(Clone)" containing "Sunflower"
        if (heldFlowerName.Contains(wantedFlowerType))
        {
            // --- SUCCESS ---
            DialogueBox.text = successResponse;
            NPC.GetComponent<Renderer>().material.color = Color.green;
            
            if (leftHandSocket != null && MoneyPrefab != null)
            {
                Instantiate(MoneyPrefab, leftHandSocket.transform.position, Quaternion.identity);
            }

            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null) gameManager.happyCustomers += 1;

            Invoke("LeaveShop", 2.0f); 
        }
        else
        {
            // --- FAILURE ---
            DialogueBox.text = failResponse;
            NPC.GetComponent<Renderer>().material.color = Color.red;
            Invoke("LeaveShop", 2.0f); 
        }
    }

    public void LeaveShop()
    {
        if (selectedObject != null) Destroy(selectedObject);
        Destroy(NPC);
    }
}