using UnityEngine;

public class MoneyBehaviourScript : MonoBehaviour
{
    public float moneyValue;
    [SerializeField]
    private Collider CashRegisterCollider;

    void Start()
    {
        CashRegisterCollider = FindObjectOfType<CashRegisterBehaviour>().GetComponent<Collider>();
        moneyValue = Random.Range(5f, 20f);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Money collided with: " + other.name);
        if (other == CashRegisterCollider)
        {
            CashRegisterBehaviour.Instance.amountInRegister += moneyValue;
            CashRegisterBehaviour.Instance.UpdateAmountInRegister();
            Destroy(gameObject);
        }
    }
}
