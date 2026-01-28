using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Collections.Generic;

public class NPCFlowerCheck : MonoBehaviour
{

    public static NPCFlowerCheck Instance;

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor rightHandSocket;

    [SerializeField]
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor leftHandSocket;

    [SerializeField]
    public List<string> acceptableFlowerTypes;

    public string wantedFlowerType;

    [SerializeField]
    public TMP_Text DialogueBox;

    public GameObject NPC;

    private string heldFlowerType;

    [SerializeField]
    private GameObject MoneyPrefab;

    private GameObject selectedObject;

    GameManager gameManager;

    void Start()
    {
        NPC = this.gameObject;
        wantedFlowerType = acceptableFlowerTypes[Random.Range(0, acceptableFlowerTypes.Count)];
        DialogueBox.SetText("I would love a " + wantedFlowerType + "!");
    }

    public void onHoverEntered()
    {
        Debug.Log("Listener added");
        rightHandSocket.selectEntered.AddListener(CheckFlower);
    }

    public void onHoverExited()
    {
        rightHandSocket.selectEntered.RemoveListener(CheckFlower);
    }

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

    public void LeaveShop()
    {
        Destroy(selectedObject);
        Destroy(NPC);
    }
}