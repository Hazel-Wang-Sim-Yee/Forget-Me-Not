using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoxBehaviourScript : MonoBehaviour
{
    public static BoxBehaviourScript Instance;
    public TextMeshPro boxLabel;
    public Animator boxAnimator;
    public GameObject flowersInside;

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

    void FlowerTypeInBox()
    {
        
    }
}
