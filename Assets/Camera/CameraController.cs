using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;  // Reference to the player's Transform
    public Vector3 offset;    // Offset position of the camera from the player
    public float smoothSpeed = 0.125f;  // How smooth the camera movement is

    void LateUpdate()
    {
        // Calculate the target position for the camera
        Vector3 desiredPosition = player.position + offset;

        // Smoothly move the camera to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Set the camera's position to the smoothed position
        transform.position = smoothedPosition;
    }
}
