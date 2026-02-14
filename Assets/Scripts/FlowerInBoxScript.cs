/*
* Author: Hazel
* Date: 2026-02-08
* Description: Manages the flowers inside the box in the game. It handles the interaction when a flower is selected to be stocked in the box. When a flower is selected, it retrieves the flower prefab from the ContainerScript and instantiates it in the box at a specific position and rotation. The script also manages the state of the box, allowing it to be emptied on the next stock if necessary.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowerInBoxScript : MonoBehaviour
{
    public static FlowerInBoxScript Instance; // Singleton instance for easy access from other scripts

    ContainerScript containerScript; // Reference to the ContainerScript to manage the interaction when a flower is selected to be stocked in the box
    [SerializeField]
    public GameObject box; // Reference to the box GameObject that contains the flowers, set in the Unity Editor

    [SerializeField]
    public GameObject flowerGroup; // Reference to the GameObject representing the group of flowers inside the box, set in the Unity Editor

    [SerializeField]
    public GameObject allFlowersInBox; // Reference to the parent GameObject that contains all the flowers inside the box, set in the Unity Editor

    [SerializeField]
    List<GameObject> otherFlowersInGroup; // List of GameObjects representing the other flowers in the group that are not the base prefab, set in the Unity Editor

    [SerializeField]
    public GameObject basePrefab; // Reference to the base flower prefab that will be stocked in the box, set in the Unity Editor

    public bool emptyNextStock = false; // Flag to indicate whether the box should be emptied on the next stock or not, initialized to false

    // This method is called when a flower is selected to be stocked in the box. It retrieves the flower prefab from the ContainerScript and instantiates it in the box at a specific position and rotation. If the emptyNextStock flag is true, it means that the box should be emptied on the next stock, so it destroys the current box and all the flowers inside it before instantiating the new flower. If the emptyNextStock flag is false, it simply destroys the other flowers in the group and the flower group itself before instantiating the new flower, allowing for a new flower to be stocked without emptying the entire box.
    public void StockFlowersInBox()
    {
        containerScript = FindFirstObjectByType<ContainerScript>();
        containerScript.flowerToStock = basePrefab;
        Debug.Log("Flower stocked in box");
        if (emptyNextStock)
        {
            Debug.Log("Flower box emptied");
            Destroy(box);
            foreach (GameObject flower in otherFlowersInGroup)
            {
                Destroy(flower);
            }
            Destroy(flowerGroup);
            Destroy(allFlowersInBox);
        }
        else
        {
            foreach (GameObject flower in otherFlowersInGroup)
            {
                Destroy(flower);
            }
            Destroy(flowerGroup);
            emptyNextStock = true;
        }
    }
}
