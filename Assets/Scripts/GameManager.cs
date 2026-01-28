using UnityEngine;
using UnityEngine.UI;
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
    public bool isDayActive = true;
    private CashRegisterBehaviour cashRegisterBehaviour;
    public GameObject StartNextDayCanvas;

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
        cashRegisterBehaviour = FindObjectOfType<CashRegisterBehaviour>();
    }

    void Update()
    {
        GameObject NPC = GameObject.FindWithTag("NPC");
        if (NPC == null)
        {
            if (currentCustomerIndex + 1 <= totalCustomers && isDayActive)
            {
                currentCustomerIndex++;
                // Spawn next customer logic here
                SpawnNextCustomer();
            }
            else if (isDayActive)
            {
                isDayActive = false;
                currentEarnings += cashRegisterBehaviour.amountInRegister;
                Debug.Log("Day " + currentDay + " ended with " + happyCustomers + " happy customers and earnings of $" + currentEarnings);
                // Reset for next day
                currentDay++;
                happyCustomers = 0;
                currentCustomerIndex = 0;
                cashRegisterBehaviour.amountInRegister = 0f;
                cashRegisterBehaviour.UpdateAmountInRegister();
                StartNextDayCanvas.SetActive(true);

            }
        }
    }

    void SpawnNextCustomer()
    {
        // Logic to spawn the next customer NPC
        Debug.Log("Spawning customer " + (currentCustomerIndex));
        // Instantiate NPC prefab at designated location
        Instantiate(NPCPrefab, spawnLocation.position, Quaternion.identity);
    }
}
