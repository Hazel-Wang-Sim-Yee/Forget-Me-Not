/*
* Author: Hazel
* Date: 2026-02-06
* Description: Behaviour script for the cash register in the game. It manages the state of the cash register (locked/unlocked), handles interactions with the player, and updates the amount of money in the register. The cash register can be unlocked when the player has money, and it will lock again when the player leaves the shop.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CashRegisterBehaviour : MonoBehaviour
{
    public static CashRegisterBehaviour Instance; // Singleton instance for easy access from other scripts

    NPCFlowerCheck npcFlowerCheck; // Reference to the NPCFlowerCheck script to manage NPC interactions

    private bool isLocked = true; // Indicates whether the cash register is currently locked or unlocked
    public float amountInRegister = 0f; // The amount of money currently in the cash register, initialized to 0

    [SerializeField]
    private GameObject cashDrawer; // Reference to the cash drawer GameObject, set in the Unity Editor

    [SerializeField]
    private ConfigurableJoint drawerJoint; // Reference to the ConfigurableJoint component that controls the movement of the cash drawer, set in the Unity Editor

    [SerializeField]
    private Collider cashRegisterCollider; // Reference to the Collider component of the cash register, set in the Unity Editor

    [SerializeField]
    private TextMeshProUGUI amountInRegisterText; // Reference to the TextMeshProUGUI component that displays the amount of money in the register, set in the Unity Editor

    //Makes Register drawer moveable
    public void UnlockRegister()
    {
        drawerJoint.xMotion = ConfigurableJointMotion.Limited;
    }

    //Locks Register drawer
    public void LockRegister()
    {
        drawerJoint.xMotion = ConfigurableJointMotion.Locked;
    }

    // This method is called when the player attempts to open the cash register. It checks if the player has money and if the register is currently locked. If both conditions are met, it unlocks the register and moves the cash drawer to simulate opening. If the player does not have money, it logs a message indicating that the register cannot be opened.
    public void OpenRegister()
    {
        Debug.Log("Attempting to open register...");
        GameObject money = GameObject.FindWithTag("Money");
        if (money != null && isLocked)
        {
            isLocked = false;
            cashDrawer.transform.localPosition = new Vector3(cashDrawer.transform.localPosition.x, cashDrawer.transform.localPosition.y, cashDrawer.transform.localPosition.z - 0.3f);
        }
        else
        {
            Debug.Log("No money to open the register.");
        }
    }

    // This method is called when the player attempts to close the cash register. It checks if the register is currently unlocked. If it is, it locks the register and moves the cash drawer back to its original position to simulate closing.
    void Update()
    {
        if (isLocked)
        {
            LockRegister();
        }
        else
        {
            UnlockRegister();
        }
    }

    // This method is called when another collider enters the trigger collider of the cash register. It checks if the collider belongs to the cash register, and if so, it locks the register and calls the LeaveShop method on the NPCFlowerCheck script to manage NPC interactions when the player leaves the shop.
    void OnTriggerEnter(Collider other)
    {
        if (cashRegisterCollider == other)
        {
            isLocked = true;
            npcFlowerCheck = FindObjectOfType<NPCFlowerCheck>();
            npcFlowerCheck.LeaveShop();
        }
    }

    // This method updates the text displayed on the cash register to show the current amount of money in the register. It formats the amount to two decimal places for better readability.
    public void UpdateAmountInRegister()
    {
        amountInRegisterText.text = "Amount in Register: $" + amountInRegister.ToString("F2");
    }
}
