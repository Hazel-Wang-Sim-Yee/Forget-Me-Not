using UnityEngine;
using TMPro;
using System.Collections.Generic; // Required for List

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int happyCustomers = 0;
    public int totalCustomers = 2;
    public int currentDay = 1;
    public float currentEarnings = 0;
    public int currentCustomerIndex = 0;

    public GameObject[] NPCPrefabs; 
    private List<GameObject> availableNPCs = new List<GameObject>();

    public Transform spawnLocation;
    public Transform customerDestination;
    public bool isDayActive = true;
    private CashRegisterBehaviour cashRegisterBehaviour;
    public GameObject StartNextDayCanvas;

    private GameObject currentActiveNPC; 

    public Transform xrOrigin;

    DayResetScript dayResetScript;

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
        cashRegisterBehaviour = FindFirstObjectByType<CashRegisterBehaviour>();
        dayResetScript = FindFirstObjectByType<DayResetScript>();

        if (currentDay == 1)
        {
            StartNextDay();
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
        if (NPCPrefabs == null || NPCPrefabs.Length == 0)
        {
            Debug.LogError("No NPC prefabs assigned!");
            return;
        }

        if (availableNPCs.Count == 0)
        {
            availableNPCs.AddRange(NPCPrefabs);
        }

        Debug.Log("Spawning customer " + currentCustomerIndex);
        
        int randomIndex = Random.Range(0, availableNPCs.Count);
        GameObject selectedNPC = availableNPCs[randomIndex];

        currentActiveNPC = Instantiate(selectedNPC, spawnLocation.position, Quaternion.identity);

        availableNPCs.RemoveAt(randomIndex);

        NPCBehaviour npcScript = currentActiveNPC.GetComponent<NPCBehaviour>();
        if (npcScript != null)
        {
            npcScript.SetTarget(customerDestination);
        }
    }

    void EndDay()
    {
        isDayActive = false;
        currentEarnings += cashRegisterBehaviour.amountInRegister;
        Debug.Log("Day " + currentDay + " ended.");
        
        currentDay++;
        happyCustomers = 0;
        currentCustomerIndex = 0;
        
        availableNPCs.Clear();

        cashRegisterBehaviour.amountInRegister = 0f;
        cashRegisterBehaviour.UpdateAmountInRegister();
        StartNextDayCanvas.SetActive(true);
    }

    void StartNextDay()
    {
        isDayActive = true;
        dayResetScript = FindFirstObjectByType<DayResetScript>();
        dayResetScript.DayReset();
    }
}