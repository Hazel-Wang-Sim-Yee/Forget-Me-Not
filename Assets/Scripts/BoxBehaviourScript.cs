using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoxBehaviourScript : MonoBehaviour
{
    public static BoxBehaviourScript Instance;
    FlowerInBoxScript flowerInBoxScript;
    public TextMeshProUGUI boxLabel;
    public GameObject flowersInside;
    [SerializeField]
    GameObject leftBoxFlap;
    [SerializeField]
    GameObject rightBoxFlap;
    [SerializeField]
    GameObject boxLabelObject;
    [SerializeField]
    public GameObject CarnationsFlowers;
    [SerializeField]
    public GameObject TulipsFlowers;
    [SerializeField]
    public GameObject DaisiesFlowers;
    [SerializeField]
    public GameObject LiliesFlowers;
    bool hasFlowers = false;

    GameObject flowersInsideGrp;

    public void UpdateBoxLabel(string labelText)
    {
        boxLabel.text = labelText;
    }

    public void OpenBoxFlaps()
    {
        boxLabelObject.SetActive(false);
        leftBoxFlap.transform.Rotate(Vector3.forward, -240);
        rightBoxFlap.transform.Rotate(Vector3.forward, 240);
        if (!hasFlowers)
        {
            hasFlowers = true;
            FlowerTypeInBox(flowersInside);
        }
    }

    public void FlowerTypeInBox(GameObject flowerType)
    {
        if (flowerType == CarnationsFlowers)
        {
            flowersInsideGrp = Instantiate(CarnationsFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == TulipsFlowers)
        {
            flowersInsideGrp = Instantiate(TulipsFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == DaisiesFlowers)
        {
            flowersInsideGrp = Instantiate(DaisiesFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }
        else if (flowerType == LiliesFlowers)
        {
            flowersInsideGrp = Instantiate(LiliesFlowers, new Vector3(transform.position.x - 0.28f, transform.position.y + 0.3f, transform.position.z + 0.2f), transform.rotation, transform) as GameObject;
        }

        flowerInBoxScript = flowersInsideGrp.GetComponent<FlowerInBoxScript>();
        flowerInBoxScript.box = this.gameObject;
    }
}
