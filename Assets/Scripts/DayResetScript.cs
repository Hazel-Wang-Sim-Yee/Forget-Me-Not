using UnityEngine;
using System.Collections;

public class DayResetScript : MonoBehaviour
{
    public static DayResetScript Instance;

    BoxBehaviourScript boxBehaviourScript;

    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    Transform SuppliesSpawnPoint;

    [SerializeField]
    GameObject boxPrefab;

    GameObject thisBox;

    private void Start()
    {
        StartCoroutine(DayReset());  
    }

    public IEnumerator DayReset()
    {
        Debug.Log("Day Reset Initiated");
        Debug.Log("Current Day: " + gameManager.currentDay);
        gameManager.totalCustomers = gameManager.currentDay + 1;

        SuppliesSpawnPoint = GameObject.Find("SuppliesSpawnPoint").transform;
        for (int i = 0; i < (gameManager.currentDay + 1); i++)
        {
            GameObject thisBox = Instantiate(boxPrefab, SuppliesSpawnPoint.position, SuppliesSpawnPoint.rotation);
            boxBehaviourScript = thisBox.GetComponent<BoxBehaviourScript>();
            if (i % 4 == 1)
            {
                Debug.Log("Carnations box spawned");
                boxBehaviourScript.UpdateBoxLabel("Carnations");
                boxBehaviourScript.flowersInside = boxBehaviourScript.CarnationsFlowers;
            }
            else if (i % 4 == 2)
            {
                Debug.Log("Tulips box spawned");
                boxBehaviourScript.UpdateBoxLabel("Tulips");
                boxBehaviourScript.flowersInside = boxBehaviourScript.TulipsFlowers;
            }
            else if (i % 4 == 3)
            {
                Debug.Log("Daisies box spawned");
                boxBehaviourScript.UpdateBoxLabel("Daisies");
                boxBehaviourScript.flowersInside = boxBehaviourScript.DaisiesFlowers;
            }
            else if (i % 4 == 0)
            {
                Debug.Log("Lilies box spawned");
                boxBehaviourScript.UpdateBoxLabel("Lilies");
                boxBehaviourScript.flowersInside = boxBehaviourScript.LiliesFlowers;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

}
