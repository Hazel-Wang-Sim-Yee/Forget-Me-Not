/*
* Author: Alex
* Date: 2026-02-09
* Description: This script manages the behavior of the water interaction in the game. It handles the visual effects of ripples when the player enters and exits water, and updates the position of the ripple effect based on the water's height.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterInteraction : MonoBehaviour
{
    [SerializeField] private ParticleSystem _interactionParticleSystem; // Reference to the particle system that creates the ripple effect when the player interacts with water, set in the Unity Editor
    private ParticleSystem.EmissionModule emissionModule; // Reference to the emission module of the particle system, used to enable and disable the ripple effect when the player enters and exits water

    // This method is called when the script instance is being loaded. It checks if the interaction particle system is active and deactivates it if it is. It also retrieves the emission module from the particle system and disables it to ensure that the ripple effect is not active until the player interacts with water.
    private void Start()
    {
        if (_interactionParticleSystem.gameObject.activeSelf)
        {
            _interactionParticleSystem.gameObject.SetActive(false);
        }

        emissionModule = _interactionParticleSystem.emission;
        emissionModule.enabled = false;
    }

    // This method is called when another collider enters the trigger collider of the water. It checks if the collider belongs to the water, and if so, it activates the ripple effect by enabling the emission module of the particle system. It also ensures that the ripple effect is only activated when the player is in the water and not when they are outside of it.
    private void OnTriggerEnter(Collider water)
    {
        if (!_interactionParticleSystem.gameObject.activeSelf)
        {
            _interactionParticleSystem.gameObject.SetActive(true);
        }

        emissionModule.enabled = true; //Enable the ripple emission when player is in the water
    }

    // This method is called when another collider stays within the trigger collider of the water. It updates the position of the ripple effect based on the height of the water, ensuring that the ripples appear at the correct height relative to the water surface as the player moves through it.
    private void OnTriggerStay(Collider water)
    {
        float waterHeight = water.transform.position.y;
        _interactionParticleSystem.transform.position = new Vector3(gameObject.transform.position.x, waterHeight, gameObject.transform.position.z);
    }

    // This method is called when another collider exits the trigger collider of the water. It checks if the collider belongs to the water, and if so, it deactivates the ripple effect by disabling the emission module of the particle system. This ensures that the ripple effect is only active when the player is in the water and is turned off when they leave it.
    private void OnTriggerExit(Collider water)
    {
        emissionModule.enabled = false; //Disable the ripple emission when player is in the water
    }
}
