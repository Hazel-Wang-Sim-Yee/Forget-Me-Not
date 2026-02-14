/*
* Author: Hazel
* Date: 2026-02-08
* Description: This script manages the behavior of the start next day button in the game. It handles enabling and disabling the canvas that displays the next day button, and transitioning to the exterior scene when the button is pressed. The script also includes logic for handling special conditions such as triggering a nurse twist if all recalled conditions are met.
*/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartNextDayScript : MonoBehaviour
{
    public static StartNextDayScript Instance; // Singleton instance for easy access from other scripts
    public GameObject StartNextDayCanvas; // Reference to the canvas GameObject that contains the next day button, set in the Unity Editor

    // This method is called when the script instance is being loaded. It implements the singleton pattern to ensure that only one instance of the StartNextDayScript exists in the scene. If an instance already exists, it destroys the new instance to prevent duplicates. The DontDestroyOnLoad method is called to ensure that the StartNextDayScript persists across scene transitions, allowing it to manage the next day button and canvas throughout the game.
    void Awake()
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

    // This method is called when the script instance is being enabled. It activates the StartNextDayCanvas, making it visible to the player when they are ready to proceed to the next day.
    public void isDayActiveBecomeTrue()
    {
        GameManager.Instance.isDayActive = true;
        StartNextDayCanvas.SetActive(false);
    }   

    // This method is called when the player clicks the next day button. It checks if the current day is greater than or equal to 6 and if all recalled conditions are met. If both conditions are true, it triggers the nurse twist by calling the TriggerNurseTwist method on the GameManager instance and hides the StartNextDayCanvas. If the conditions are not met, it loads the exterior scene to start the next day as usual.
    public void goToExteriorScene()
    {
        if (GameManager.Instance.currentDay >= 6 && GameManager.Instance.allRecalled)
        {
            Debug.Log("Twist Conditions Met: Cancelling Town trip, spawning Nurse.");
            
            StartNextDayCanvas.SetActive(false);
            
            GameManager.Instance.TriggerNurseTwist();
        }
        else
        {
            Debug.Log("Loading Exterior Scene...");
            SceneManager.LoadScene("ExteriorScene");
        }
    }
}