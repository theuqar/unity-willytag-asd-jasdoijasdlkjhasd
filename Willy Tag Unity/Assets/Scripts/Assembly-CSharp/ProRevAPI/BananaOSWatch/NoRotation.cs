using UnityEngine;

public class NoRotation : MonoBehaviour
{
    // This variable will hold the original rotation of the GameObject
    private Quaternion originalRotation;

    // This method will be called when the script is first enabled
    private void Awake()
    {
        // Save the original rotation of the GameObject
        originalRotation = transform.rotation;
    }

    // This method will be called every frame
    private void Update()
    {
        // Reset the rotation of the GameObject to its original rotation
        transform.rotation = originalRotation;
    }
}
