using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int happyCustomers = 0;
    public int totalCustomers = 2;
    public int currentDay = 1;
    public float currentEarnings = 0;
    public int currentCustomerIndex = 0;
    public GameObject NPCPrefab;
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
<<<<<<< Updated upstream
=======
        dayResetScript = FindFirstObjectByType<DayResetScript>();
        // Prepare our first pool of unique NPCs
        ResetAvailableNPCs();

        if (currentDay == 1)
        {
            StartNextDay();
        }
    }

    // Creates a list of numbers [0, 1, 2, 3, 4] representing your NPC prefabs
    void ResetAvailableNPCs()
    {
        availableIndices.Clear();
        for (int i = 0; i < npcPrefabs.Count; i++)
        {
            availableIndices.Add(i);
        }
>>>>>>> Stashed changes
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
        Debug.Log("Spawning customer " + currentCustomerIndex);
        
        currentActiveNPC = Instantiate(NPCPrefab, spawnLocation.position, Quaternion.identity);

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