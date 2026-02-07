using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BouquetWrap : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] flowerSlots;

    public GameObject bouquetPrefab;
    public bool isWrapped = false;
    public string bouquetType;
    [SerializeField]
    public int bouquetSize;
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
                BouquetToCreate(other.gameObject);
            }

            if (allSlotsFilled)
            {
                WrapBouquet();
            }
        }
    }

    private void BouquetToCreate(GameObject other)
    {
        
        if (bouquetType == null)
        {
            bouquetType = other.name;
        }
        else if (bouquetType == other.name)
        {
            return;
        }
        else
        {
            bouquetType = "mixed";
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

        if (bouquetSize >= 3)
        {
            bouquetPrefab = Resources.Load<GameObject>("Bouquet(M)_" + bouquetType);
        }
        else
        {
            bouquetPrefab = Resources.Load<GameObject>("Bouquet(S)_" + bouquetType);
        }

        Instantiate(bouquetPrefab, pos, rot);
        Destroy(gameObject);
    }
}
