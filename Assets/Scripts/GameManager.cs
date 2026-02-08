using UnityEngine;
using TMPro;
using System.Collections.Generic; // Added for Lists

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Stats")]
    public int happyCustomers = 0;
    public int totalCustomers = 2;
    public int currentDay = 1;
    public float currentEarnings = 0;
    public int currentCustomerIndex = 0;
    public bool isDayActive = true;

    [Header("NPC Spawning")]
    // Drag your Uncle Tan, Siti, etc. prefabs here in the Inspector
    public List<GameObject> npcPrefabs; 
    private List<int> availableIndices = new List<int>();

    [Header("Scene References")]
    public Transform spawnLocation;
    public Transform customerDestination;
    public GameObject StartNextDayCanvas;
    public Transform xrOrigin;

    private GameObject currentActiveNPC; 
    private CashRegisterBehaviour cashRegisterBehaviour;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        totalCustomers = 2;
        cashRegisterBehaviour = FindFirstObjectByType<CashRegisterBehaviour>();
        
        // Prepare our first pool of unique NPCs
        ResetAvailableNPCs();
    }

    // Creates a list of numbers [0, 1, 2, 3, 4] representing your NPC prefabs
    void ResetAvailableNPCs()
    {
        availableIndices.Clear();
        for (int i = 0; i < npcPrefabs.Count; i++)
        {
            availableIndices.Add(i);
        }
    }

    void Update()
    {
        if (currentActiveNPC == null)
        {
            if (currentCustomerIndex < totalCustomers && isDayActive)
            {
                currentCustomerIndex++;
                SpawnNextCustomer();
            }
            else if (isDayActive && currentCustomerIndex >= totalCustomers)
            {
                EndDay();
            }
        }
    }

    void SpawnNextCustomer()
    {
        if (npcPrefabs.Count == 0)
        {
            Debug.LogError("No NPC Prefabs assigned in GameManager!");
            return;
        }

        // If we've cycled through everyone, reset the pool so we can see them again
        if (availableIndices.Count == 0)
        {
            ResetAvailableNPCs();
        }

        // Pick a random NPC from the available pool
        int randomIndex = Random.Range(0, availableIndices.Count);
        int npcToSpawnIndex = availableIndices[randomIndex];

        // Remove them from the pool so they don't spawn again until the pool resets
        availableIndices.RemoveAt(randomIndex);

        Debug.Log("Spawning customer " + currentCustomerIndex + ": " + npcPrefabs[npcToSpawnIndex].name);
        
        currentActiveNPC = Instantiate(npcPrefabs[npcToSpawnIndex], spawnLocation.position, Quaternion.identity);

        NPCBehaviour npcScript = currentActiveNPC.GetComponent<NPCBehaviour>();
        if (npcScript != null)
        {
            npcScript.SetTarget(customerDestination);
        }
    }

    void EndDay()
    {
        isDayActive = false;
        if (cashRegisterBehaviour != null)
        {
            currentEarnings += cashRegisterBehaviour.amountInRegister;
            cashRegisterBehaviour.amountInRegister = 0f;
            cashRegisterBehaviour.UpdateAmountInRegister();
        }
        
        Debug.Log("Day " + currentDay + " ended.");
        
        currentDay++;
        happyCustomers = 0;
        currentCustomerIndex = 0;
        StartNextDayCanvas.SetActive(true);
    }
}