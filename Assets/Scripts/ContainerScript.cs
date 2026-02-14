/*
* Author: Hazel
* Date: 2026-02-06
* Description: This script manages the container in the game. It handles the interaction when a flower is selected to be stocked in the container. When a flower is selected, it retrieves the flower prefab from the FlowerInBoxScript and instantiates it in each of the container sockets at a specific position and rotation. The script ensures that the correct flower is stocked in the container based on the player's selection.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ContainerScript : MonoBehaviour
{
    public static ContainerScript Instance; // Singleton instance for easy access from other scripts

    FlowerInBoxScript flowerInBoxScript; // Reference to the FlowerInBoxScript to manage the flowers inside the box

    public GameObject flowerToStock; // Reference to the flower prefab that will be stocked in the container, set when a flower is selected in the FlowerInBoxScript

    [SerializeField]
    List<GameObject> containerSockets; // List of GameObjects representing the sockets in the container where the flowers will be instantiated, set in the Unity Editor

    // This method is called when a flower is selected in the FlowerInBoxScript. It retrieves the flower prefab from the FlowerInBoxScript and instantiates it in each of the container sockets at a specific position and rotation. The instantiated flowers are parented to the respective sockets to ensure they move together with the container.
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
