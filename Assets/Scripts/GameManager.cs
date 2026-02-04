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
}