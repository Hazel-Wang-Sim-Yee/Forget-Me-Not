/*
* Author: Hazel
* Date: 2026-02-06
* Description: Automatically adds money to the cash register when it collides with the cash register's collider. The money value is set in the Unity Editor.
*/
using UnityEngine;

public class MoneyBehaviourScript : MonoBehaviour
{
    public float moneyValue; // The value of the money, set in the Unity Editor
    [SerializeField]
    private Collider CashRegisterCollider; // Reference to the Collider component of the cash register, set in the Unity Editor

    NPCFlowerCheck npcFlowerCheck; // Reference to the NPCFlowerCheck script to manage NPC interactions

    // This method is called when the script instance is being loaded. It finds the Collider component of the cash register and assigns it to the CashRegisterCollider variable. It also gets the instance of the NPCFlowerCheck script for managing NPC interactions.
    void Start()
    {
        CashRegisterCollider = FindObjectOfType<CashRegisterBehaviour>().GetComponent<Collider>();
        npcFlowerCheck = NPCFlowerCheck.Instance;
    }

    // This method is called when another collider enters the trigger collider of the money object. It checks if the collider belongs to the cash register, and if so, it adds the money value to the amount in the cash register, updates the display of the amount in the register, and destroys the money object.
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Money collided with: " + other.name);
        if (other == CashRegisterCollider)
        {
            CashRegisterBehaviour.Instance.amountInRegister += moneyValue;
            CashRegisterBehaviour.Instance.UpdateAmountInRegister();
            Destroy(gameObject);
        }
    }
}
