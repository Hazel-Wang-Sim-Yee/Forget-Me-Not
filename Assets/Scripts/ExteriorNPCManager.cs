using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactors; 

public class ExteriorNPCManager : MonoBehaviour
{
    [Header("NPC Settings")]
    public GameObject[] npcPrefabs; 
    public int totalNPCsToSpawn = 5;

    [Header("Spawn Locations")]
    public Transform[] spawnPoints; 

    private List<GameObject> availableNPCs = new List<GameObject>();

    void Start()
    {
        spawnAndInitializeNPCs();
    }

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