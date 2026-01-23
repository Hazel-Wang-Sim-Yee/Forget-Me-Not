using UnityEngine;

public class CashierBehaviour : MonoBehaviour
{
    public GameObject drawer; 
    private bool isDrawerOpen = false;

    public void PressButton()
    {
        if (!isDrawerOpen)
        {
            drawer.transform.localPosition += new Vector3(0.5f, 0, 0); 
            isDrawerOpen = true;
        }
        else
        {
            drawer.transform.localPosition -= new Vector3(0.5f, 0, 0);
            isDrawerOpen = false;
        }
    }
}