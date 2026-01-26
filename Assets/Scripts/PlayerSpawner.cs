using UnityEngine;
using Unity.XR.CoreUtils; // Required for XROrigin

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;

    private void Start()
    {
        if (spawnPoint != null)
        {
            XROrigin xrOrigin = GetComponent<XROrigin>();

            if (xrOrigin != null)
            {
                xrOrigin.MoveCameraToWorldLocation(spawnPoint.position);
                
                xrOrigin.MatchOriginUpCameraForward(spawnPoint.up, spawnPoint.forward);
            }
            else
            {
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
            }
        }
    }
}