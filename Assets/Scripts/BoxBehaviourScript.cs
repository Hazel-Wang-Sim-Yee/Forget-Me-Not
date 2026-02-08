using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoxBehaviourScript : MonoBehaviour
{
    public static BoxBehaviourScript Instance;
    public TextMeshProUGUI boxLabel;
    public Animator boxAnimator;
    public GameObject flowersInside;

    void Start()
    {
        boxAnimator = GetComponent<Animator>();
        boxAnimator.SetBool("isGrabbed", false);
    }

    public void UpdateBoxLabel(string labelText)
    {
        boxLabel.text = labelText;
    }

    void OnGrabbed()
    {
        boxAnimator.SetBool("isGrabbed", true);
    }

    void OnReleased()
    {
        boxAnimator.SetBool("isGrabbed", false);
    }

    public void FlowerTypeInBox(string flowerType)
    {
        flowersInside = Resources.Load<GameObject>(flowerType);
        Instantiate(flowersInside, transform.position, transform.rotation, transform);
    }
}
