using UnityEngine;

public class MoneySnap : MonoBehaviour 
{
    public Transform snapPoint;

    void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Money")) 
        {
            other.transform.SetParent(this.transform);

            other.transform.position = snapPoint.position;
            other.transform.rotation = snapPoint.rotation;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Money"))
        {
            other.transform.SetParent(null);
        }
    }
}