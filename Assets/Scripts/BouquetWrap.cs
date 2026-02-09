using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class BouquetWrap : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] flowerSlots;

    public GameObject bouquetPrefab;
    public bool isWrapped = false;
    public string bouquetType = null;

    [SerializeField]
    GameObject lilyBouquetPrefab;

    [SerializeField]
    GameObject tulipBouquetPrefab;

    [SerializeField]
    GameObject daisyBouquetPrefab;

    [SerializeField]
    GameObject carnationBouquetPrefab;


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
