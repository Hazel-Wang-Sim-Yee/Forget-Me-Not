using UnityEngine;

public class MoneyBehaviourScript : MonoBehaviour
{
    public float moneyValue;
    [SerializeField]
    private Collider CashRegisterCollider;

    NPCFlowerCheck npcFlowerCheck;

    void Start()
    {
        CashRegisterCollider = FindObjectOfType<CashRegisterBehaviour>().GetComponent<Collider>();
        npcFlowerCheck = NPCFlowerCheck.Instance;
        if (npcFlowerCheck.heldFlowerType == "Bouquet(s)")
        {
            moneyValue = 5f;
        }
        if (npcFlowerCheck.heldFlowerType == "Bouquet(m)")
        {
            moneyValue = 12f;
        }
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
