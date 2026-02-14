/*
* Author: Alex
* Date: 2026-02-06
* Description: This script manages the bouquet wrapping process in the game. It checks if all flower slots are filled when the player interacts with the ribbon, and if so, it wraps the bouquet by destroying the individual flowers and instantiating a bouquet prefab based on the type of flowers used. The bouquet type is determined by the names of the flowers placed in the slots, and if multiple types of flowers are used, it defaults to a mixed bouquet. The script also handles the interaction logic for selecting flowers and determining the bouquet type.
*/
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class BouquetWrap : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] flowerSlots; // Array of socket interactors representing the flower slots, set in the Unity Editor

    public GameObject bouquetPrefab; // The prefab for the wrapped bouquet, set in the Unity Editor
    public bool isWrapped = false; // Flag to indicate whether the bouquet has been wrapped or not
    public string bouquetType = null; // String to store the type of bouquet based on the flowers used, determined by the names of the flowers placed in the slots

    [SerializeField]
    GameObject lilyBouquetPrefab; // The prefab for the lily bouquet, set in the Unity Editor

    [SerializeField]
    GameObject tulipBouquetPrefab; // The prefab for the tulip bouquet, set in the Unity Editor

    [SerializeField]
    GameObject daisyBouquetPrefab; // The prefab for the daisy bouquet, set in the Unity Editor

    [SerializeField]
    GameObject carnationBouquetPrefab; // The prefab for the carnation bouquet, set in the Unity Editor

    // This method is called when another collider enters the trigger collider of the bouquet wrapper. It checks if the collider belongs to the ribbon and if the bouquet has not already been wrapped. If both conditions are met, it checks if all flower slots are filled. If they are, it calls the WrapBouquet method to wrap the bouquet.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ribbon") && !isWrapped)
        {
            bool allSlotsFilled = true;
            foreach (var slot in flowerSlots)
            {
                if (!slot.hasSelection)
                {
                    allSlotsFilled = false;
                    break;
                }
            }

            if (allSlotsFilled)
            {
                Debug.Log("Wrapping bouquet...");
                WrapBouquet();
            }
        }
    }

    // This method is called when a flower is selected in one of the flower slots. It checks the name of the selected flower and updates the bouquetType variable accordingly. If the bouquetType is not set, it sets it to the name of the selected flower. If the bouquetType is already set to the same flower, it does nothing. If the bouquetType is set to a different flower, it changes it to "mixed" to indicate that multiple types of flowers are used in the bouquet.
    public void BouquetToCreate(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs other)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable = other.interactableObject;
        Debug.Log(interactable.transform.gameObject.name);
        Debug.Log("Current bouquet type: " + bouquetType);
        if (bouquetType == "")
        {
            bouquetType = interactable.transform.gameObject.name;
            Debug.Log("Set bouquet type to: " + bouquetType);
        }
        else if (bouquetType == interactable.transform.gameObject.name)
        {
            Debug.Log("Bouquet type already set to: " + bouquetType);
            return;
        }
        else
        {
            bouquetType = "mixed";
            Debug.Log("Set bouquet type to mixed");
        }
    }

    // This method handles the bouquet wrapping process. It sets the isWrapped flag to true, iterates through each flower slot, and if a slot has a selection, it deselects the flower and destroys its game object. It then determines the position and rotation for the bouquet based on the wrapper's transform, checks the bouquetType to determine which bouquet prefab to instantiate, and finally instantiates the bouquet prefab at the calculated position and rotation before destroying the wrapper game object.
    private void WrapBouquet()
    {
        isWrapped = true;

        foreach (var slot in flowerSlots)
        {
            if (!slot.hasSelection) continue;
            var flower = slot.interactablesSelected[0];

            slot.interactionManager.SelectExit(slot, flower); // Deselect the flower from the slot

            Destroy(flower.transform.gameObject);
        }

        Vector3 pos =
            transform.position +
            transform.up * 0.5f;

        Quaternion rot =
            transform.rotation *
            Quaternion.Euler(90f, 0f, 0f);

        if (bouquetType == "flowerLily(Clone)")
        {
            bouquetPrefab = lilyBouquetPrefab;
        }
        else if (bouquetType == "flowerTulip(Clone)")
        {
            bouquetPrefab = tulipBouquetPrefab;
        }
        else if (bouquetType == "flowerDaisy(Clone)")
        {
            bouquetPrefab = daisyBouquetPrefab;
        }
        else if (bouquetType == "flowerCarnation(Clone)")
        {
            bouquetPrefab = carnationBouquetPrefab;
        }
        else
        {
            // Default to a mixed bouquet prefab if needed
            bouquetPrefab = daisyBouquetPrefab; // Example default
        }

        Debug.Log("Creating bouquet of type: " + bouquetType);
        Debug.Log(bouquetPrefab.name);
        Instantiate(bouquetPrefab, pos, rot);
        Destroy(gameObject);
    }
}
