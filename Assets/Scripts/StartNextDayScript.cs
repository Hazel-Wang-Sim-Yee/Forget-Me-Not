using UnityEngine;
using UnityEngine.UI;

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
        Debug.Log("Starting next day...");
        GameManager.Instance.isDayActive = true;
        StartNextDayCanvas.SetActive(false);
        Debug.Log("isDayActive set to true");
    }   
}
