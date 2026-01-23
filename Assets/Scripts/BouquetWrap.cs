using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BouquetWrap : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor[] flowerSlots;

    public GameObject bouquetPrefab;
    public bool isWrapped = false;
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
                WrapBouquet();
            }
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
            Quaternion.Euler(0f, 90f, 0f);

        Instantiate(bouquetPrefab, pos, rot);
        Destroy(gameObject);
    }
}
