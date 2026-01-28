using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CashRegisterBehaviour : MonoBehaviour
{
    public static CashRegisterBehaviour Instance;

    NPCFlowerCheck npcFlowerCheck;

    private bool isLocked = true;
    public float amountInRegister = 0f;

    [SerializeField]
    private GameObject cashDrawer;

    [SerializeField]
    private ConfigurableJoint drawerJoint;

    [SerializeField]
    private Collider cashRegisterCollider;

    [SerializeField]
    private TextMeshProUGUI amountInRegisterText;

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

    public void UnlockRegister()
    {
        drawerJoint.xMotion = ConfigurableJointMotion.Limited;
    }

    public void LockRegister()
    {
        drawerJoint.xMotion = ConfigurableJointMotion.Locked;
    }

    public void OpenRegister()
    {
        Debug.Log("Attempting to open register...");
        GameObject money = GameObject.FindWithTag("Money");
        if (money != null && isLocked)
        {
            isLocked = false;
            cashDrawer.transform.localPosition = new Vector3(cashDrawer.transform.localPosition.x + 0.3f, cashDrawer.transform.localPosition.y, cashDrawer.transform.localPosition.z);
        }
        else
        {
            Debug.Log("No money to open the register.");
        }
    }

    void Update()
    {
        if (isLocked)
        {
            LockRegister();
        }
        else
        {
            UnlockRegister();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (cashRegisterCollider == other)
        {
            isLocked = true;
            npcFlowerCheck = FindObjectOfType<NPCFlowerCheck>();
            npcFlowerCheck.LeaveShop();
        }
    }

    public void UpdateAmountInRegister()
    {
        amountInRegisterText.text = "Amount in Register: $" + amountInRegister.ToString("F2");
    }
}
