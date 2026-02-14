/*
* Author: Hazel
* Date: 2026-02-08
* Description: This script manages the behavior of the box in the game. It handles opening the box flaps, updating the box label, and spawning the appropriate flowers inside the box based on the type of flowers selected by the player. The script also ensures that the flowers are only spawned once when the box is opened for the first time. The flower types are determined by comparing the selected flower with predefined flower GameObjects, and the corresponding flower group is instantiated inside the box at a specific position and rotation.
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoxBehaviourScript : MonoBehaviour
{
    public static BoxBehaviourScript Instance; // Singleton instance for easy access from other scripts
    FlowerInBoxScript flowerInBoxScript; // Reference to the FlowerInBoxScript to manage the flowers inside the box
    public TextMeshProUGUI boxLabel; // Reference to the TextMeshProUGUI component that displays the box label
    public GameObject flowersInside; // Reference to the GameObject representing the flowers inside the box
    [SerializeField]
    GameObject leftBoxFlap; // Reference to the left box flap GameObject
    [SerializeField]
    GameObject rightBoxFlap; // Reference to the right box flap GameObject
    [SerializeField]
    GameObject boxLabelObject; // Reference to the GameObject that contains the box label
    [SerializeField]
    public GameObject CarnationsFlowers; // The prefab for the carnations flowers
    [SerializeField]
    public GameObject TulipsFlowers; // The prefab for the tulips flowers
    [SerializeField]
    public GameObject DaisiesFlowers; // The prefab for the daisies flowers
    [SerializeField]
    public GameObject LiliesFlowers; // The prefab for the lilies flowers
    bool hasFlowers = false; // Flag to indicate whether the box already has flowers inside or not

    GameObject flowersInsideGrp; // Reference to the instantiated group of flowers inside the box

    // Updates the box label text with the provided labelText parameter. This method can be called by other scripts to change the label displayed on the box.
    public void UpdateBoxLabel(string labelText)
    {
        boxLabel.text = labelText;
    }

    // This method is called to open the box flaps. It first hides the box label, then rotates the left and right box flaps to simulate opening. If the box does not already have flowers inside, it sets the hasFlowers flag to true and calls the FlowerTypeInBox method to spawn the appropriate flowers based on the flowersInside reference.
    public void OpenBoxFlaps()
    {
        boxLabelObject.SetActive(false);
        leftBoxFlap.transform.Rotate(Vector3.forward, -240);
        rightBoxFlap.transform.Rotate(Vector3.forward, 240);
        if (!hasFlowers)
        {
            hasFlowers = true;
            FlowerTypeInBox(flowersInside);
        }
    }

    // This method spawns the appropriate flowers inside the box based on the type of flowers selected by the player. It checks the flowersInside reference against predefined flower GameObjects (CarnationsFlowers, TulipsFlowers, DaisiesFlowers, LiliesFlowers) and instantiates the corresponding flower group at a specific position and rotation inside the box. It also gets a reference to the FlowerInBoxScript component of the instantiated flower group and assigns the current box game object to it for further management of the flowers inside the box.
    public void FlowerTypeInBox(GameObject flowerType)
    {
        if (flowerType == CarnationsFlowers)
        {
            flowersInsideGrp = Instantiate(CarnationsFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == TulipsFlowers)
        {
            flowersInsideGrp = Instantiate(TulipsFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == DaisiesFlowers)
        {
            flowersInsideGrp = Instantiate(DaisiesFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == LiliesFlowers)
        {
            flowersInsideGrp = Instantiate(LiliesFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }

        flowerInBoxScript = flowersInsideGrp.GetComponent<FlowerInBoxScript>();
        flowerInBoxScript.box = this.gameObject;
    }
}
