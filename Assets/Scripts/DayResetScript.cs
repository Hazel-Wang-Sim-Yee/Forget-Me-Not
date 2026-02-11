using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DayResetScript : MonoBehaviour
{
    public static DayResetScript Instance;

    private BoxBehaviourScript boxBehaviourScript;

    [SerializeField]
    private Transform SuppliesSpawnPoint;

    [SerializeField]
    private GameObject boxPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void TriggerDayReset()
    {
        StartCoroutine(DayReset());  
    }

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