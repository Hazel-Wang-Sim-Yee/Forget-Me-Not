/*
* Author: Hazel
* Date: 2026-02-06
* Description: Automatically spawns an item when a collision is detected. The item to be spawned and the spawn point can be set in the Unity Editor.
*/
using UnityEngine;

public class ItemSpawnerScript : MonoBehaviour
{
    [SerializeField]
    public GameObject itemPrefab;// The item to be spawned, set in the Unity Editor
    [SerializeField]
    public Transform spawnPoint;// The point where the item will be spawned, set in the Unity Editor

    // This method is called when a collision is detected. It calls the SpawnItem method to create a new item at the specified spawn point.
    void OnCollisionEnter(Collision collision)
    {
        SpawnItem();   
    }

    // This method instantiates the itemPrefab at the spawnPoint's position and rotation. It is called by the OnCollisionEnter method when a collision is detected.
    public void SpawnItem()
    {
        Debug.Log("Spawning item...");  
        Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
