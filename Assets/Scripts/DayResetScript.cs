/*
* Author: Hazel
* Date: 2026-02-06
* Description: This script manages the day reset process in the game. It is responsible for spawning the appropriate boxes with flowers based on the NPCs that are present for the day. The script checks the names of the NPCs to determine which types of flowers are needed and then instantiates boxes with the corresponding flowers at a designated spawn point. The day reset process is triggered by calling the TriggerDayReset method, which starts a coroutine to handle the spawning of boxes and flowers in a sequential manner with a delay between each spawn.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DayResetScript : MonoBehaviour
{
    public static DayResetScript Instance; // Singleton instance for easy access from other scripts

    private BoxBehaviourScript boxBehaviourScript; // Reference to the BoxBehaviourScript to manage the boxes and flowers that are spawned during the day reset process

    [SerializeField]
    private Transform SuppliesSpawnPoint; // Reference to the Transform component representing the spawn point for the boxes and flowers, set in the Unity Editor

    [SerializeField]
    private GameObject boxPrefab; // Reference to the box prefab that will be instantiated during the day reset process, set in the Unity Editor

    // This method is called when the script instance is being loaded. It assigns the current instance of the DayResetScript to the static Instance variable, allowing other scripts to easily access it.
    private void Awake()
    {
        Instance = this;
    }

    // This method is called to trigger the day reset process. It starts a coroutine that handles the spawning of boxes and flowers based on the NPCs present for the day.
    public void TriggerDayReset()
    {
        StartCoroutine(DayReset());  
    }

    // This coroutine handles the day reset process by spawning boxes with the appropriate flowers based on the NPCs present for the day. It first retrieves the spawn point for the boxes, then iterates through the list of daily NPCs to determine which types of flowers are needed. For each required flower type, it instantiates a box at the spawn point, updates the box label, and assigns the corresponding flower prefab to be spawned inside the box. The coroutine includes a delay between each box spawn to ensure that they are spawned sequentially.
    public IEnumerator DayReset()
    {
        // USE GameManager.Instance DIRECTLY!
        Debug.Log("Day Reset Initiated. Current Day: " + GameManager.Instance.currentDay);

        SuppliesSpawnPoint = GameObject.Find("SuppliesSpawnPoint").transform;
        
        HashSet<string> requiredFlowers = new HashSet<string>();

        foreach (GameObject npc in GameManager.Instance.dailyNPCs)
        {
            string npcName = npc.name.ToLower();

            if (npcName.Contains("ah_boon")) requiredFlowers.Add("Tulips");
            else if (npcName.Contains("mdm_wei_ting") || npcName.Contains("mrs_raj")) requiredFlowers.Add("Lilies");
            else if (npcName.Contains("siti")) requiredFlowers.Add("Daisies");
            else if (npcName.Contains("uncle_tan")) requiredFlowers.Add("Carnations");
        }

        foreach (string flowerType in requiredFlowers)
        {
            GameObject thisBox = Instantiate(boxPrefab, SuppliesSpawnPoint.position, SuppliesSpawnPoint.rotation);
            boxBehaviourScript = thisBox.GetComponent<BoxBehaviourScript>();

            if (flowerType == "Carnations")
            {
                Debug.Log("Carnations box spawned");
                boxBehaviourScript.UpdateBoxLabel("Carnations");
                boxBehaviourScript.flowersInside = boxBehaviourScript.CarnationsFlowers;
            }
            else if (flowerType == "Tulips")
            {
                Debug.Log("Tulips box spawned");
                boxBehaviourScript.UpdateBoxLabel("Tulips");
                boxBehaviourScript.flowersInside = boxBehaviourScript.TulipsFlowers;
            }
            else if (flowerType == "Daisies")
            {
                Debug.Log("Daisies box spawned");
                boxBehaviourScript.UpdateBoxLabel("Daisies");
                boxBehaviourScript.flowersInside = boxBehaviourScript.DaisiesFlowers;
            }
            else if (flowerType == "Lilies")
            {
                Debug.Log("Lilies box spawned");
                boxBehaviourScript.UpdateBoxLabel("Lilies");
                boxBehaviourScript.flowersInside = boxBehaviourScript.LiliesFlowers;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}