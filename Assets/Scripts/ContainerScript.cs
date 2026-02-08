using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ContainerScript : MonoBehaviour
{
    public static ContainerScript Instance;

    FlowerInBoxScript flowerInBoxScript;

    public GameObject flowerToStock;

    [SerializeField]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    public void OnSelectEnter()
    {
        flowerInBoxScript = FindFirstObjectByType<FlowerInBoxScript>();
        flowerInBoxScript.StockFlowersInBox();
        Instantiate(flowerToStock, transform.position, transform.rotation, transform);
        Instantiate(flowerToStock, new Vector3(transform.position.x + 0.1f, transform.position.y +0.1f, transform.position.z + 0.1f), transform.rotation, transform);
        Instantiate(flowerToStock, new Vector3(transform.position.x - 0.1f, transform.position.y, transform.position.z - 0.1f), transform.rotation, transform);
        Instantiate(flowerToStock, new Vector3(transform.position.x - 0.1f, transform.position.y + 0.1f, transform.position.z - 0.1f), transform.rotation, transform);
        Instantiate(flowerToStock, new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z), transform.rotation, transform);
        Instantiate(flowerToStock, new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z + 0.1f), transform.rotation, transform);
    }

    void Start()
    {
        socket.enabled = true;
    }
}
