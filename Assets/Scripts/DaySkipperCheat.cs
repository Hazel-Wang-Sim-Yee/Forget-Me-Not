using UnityEngine;
using UnityEngine.InputSystem; // <-- We need this to use the new system!

public class DaySkipperCheat : MonoBehaviour
{
    void Update()
    {
        // 1. Make sure a keyboard is actually plugged in/detected
        if (Keyboard.current == null) return;

        // 2. Ask the NEW Input System if 'F' was pressed this exact frame
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log("<color=red>CHEAT ACTIVATED: Fast-forwarding day...</color>");
            ExecuteSkipCheat();
        }
    }

    private void ExecuteSkipCheat()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        // Destroy the current NPC at the counter (and any others roaming)
        NPCBehaviour[] allNPCs = FindObjectsOfType<NPCBehaviour>();
        foreach (NPCBehaviour npc in allNPCs)
        {
            Destroy(npc.gameObject);
        }

        // Clean up today's flower boxes
        BoxBehaviourScript[] allBoxes = FindObjectsOfType<BoxBehaviourScript>();
        foreach (BoxBehaviourScript box in allBoxes)
        {
            Destroy(box.gameObject);
        }

        // The Magic Trick: Tell GameManager we already served everyone
        gm.currentCustomerIndex = gm.totalCustomers;
    }
}