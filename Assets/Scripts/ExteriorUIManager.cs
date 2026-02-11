using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ExteriorUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject welcomeUI;
    public TMP_Text statusText; 
    public Button nextDayButton;

    void Start()
    {
        if (welcomeUI != null) welcomeUI.SetActive(true);
        
        if (statusText != null)
        {
            statusText.text = "Welcome to the village square. You're off the clock! Take a stroll and say hello to some familiar faces, or head straight back to work when you're ready.";
        }
    }

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