using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SyncScale : NetworkBehaviour
{
    // Declare a NetworkVariable to hold the scale of the GameObject
    public NetworkVariable<Vector3> networkScale = new NetworkVariable<Vector3>(new Vector3(1, 1, 1)); // Default scale is (1, 1, 1)

    private void Start()
    {
        // When the network scale changes, update the local GameObject scale
        networkScale.OnValueChanged += OnScaleChanged;
    }

    private void Update()
    {
        // Only the server should update the scale
        if (IsServer)
        {
            // For example, let's scale up the object over time (you can replace this with your logic)
            float scaleAmount = Mathf.PingPong(Time.time, 2); // Will oscillate between 0 and 2
            Vector3 newScale = new Vector3(scaleAmount, scaleAmount, scaleAmount);

            // Set the NetworkVariable to sync the scale across clients
            networkScale.Value = newScale;
        }
    }

    // Callback function to update the local GameObject's scale when the network scale changes
    private void OnScaleChanged(Vector3 oldScale, Vector3 newScale)
    {
        StartCoroutine(SmoothScaleTransition(newScale));
    }

    private IEnumerator SmoothScaleTransition(Vector3 targetScale)
    {
        float timeToSmooth = 0.2f; // Time in seconds for the smoothing effect
        Vector3 initialScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < timeToSmooth)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / timeToSmooth);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale; // Ensure the final scale is exactly the target
    }
}
