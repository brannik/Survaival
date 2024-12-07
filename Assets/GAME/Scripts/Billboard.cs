using UnityEngine;
using Cinemachine;

public class Billboard : MonoBehaviour
{
    private Camera localCamera;

    private void Start()
    {
        // Find the main camera or Cinemachine brain's camera
        FindLocalCamera();
    }

    private void LateUpdate()
    {
        if (localCamera == null)
        {
            // Retry finding the camera if it hasn't been assigned yet
            FindLocalCamera();
            if (localCamera == null) return;
        }

        // Calculate the direction to face the camera
        Vector3 directionToCamera = localCamera.transform.position - transform.position;

        // Apply the rotation to face the camera with an offset to correct for any inversion
        Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera.normalized);
        if(gameObject.tag.Equals("PlayerName")){
            transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0); // Example offset
        }else{
            transform.rotation = lookRotation;
        }
        
    }

    private void FindLocalCamera()
    {
        // First, try to get the active camera from the Cinemachine Brain
        CinemachineBrain brain = FindAnyObjectByType<CinemachineBrain>();
        if (brain != null && brain.OutputCamera != null)
        {
            localCamera = brain.OutputCamera;
        }
        else
        {
            // Fallback to the standard Camera.main
            localCamera = Camera.main;
        }
    }
}
