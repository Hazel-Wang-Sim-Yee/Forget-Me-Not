/*
* Author: Jeffrey
* Date: 2026-02-11
* Description: Automatically spawns NPCs in the exterior of the game world. The NPCs to be spawned and their spawn points can be set in the Unity Editor.
*/
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 

public class ExteriorNPCManager : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject[] npcPrefabs; // Array of NPC prefabs that can be spawned, set in the Unity Editor
    public int totalNPCsToSpawn = 5; // Total number of NPCs to spawn, set in the Unity Editor

    [Header("Spawn Locations")]
    public Transform[] spawnPoints; // Array of Transform components representing the spawn points for the NPCs, set in the Unity Editor

    private List<GameObject> availableNPCs = new List<GameObject>(); // List to keep track of available NPC prefabs for spawning, initialized with the npcPrefabs array

    // This method is called when the script instance is being loaded. It initializes the availableNPCs list with the NPC prefabs and calls the method to spawn and initialize the NPCs in the exterior.
    void Start()
    {
        spawnAndInitializeNPCs();
    }

    // This method handles the spawning and initialization of NPCs in the exterior. It checks if there are valid NPC prefabs and spawn points, then randomly selects an NPC prefab and a spawn point for each NPC to be spawned. The selected NPC is instantiated at the chosen spawn point, and the NPC is cleaned up for exterior use by disabling its dialogue canvas and removing any XRSocketInteractors. Finally, if the NPC has an NPCBehaviour component, it sets it to roam around the exterior.   
    void spawnAndInitializeNPCs()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0 || spawnPoints == null || spawnPoints.Length == 0) return;

        for (int i = 0; i < totalNPCsToSpawn; i++)
        {
            if (availableNPCs.Count == 0) availableNPCs.AddRange(npcPrefabs);

            int npcIndex = Random.Range(0, availableNPCs.Count);
            int spawnIndex = Random.Range(0, spawnPoints.Length);

            GameObject selectedPrefab = availableNPCs[npcIndex];
            Transform selectedSpawn = spawnPoints[spawnIndex];

            GameObject spawnedNPC = Instantiate(selectedPrefab, selectedSpawn.position, selectedSpawn.rotation);
            availableNPCs.RemoveAt(npcIndex);

            CleanNPCForExterior(spawnedNPC);

            NPCBehaviour behavior = spawnedNPC.GetComponent<NPCBehaviour>();
            if (behavior != null)
            {
                behavior.isRoaming = true; 
            }
        }
    }

    // This method cleans up the NPC for exterior use by disabling its dialogue canvas and removing any XRSocketInteractors. It takes a GameObject representing the NPC as a parameter and performs the necessary cleanup operations to ensure that the NPC is suitable for roaming in the exterior environment without any interactive elements that are meant for indoor use.
    private void CleanNPCForExterior(GameObject npc)
    {
        Transform canvasTrans = npc.transform.Find("DialogueCanvas");
        if (canvasTrans != null) 
        {
            canvasTrans.gameObject.SetActive(false);
        }

        XRSocketInteractor[] sockets = npc.GetComponentsInChildren<XRSocketInteractor>();
        foreach (var socket in sockets)
        {
            Destroy(socket.gameObject);
        }
        
    }
}