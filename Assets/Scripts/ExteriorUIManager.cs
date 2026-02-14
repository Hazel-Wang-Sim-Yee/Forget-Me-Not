/*
* Author: Jeffrey
* Date: 2026-02-11
* Description: Manages the UI elements in the exterior scene. It handles showing the welcome UI, updating status text, and enabling/disabling the next day button based on the current day.
*/
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ExteriorUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject welcomeUI; // Container for the welcome UI elements, set in the Unity Editor
    public TMP_Text statusText; // Text component for displaying status messages to the player, set in the Unity Editor
    public Button nextDayButton; // Button for proceeding to the next day, set in the Unity Editor

    // This method is called when the script instance is being loaded. It shows the welcome UI and updates the status text with a message for the player. The next day button is enabled by default, allowing the player to proceed to the next day when they are ready.
    void Start()
    {
        if (welcomeUI != null) welcomeUI.SetActive(true);
        
        if (statusText != null)
        {
            statusText.text = "Welcome to the village square. You're off the clock! Take a stroll and say hello to some familiar faces, or head straight back to work when you're ready.";
        }
    }

    // This method is called when the player clicks the next day button. It checks if the current day is greater than or equal to 6, and if so, it updates the status text with a message and disables the next day button to prevent further progression. If the current day is less than 6, it loads the main game scene to start the next day.
    public void LoadNextDay()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay >= 6)
        {
            statusText.text = "The shop can wait. Some familiar faces are wandering nearby... perhaps you should say hello before heading back to work?";
            
            if (nextDayButton != null)
            {
                nextDayButton.interactable = false;
            }

            Debug.Log("Day 6+ reached. Next Day button has been disabled.");
            return; 
        }

        SceneManager.LoadScene("MainGameScene");
    }


}