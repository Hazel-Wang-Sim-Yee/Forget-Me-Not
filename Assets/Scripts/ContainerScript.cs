using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ContainerScript : MonoBehaviour
{
    public static ContainerScript Instance;

    FlowerInBoxScript flowerInBoxScript;

    public GameObject flowerToStock;

    [SerializeField]
    List<GameObject> containerSockets;

    public void OnSelectEnter()
    {
        flowerInBoxScript = FindFirstObjectByType<FlowerInBoxScript>();
        flowerInBoxScript.StockFlowersInBox();
        flowerToStock = flowerInBoxScript.basePrefab;
        foreach (var socket in containerSockets)
        {
            Debug.Log("Flower instantiated in socket");
            Instantiate(flowerToStock, new Vector3(socket.transform.position.x, socket.transform.position.y + 0.17f, socket.transform.position.z - 0.09f), socket.transform.rotation, socket.transform);
        }
    }
}
