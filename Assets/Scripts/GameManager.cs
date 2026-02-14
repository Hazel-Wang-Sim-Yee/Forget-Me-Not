/*
* Author: Hazel
* Date: 2026-02-06
* Description: This script manages the overall game flow and state in the Forget-Me-Not game. It handles the progression of days, spawning of customers, tracking of player stats such as happy customers and earnings, and the unlocking of a twist in the storyline. The script also interacts with Firebase to save and load game data, ensuring that player progress is maintained across sessions. Additionally, it manages the transition to a special twist involving a nurse character when certain conditions are met.
*/
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
    public static GameManager Instance; // Singleton instance for easy access from other scripts
    private DatabaseReference dbRef; // Reference to the Firebase Realtime Database for saving and loading game data

    [Header("Twist Settings")]
    public GameObject nursePrefab; // Reference to the nurse prefab that will be spawned during the twist, set in the Unity Editor
    public bool allRecalled = false; // Flag to indicate whether all NPCs have been recalled by the player, initialized to false
    public bool twist_unlocked = false; // Flag to indicate whether the twist has been unlocked, initialized to false

    [Header("Player Stats")]
    public int happyCustomers = 0; // The number of happy customers the player has served, initialized to 0
    public int totalCustomers = 2; // The total number of customers for the current day, initialized to 2 for the first day
    public int currentDay = 1; // The current day in the game, initialized to 1
    public float currentEarnings = 0; // The total earnings the player has accumulated, initialized to 0
    public int currentCustomerIndex = 0; // The index of the current customer being served, initialized to 0

    [Header("Prefabs & References")]
    public GameObject[] NPCPrefabs; // Array of NPC prefabs that can be spawned as customers, set in the Unity Editor
    public List<GameObject> dailyNPCs = new List<GameObject>(); // List of NPCs that are present for the current day, populated at the start of each day based on the total number of customers and the available NPC prefabs
    private List<GameObject> availableNPCs = new List<GameObject>(); // List of NPCs that are still waiting to be spawned for the current day, initialized as a copy of dailyNPCs at the start of each day and updated as customers are spawned

    [Header("Scene References")]
    public Transform spawnLocation; // Reference to the Transform component representing the spawn location for customers, set in the Unity Editor
    public Transform customerDestination; // Reference to the Transform component representing the destination point for customers to move towards, set in the Unity Editor
    public bool isDayActive = true; // Flag to indicate whether the day is currently active, initialized to true at the start of the game and set to false when the day ends
    private CashRegisterBehaviour cashRegisterBehaviour; // Reference to the CashRegisterBehaviour script to manage interactions with the cash register and track earnings
    public GameObject StartNextDayCanvas; // Reference to the GameObject representing the canvas that appears at the end of each day to show stats and prompt the player to start the next day, set in the Unity Editor
    public TMP_Text StartNextDayCanvasText; // Reference to the TextMeshProUGUI component that displays the stats and message on the StartNextDayCanvas, set in the Unity Editor

    private GameObject currentActiveNPC; // Reference to the currently active NPC (customer) in the scene, initialized to null and updated as customers are spawned and served
    public Transform xrOrigin; // Reference to the Transform component representing the XR Origin for VR interactions, set in the Unity Editor
    private DayResetScript dayResetScript; // Reference to the DayResetScript to manage the day reset process and spawning of boxes and flowers at the start of each day

    // This method is called when the script instance is being loaded. It assigns the current instance of the GameManager to the static Instance variable, allowing other scripts to easily access it. It also initializes the Firebase database reference for saving and loading game data.
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

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; } // Subscribe to the sceneLoaded event to initialize references and start the game flow when the main game scene is loaded
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; } // Unsubscribe from the sceneLoaded event when the script is disabled to prevent memory leaks and unintended behavior

    // This method is called when the scene is loaded. It checks if the loaded scene is the main game scene, and if so, it initializes references to important GameObjects and components in the scene, such as the spawn location for customers, the customer destination point, and the canvas for starting the next day. It also checks if there is saved game data for the current user in Firebase, and if so, it loads that data to restore the player's progress. If there is no saved data, it initializes the first-time setup and starts the first day of the game.
    void Start()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null) LoadGameData();
        else if (currentDay == 1) { InitializeFirstTimeSetup(); StartNextDay(); }
    }

    // This method is called every frame to check the state of the current active NPC (customer). If there is no active NPC, it checks if there are more customers to spawn for the day. If there are, it increments the current customer index and spawns the next customer. If there are no more customers to spawn and the day is still active, it ends the day by calling the EndDay() method, which handles the transition to the next day and updates player stats accordingly.
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

    // This method loads the game data for the current user from Firebase. It retrieves the current day and whether the twist has been unlocked from the database, and then checks if the conditions for unlocking the twist have been met. If there is no saved data for the user, it initializes the first-time setup and starts the first day of the game.
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

    // This method checks if the conditions for unlocking the twist have been met. It retrieves the list of recalled NPCs from the database for the current user and checks if all of them have been recalled. If all NPCs have been recalled, it sets the twist_unlocked flag to true and updates the database accordingly, allowing the player to access the twist content in the game.
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

    // This method initializes references to important GameObjects and components in the scene that are needed for the game flow, such as the CashRegisterBehaviour for managing the cash register interactions and the DayResetScript for handling the day reset process. This method is called during the scene loading process to ensure that all necessary references are set up before the game starts.
    void InitializeFirstTimeSetup()
    {
        cashRegisterBehaviour = FindFirstObjectByType<CashRegisterBehaviour>();
        dayResetScript = FindFirstObjectByType<DayResetScript>();
    }

    // This method is called to trigger the special twist involving the nurse character. It first destroys all existing boxes and NPCs in the scene to clear the way for the new content. It then sets the day as active, resets the customer index and total customers to 1 for the twist scenario, and instantiates the nurse prefab at the spawn location. The nurse is then sent to a specific waypoint using a coroutine, and its dialogue script is set up with the appropriate NPC ID for dialogue management during interactions.
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

    // This coroutine is responsible for sending the nurse character to a specific waypoint after it has been instantiated during the twist scenario. It waits for the end of the frame to ensure that the nurse has been fully initialized, then retrieves the NPCBehaviour script from the nurse and sets its target to the customer destination point, allowing the nurse to move towards that location in the scene.
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

    // This method is responsible for spawning the next customer NPC in the scene. It checks if there are any available NPCs left to spawn for the current day, and if so, it instantiates the next NPC from the availableNPCs list at the spawn location. The instantiated NPC is then set as the current active NPC, and its target is set to the customer destination point to ensure that it moves towards that location in the scene.
    void SpawnNextCustomer()
    {
        if (availableNPCs.Count == 0) return;
        GameObject selectedNPC = availableNPCs[0];
        currentActiveNPC = Instantiate(selectedNPC, spawnLocation.position, Quaternion.identity);
        availableNPCs.RemoveAt(0);

        NPCBehaviour npcScript = currentActiveNPC.GetComponent<NPCBehaviour>();
        if (npcScript != null) npcScript.SetTarget(customerDestination);
    }

    // This method is called to end the current day in the game. It sets the day as inactive, updates the player's earnings by adding the amount in the cash register, increments the current day count, and saves the updated day count to Firebase. It then checks if the conditions for transitioning to the twist scenario have been met. Finally, it resets various stats and lists for the next day and starts a coroutine to decide whether to show the next day canvas or transition directly to the twist based on the current day and twist unlock status.
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

    // This coroutine is responsible for deciding the next step after the day has ended and the game data has been saved to Firebase. It waits for a short duration to ensure that the Firebase operations have completed, then checks if the current day is greater than or equal to 6 and if the twist has been unlocked. If both conditions are met, it skips showing the next day canvas and directly triggers the nurse twist scenario. If not, it updates the text on the next day canvas with the stats from the completed day and prompts the player to start the next day, then shows the canvas.
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

    // This method is called to start the next day in the game. It sets the day as active, calculates the total number of customers for the new day based on the current day count, and populates the dailyNPCs list with a random selection of NPC prefabs from the available pool. It then creates a copy of the dailyNPCs list to track which NPCs are still waiting to be spawned, and triggers the day reset process to spawn the necessary boxes and flowers for the new day.
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

    // This method is called when the scene is loaded. It checks if the loaded scene is the main game scene, and if so, it initializes references to important GameObjects and components in the scene, such as the spawn location for customers, the customer destination point, and the canvas for starting the next day. It also checks if there is saved game data for the current user in Firebase, and if so, it loads that data to restore the player's progress. If there is no saved data, it initializes the first-time setup and starts the first day of the game.
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