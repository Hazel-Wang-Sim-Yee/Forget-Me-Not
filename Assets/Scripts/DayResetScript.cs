using UnityEngine;

public class DayResetScript : MonoBehaviour
{
    public static DayResetScript Instance;

    BoxBehaviourScript boxBehaviourScript;
    GameManager gameManager;

    Transform SuppliesSpawnPoint;

    void Start()
    {
        gameManager.totalCustomers = gameManager.currentDay + 1;

        SuppliesSpawnPoint = GameObject.Find("SuppliesSpawnPoint").transform;
        for (int i = 0; i < gameManager.currentDay; i++)
        {
            Instantiate(Resources.Load("SupplyBox"), SuppliesSpawnPoint.position, SuppliesSpawnPoint.rotation);
            if (i % 4 == 1)
            {
                boxBehaviourScript.UpdateBoxLabel("Roses");
            }
            else if (i % 4 == 2)
            {
                boxBehaviourScript.UpdateBoxLabel("Tulips");
            }
            else if (i % 4 == 3)
            {
                boxBehaviourScript.UpdateBoxLabel("Daisies");
            }
            else
            {
                boxBehaviourScript.UpdateBoxLabel("Sunflowers");
            }
        }
    }


}
