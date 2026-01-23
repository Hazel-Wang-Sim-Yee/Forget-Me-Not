using UnityEngine;

public class CashierBehaviour : MonoBehaviour
{
    public GameObject drawer; 
    public bool holdingMoney = false;
    private bool isDrawerOpen = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            holdingMoney = true;
            Debug.Log("Holding Money!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            holdingMoney = false;
            Debug.Log("Money Dropped.");
        }
    }

    public void PressButton()
    {
        if (!isDrawerOpen)
        {
            drawer.transform.localPosition += new Vector3(0.3f, 0, 0); // Opens
            isDrawerOpen = true;
        }
        else
        {
            drawer.transform.localPosition -= new Vector3(0.3f, 0, 0); // Closes
            isDrawerOpen = false;
        }
    }
}