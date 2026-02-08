using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowerInBoxScript : MonoBehaviour
{
    public static FlowerInBoxScript Instance;

    ContainerScript containerScript;
    [SerializeField]
    public GameObject box;

    [SerializeField]
    public GameObject flowerGroup;

    [SerializeField]
    public GameObject allFlowersInBox;

    [SerializeField]
    List<GameObject> otherFlowersInGroup;

    [SerializeField]
    public GameObject basePrefab;

    public void StockFlowersInBox()
    {
        containerScript = FindFirstObjectByType<ContainerScript>();
        containerScript.flowerToStock = basePrefab;
        Debug.Log("Flower stocked in box");
        if (allFlowersInBox.transform.childCount == 1)
        {
            Destroy(box);
            foreach (GameObject flower in otherFlowersInGroup)
            {
                Destroy(flower);
            }
            containerScript.socket.enabled = false;
            Destroy(flowerGroup);
        }
        else
        {
            foreach (GameObject flower in otherFlowersInGroup)
            {
                Destroy(flower);
            }
            Destroy(flowerGroup);
            containerScript.socket.enabled = false;
        }
    }
}
