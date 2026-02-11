using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartNextDayScript : MonoBehaviour
{
    public static StartNextDayScript Instance;
    public GameObject StartNextDayCanvas;

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

    public void isDayActiveBecomeTrue()
    {
        GameManager.Instance.isDayActive = true;
        StartNextDayCanvas.SetActive(false);
    }   

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