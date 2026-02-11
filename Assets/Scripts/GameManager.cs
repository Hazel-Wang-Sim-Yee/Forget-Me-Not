using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private DatabaseReference dbRef;

    [Header("Twist Settings")]
    public GameObject nursePrefab;
    public bool allRecalled = false;
    public bool twist_unlocked = false;

    [Header("Player Stats")]
    public int happyCustomers = 0;
    public int totalCustomers = 2;
    public int currentDay = 1;
    public float currentEarnings = 0;
    public int currentCustomerIndex = 0;

    [Header("Prefabs & References")]
    public GameObject[] NPCPrefabs;
    public List<GameObject> dailyNPCs = new List<GameObject>();
    private List<GameObject> availableNPCs = new List<GameObject>();

    [Header("Scene References")]
    public Transform spawnLocation;
    public Transform customerDestination;
    public bool isDayActive = true;
    private CashRegisterBehaviour cashRegisterBehaviour;
    public GameObject StartNextDayCanvas;
    public TMP_Text StartNextDayCanvasText;

    private GameObject currentActiveNPC;
    public Transform xrOrigin;
    private DayResetScript dayResetScript;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void Start()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null) LoadGameData();
        else if (currentDay == 1) { InitializeFirstTimeSetup(); StartNextDay(); }
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

    void LoadGameData()
    {
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        dbRef.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.HasChild("current_day"))
                    currentDay = int.Parse(snapshot.Child("current_day").Value.ToString());
                if (snapshot.HasChild("twist_unlocked"))
                    twist_unlocked = (bool)snapshot.Child("twist_unlocked").Value;

                CheckForTwistTransition();
                InitializeFirstTimeSetup();
                StartNextDay();
            }
            else { InitializeFirstTimeSetup(); StartNextDay(); }
        });
    }

    public void CheckForTwistTransition()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null) return;
        string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        dbRef.Child("users").Child(userId).Child("recalled_npcs").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                bool complete = true;
                foreach (var child in task.Result.Children)
                {
                    if (child.Value is bool b && !b) { complete = false; break; }
                }

                allRecalled = complete;

                if (allRecalled)
                {
                    twist_unlocked = true;
                    dbRef.Child("users").Child(userId).Child("twist_unlocked").SetValueAsync(true);
                    Debug.Log("Firebase Sync: Twist Unlocked mid-game!");
                }
            }
        });
    }

    void InitializeFirstTimeSetup()
    {
        cashRegisterBehaviour = FindFirstObjectByType<CashRegisterBehaviour>();
        dayResetScript = FindFirstObjectByType<DayResetScript>();
    }

    public void TriggerNurseTwist()
    {
        BoxBehaviourScript[] allBoxes = FindObjectsOfType<BoxBehaviourScript>();
        foreach (BoxBehaviourScript box in allBoxes) Destroy(box.gameObject);

        NPCBehaviour[] allNPCs = FindObjectsOfType<NPCBehaviour>();
        foreach (NPCBehaviour npc in allNPCs) Destroy(npc.gameObject);

        isDayActive = true;
        currentCustomerIndex = 0;
        totalCustomers = 1;

        if (nursePrefab != null)
        {
            GameObject nurse = Instantiate(nursePrefab, spawnLocation.position, Quaternion.identity);

            StartCoroutine(SendNurseToWaypoint(nurse));

            NPCDialogueFirebase dialogueScript = nurse.GetComponent<NPCDialogueFirebase>();
            if (dialogueScript != null) dialogueScript.npcID = "nurse";

            currentActiveNPC = nurse;
        }
    }

    IEnumerator SendNurseToWaypoint(GameObject nurse)
    {
        yield return new WaitForEndOfFrame();
        NPCBehaviour npcScript = nurse.GetComponent<NPCBehaviour>();
        if (npcScript != null)
        {
            npcScript.SetTarget(customerDestination);
            Debug.Log("Nurse is moving to waypoint.");
        }
    }

    void SpawnNextCustomer()
    {
        if (availableNPCs.Count == 0) return;
        GameObject selectedNPC = availableNPCs[0];
        currentActiveNPC = Instantiate(selectedNPC, spawnLocation.position, Quaternion.identity);
        availableNPCs.RemoveAt(0);

        NPCBehaviour npcScript = currentActiveNPC.GetComponent<NPCBehaviour>();
        if (npcScript != null) npcScript.SetTarget(customerDestination);
    }

    void EndDay()
    {
        isDayActive = false;
        currentEarnings += cashRegisterBehaviour.amountInRegister;
        currentDay++;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            dbRef.Child("users").Child(userId).Child("current_day").SetValueAsync(currentDay);
        }

        CheckForTwistTransition();

        happyCustomers = 0;
        currentCustomerIndex = 0;
        availableNPCs.Clear();
        dailyNPCs.Clear();
        cashRegisterBehaviour.amountInRegister = 0f;
        cashRegisterBehaviour.UpdateAmountInRegister();

        StartCoroutine(DecideNextStepAfterFirebase());
    }

    IEnumerator DecideNextStepAfterFirebase()
    {

        yield return new WaitForSeconds(0.5f);

        if (currentDay >= 6 && twist_unlocked)
        {
            Debug.Log("Day 6 + Twist: Skipping UI, spawning Nurse.");
            if (StartNextDayCanvas != null) StartNextDayCanvas.SetActive(false);
            isDayActive = true;
            TriggerNurseTwist();
        }
        else
        {
            StartNextDayCanvasText.text = "Day " + (currentDay - 1) + " complete!" +
                                         "\nEarnings: $" + currentEarnings.ToString("F2") +
                                         "\nTap to go to town.";
            if (StartNextDayCanvas != null) StartNextDayCanvas.SetActive(true);
        }
    }

    void StartNextDay()
    {
        isDayActive = true;

        if (currentDay >= 6 && twist_unlocked)
        {
            TriggerNurseTwist();
            return;
        }

        totalCustomers = currentDay + 1;
        dailyNPCs.Clear();
        List<GameObject> tempPool = new List<GameObject>(NPCPrefabs);

        for (int i = 0; i < totalCustomers; i++)
        {
            if (tempPool.Count == 0) tempPool.AddRange(NPCPrefabs);
            int randomIndex = Random.Range(0, tempPool.Count);
            dailyNPCs.Add(tempPool[randomIndex]);
            tempPool.RemoveAt(randomIndex);
        }
        availableNPCs = new List<GameObject>(dailyNPCs);
        if (dayResetScript != null) dayResetScript.TriggerDayReset();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameScene")
        {
            InitializeFirstTimeSetup();
            foreach (GameObject rootObj in scene.GetRootGameObjects())
            {
                Transform[] allChildren = rootObj.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allChildren)
                {
                    if (t.name == "NPCSpawnPoint") spawnLocation = t;
                    if (t.name == "NPCStopWaypoint") customerDestination = t;
                    if (t.name == "NextDayCanvas")
                    {
                        StartNextDayCanvas = t.gameObject;
                        StartNextDayCanvas.SetActive(false);
                        StartNextDayCanvasText = t.GetComponentInChildren<TMP_Text>(true);
                    }
                }
            }
            if (currentDay > 1) StartNextDay();
        }
    }
}