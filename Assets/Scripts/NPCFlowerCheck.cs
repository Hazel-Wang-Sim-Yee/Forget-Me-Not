/*
* Author: Hazel
* Date: 2026-02-06
* Description: This script manages the behavior of NPCs in the game. It handles checking if a flower is placed in the correct socket and updates the NPC's dialogue accordingly.
*/
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Collections.Generic;

public class NPCFlowerCheck : MonoBehaviour
{

    public static NPCFlowerCheck Instance; // Singleton instance for easy access from other scripts

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor rightHandSocket; // Reference to the XRSocketInteractor for the player's right hand, used to detect when a flower is placed in the socket for interaction

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor leftHandSocket; // Reference to the XRSocketInteractor for the player's left hand, used to detect when the player takes the money after a successful transaction

    [SerializeField]
    public List<string> acceptableFlowerTypes; // List of strings representing the types of flowers that the NPC will accept, set in the Unity Editor

    public string wantedFlowerType; // String to store the type of flower that the NPC wants, retrieved from Firebase and used to check against the player's selection during interactions

    [SerializeField]
    public TMP_Text DialogueBox; // Reference to the TextMeshPro text component that displays the NPC's dialogue, set in the Unity Editor

    public GameObject NPC; // Reference to the NPC GameObject, used for movement and interaction purposes

    public string heldFlowerType; // String to store the type of flower that the player is currently holding, used to check against the wanted flower type during interactions

    [SerializeField]
    private GameObject MoneyPrefab; // Reference to the prefab for the money that appears when the player successfully completes a transaction with the NPC, set in the Unity Editor

    private GameObject selectedObject; // Reference to the currently selected object (flower) that the player is holding, used to check against the wanted flower type during interactions and to destroy the object after the transaction is complete

    GameManager gameManager; // Reference to the GameManager script, used to update the happyCustomers count when a successful transaction is completed

    // This method is called when the script instance is being loaded. It initializes the wantedFlowerType by randomly selecting one from the acceptableFlowerTypes list and updates the DialogueBox to prompt the player with the desired flower.
    void Start()
    {
        NPC = this.gameObject;
        wantedFlowerType = acceptableFlowerTypes[Random.Range(0, acceptableFlowerTypes.Count)];
        DialogueBox.SetText("I would love a " + wantedFlowerType + "!");
    }

    // This method is called when the script instance is being enabled. It adds event listeners to the right hand socket to check for flower interactions and to the left hand socket to check when the player takes the money after a successful transaction. The CheckFlower method is called when a flower is placed in the right hand socket, and the OnMoneyTaken method is called when the player takes the money from the left hand socket.
    public void onHoverEntered()
    {
        Debug.Log("Listener added");
        rightHandSocket.selectEntered.AddListener(CheckFlower);
    }

    // This method is called when the script instance is being disabled. It removes the event listeners from the right hand socket and left hand socket to prevent any unintended interactions or errors when the NPC is not active in the scene. This ensures that the CheckFlower and OnMoneyTaken methods are only called when the NPC is active and can interact with the player.
    public void onHoverExited()
    {
        rightHandSocket.selectEntered.RemoveListener(CheckFlower);
    }

    // This method is called when a flower is placed in the right hand socket. It checks if the name of the selected flower matches the wanted flower type retrieved from Firebase. If it matches, it updates the DialogueBox with a thank you message, instantiates the money prefab for the player to take, increments the happyCustomers count in the GameManager, and destroys the selected flower. If it does not match, it updates the DialogueBox with a message indicating that it's not the correct flower, destroys the selected flower, and triggers the NPC to exit if applicable.
    private void CheckFlower(SelectEnterEventArgs args)
    {
        Debug.Log("Flower placed in socket");
        selectedObject = args.interactableObject.transform.gameObject;
        heldFlowerType = selectedObject.name;

        if (heldFlowerType.Contains(wantedFlowerType))
        {
            DialogueBox.SetText("Thank you for the " + wantedFlowerType + "!");
            NPC.GetComponent<Renderer>().material.color = Color.green; // Indicate success
            Instantiate(MoneyPrefab, leftHandSocket.transform.position, Quaternion.identity);
            gameManager = FindObjectOfType<GameManager>();
            gameManager.happyCustomers += 1;
        }
        else
        {
            DialogueBox.SetText("This is not the " + wantedFlowerType + " I wanted...");
            NPC.GetComponent<Renderer>().material.color = Color.red; // Indicate failure
            LeaveShop();
        }
    }

    // This method is called when the player takes the money from the left hand socket after a successful transaction. It checks if the transactionComplete flag is true, indicating that the player has successfully completed the transaction. If so, it triggers the NPC to exit the scene by calling the TriggerExit method on the npcMovement reference, passing in the spawn location from the GameManager and a delay time before exiting. The transactionComplete flag is then reset to false to prevent any unintended behavior if the player interacts with the NPC again.
    public void LeaveShop()
    {
        Destroy(selectedObject);
        Destroy(NPC);
    }
}